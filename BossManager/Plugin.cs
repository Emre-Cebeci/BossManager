using Terraria;
using Terraria.GameContent.Events;
using TerrariaApi.Server;
using TShockAPI;
using Mono.Cecil.Cil;
using Newtonsoft.Json;
using MonoMod.Cil;
using TShockAPI.Hooks;

namespace BossManager
{
    [ApiVersion(2, 1)]
    public partial class Plugin : TerrariaPlugin
    {
        public override void Initialize()
        {
            IL.Terraria.NPC.NewNPC += OnNewNPC;
            IL.Terraria.NPC.SpawnWOF += OnSpawnWOF;
            ServerApi.Hooks.ServerJoin.Register(this, OnJoin);
            ServerApi.Hooks.NetGetData.Register(this, OnNetGetData);
            GeneralHooks.ReloadEvent += OnReload;

            Commands.ChatCommands.Add(new Command("bossmgr.undoboss", UndoBossCommand, "undoboss", "uboss")
            {
                HelpText = "Toggle a boss defeated state."
            });

            Commands.ChatCommands.Add(new Command("bossmgr.listboss", ListBossCommand, "listboss", "lboss", "bosses")
            {
                HelpText = "List defeated bosses and events."
            });

            Commands.ChatCommands.Add(new Command("bossmgr.enableboss", EnableBossCommand, "enableboss", "enblb")
            {
                HelpText = "Toggle a boss enabled state."
            });
        }

        private void OnNewNPC(ILContext ctx)
        {
            //We are gonna patch Terraria's NPC.NewNPC() to prevent bosses from spawning by using IL editing.
            //IL editing is a powerful (and painful) way to inject our code into Terraria's code. Thus change how it works,
            //but IL editing will not work for all hooks in multiplayer and any plugins that use the same method you patched will also be affected when this plugin is running.
            //If you wanna learn more about IL editing: https://github.com/tModLoader/tModLoader/wiki/Expert-IL-Editing
            //I suggest taking a look at NPC.NewNPC() code in dnSpy to have a better understanding what i'm talking about below.

            //Important Note: We will let Mechdusa bypass the IsBossSpawnable() in this hook because if we didn't, there would be
            //an error which is really hard to fix, its much easier to let packet 61 handle Mechdusa spawn.

            TShock.Log.ConsoleInfo("[BossMGR]: Patching NPC.NewNPC()...");
            //Create a new IL cursor that pointing at index 0
            ILCursor csr = new ILCursor(ctx);
            //Finding the index of NPC.GetAvailableNPCSlot()
            csr.GotoNext(MoveType.Before, i => i.MatchCall<NPC>(nameof(NPC.GetAvailableNPCSlot)));
            //Adjust the index to where we want to inject our code
            csr.Index -= 2;
            //Escape the Main.getGoodWorld condition or this hook would only work in ftw world
            csr.MoveAfterLabels();
            //We will try to inject the code below:

            /*
            if (Type != 127 && Type != 125 && Type != 126 && Type != 134)
                IsBossSpawnable(Type)...
            else
            {
                if (Main.zenithWorld)
                    Let them spawn
                else
                    IsBossSpawnable(Type)...
            }
            */

            //Create a new label, remember that the cursor is now pointing at Ldarg.3 Opcode which is used for NPC.GetAvaliableNPCSlot()
            //because of that, we will use this label to jump to that method if the IsBossSpawnable() return 0 (Success)
            ILLabel passedTheCheck = csr.DefineLabel();
            //Also create a new label, the purpose of this label is we are gonna use it to jump to the first else statement
            //in the code above. This label's position is also pointing at Ldarg.3 Opcode.
            ILLabel jumpToElseStatement = csr.DefineLabel();
            //Push Type (aka NPCID) into stack
            csr.Emit(OpCodes.Ldarg_3);
            //Push 127 (Skeletron Prime's ID) into stack 
            csr.Emit(OpCodes.Ldc_I4, 127);
            //Compare if these 2 values are equal, proceed jumping to the first else statement
            csr.Emit(OpCodes.Beq_S, jumpToElseStatement);
            //Push Type into stack
            csr.Emit(OpCodes.Ldarg_3);
            //Push 125 (Retinazer's ID) into stack
            csr.Emit(OpCodes.Ldc_I4, 125);
            //Compare if these 2 values are equal, proceed jumping to the first else statement
            csr.Emit(OpCodes.Beq_S, jumpToElseStatement);
            //Push Type into stack
            csr.Emit(OpCodes.Ldarg_3);
            //Push 126 (Spazmatism's ID) into stack
            csr.Emit(OpCodes.Ldc_I4, 126);
            //Compare if these 2 values are equal, proceed jumping to the first else statement
            csr.Emit(OpCodes.Beq_S, jumpToElseStatement);
            //Push Type into stack
            csr.Emit(OpCodes.Ldarg_3);
            //Push 134 (Destroyer's ID) into stack
            csr.Emit(OpCodes.Ldc_I4, 134);
            //Compare if these 2 values are equal, proceed jumping to the first else statement
            csr.Emit(OpCodes.Beq_S, jumpToElseStatement);

            //If they are not equal, procceed execute the code inside if statement
            //Brackets here because i couldn't think of any other variable name
            {
                //Create a new label, this label's current position is at the Opcode after the Beq_S Opcode
                ILLabel isIllegalSpawning = csr.DefineLabel();
                //Push Type into stack
                csr.Emit(OpCodes.Ldarg_3);
                //Call IsBossSpawnable() and push the return value into stack
                csr.Emit(OpCodes.Call, typeof(Plugin).GetMethod(nameof(IsBossSpawnable)));
                //Push 1 into stack, 1 = BossSpawnAttemptResultState.IllegalSpawning
                csr.Emit(OpCodes.Ldc_I4_1);
                //Compare if these 2 values are not equal, proceed jump to the next check
                csr.Emit(OpCodes.Bne_Un_S, isIllegalSpawning);
                //If they are equal, that means it is an illegal spawning, push IEntitySource into stack
                csr.Emit(OpCodes.Ldarg_0);
                //Push Type into stack
                csr.Emit(OpCodes.Ldarg_3);
                //Call AnnounceIllegalSpawning(), uses 2 values we pushed for its arguments
                csr.Emit(OpCodes.Call, typeof(Plugin).GetMethod(nameof(AnnounceIllegalSpawning)));
                //Push IEntitySource into stack again
                csr.Emit(OpCodes.Ldarg_0);
                //Push Type into stack
                csr.Emit(OpCodes.Ldarg_3);
                //Call GiveBackSummoningItem(), uses 2 values we pushed for its arguments
                csr.Emit(OpCodes.Call, typeof(Plugin).GetMethod(nameof(GiveBackSummoningItem)));
                //Push 200 into stack
                csr.Emit(OpCodes.Ldc_I4, 200);
                //Return 200 because plugin rejected boss spawning
                csr.Emit(OpCodes.Ret);
                //Set the label's stream position to the next cursor's position, mark it as the end of the first case.
                csr.MarkLabel(isIllegalSpawning);

                //Next case: NotEnoughPlayers, pretty much do the same as above.
                ILLabel isNotEnoughPlayers = csr.DefineLabel();
                csr.Emit(OpCodes.Ldarg_3);
                csr.Emit(OpCodes.Call, typeof(Plugin).GetMethod(nameof(IsBossSpawnable)));
                csr.Emit(OpCodes.Ldc_I4_2);
                csr.Emit(OpCodes.Bne_Un_S, isNotEnoughPlayers);
                csr.Emit(OpCodes.Ldarg_0);
                csr.Emit(OpCodes.Ldarg_3);
                csr.Emit(OpCodes.Call, typeof(Plugin).GetMethod(nameof(AnnounceNotEnoughPlayers)));
                csr.Emit(OpCodes.Ldarg_0);
                csr.Emit(OpCodes.Ldarg_3);
                csr.Emit(OpCodes.Call, typeof(Plugin).GetMethod(nameof(GiveBackSummoningItem)));
                csr.Emit(OpCodes.Ldc_I4, 200);
                csr.Emit(OpCodes.Ret);
                csr.MarkLabel(isNotEnoughPlayers);

                ILLabel isNotAllowed = csr.DefineLabel();
                csr.Emit(OpCodes.Ldarg_3);
                csr.Emit(OpCodes.Call, typeof(Plugin).GetMethod(nameof(IsBossSpawnable)));
                csr.Emit(OpCodes.Ldc_I4_3);
                csr.Emit(OpCodes.Bne_Un_S, isNotAllowed);
                csr.Emit(OpCodes.Ldarg_0);
                csr.Emit(OpCodes.Ldarg_3);
                csr.Emit(OpCodes.Call, typeof(Plugin).GetMethod(nameof(AnnounceNotAllowed)));
                csr.Emit(OpCodes.Ldarg_0);
                csr.Emit(OpCodes.Ldarg_3);
                csr.Emit(OpCodes.Call, typeof(Plugin).GetMethod(nameof(GiveBackSummoningItem)));
                csr.Emit(OpCodes.Ldc_I4, 200);
                csr.Emit(OpCodes.Ret);
                csr.MarkLabel(isNotAllowed);
            }

            //After the check, the cursor position is now pointing to somewhere i don't know, but i pretty sure that it escaped
            //the first if statement.
            //Mark this as the end of the first if statement, now we are ready to create the first else statement
            csr.MarkLabel(jumpToElseStatement);

            //Create a new label to check if the current world is a zenith world or not, i don't know where is the cursor pointing at
            //but i just need to know the cursor escaped the first if brackets
            ILLabel notAZenithWorld = csr.DefineLabel();
            //ILLabel jumpToNestedElseStatement = csr.DefineLabel();
            //Load Main.zenithWorld into stack
            csr.Emit(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.zenithWorld)));
            //Check if the current world is not zenith world, procceed jumping to where notAZenithWorld label is located and proceed
            //to the nested else statement
            csr.Emit(OpCodes.Brfalse_S, notAZenithWorld);
            //If not, then we are sure that they are trying to summon Mechdusa or any other mech bosses except Skeletron Prime
            //which is impossible to do unless they are hacking.
            //Assume that they are not hacking, we will let Mechdusa bypass this hook only.
            csr.Emit(OpCodes.Br_S, passedTheCheck);

            //Set the location of notAZenithWorld label to the end of the nested if statement,
            csr.MarkLabel(notAZenithWorld);
            //Create a new label for nested else statement
            ILLabel nestedElseStatement = csr.DefineLabel();
            {
                //IsBossSpawnable()...
                ILLabel isIllegalSpawning = csr.DefineLabel();
                csr.Emit(OpCodes.Ldarg_3);
                csr.Emit(OpCodes.Call, typeof(Plugin).GetMethod(nameof(IsBossSpawnable)));
                csr.Emit(OpCodes.Ldc_I4_1);
                csr.Emit(OpCodes.Bne_Un_S, isIllegalSpawning);
                csr.Emit(OpCodes.Ldarg_0);
                csr.Emit(OpCodes.Ldarg_3);
                csr.Emit(OpCodes.Call, typeof(Plugin).GetMethod(nameof(AnnounceIllegalSpawning)));
                csr.Emit(OpCodes.Ldarg_0);
                csr.Emit(OpCodes.Ldarg_3);
                csr.Emit(OpCodes.Call, typeof(Plugin).GetMethod(nameof(GiveBackSummoningItem)));
                csr.Emit(OpCodes.Ldc_I4, 200);
                csr.Emit(OpCodes.Ret);
                csr.MarkLabel(isIllegalSpawning);

                ILLabel isNotEnoughPlayers = csr.DefineLabel();
                csr.Emit(OpCodes.Ldarg_3);
                csr.Emit(OpCodes.Call, typeof(Plugin).GetMethod(nameof(IsBossSpawnable)));
                csr.Emit(OpCodes.Ldc_I4_2);
                csr.Emit(OpCodes.Bne_Un_S, isNotEnoughPlayers);
                csr.Emit(OpCodes.Ldarg_0);
                csr.Emit(OpCodes.Ldarg_3);
                csr.Emit(OpCodes.Call, typeof(Plugin).GetMethod(nameof(AnnounceNotEnoughPlayers)));
                csr.Emit(OpCodes.Ldarg_0);
                csr.Emit(OpCodes.Ldarg_3);
                csr.Emit(OpCodes.Call, typeof(Plugin).GetMethod(nameof(GiveBackSummoningItem)));
                csr.Emit(OpCodes.Ldc_I4, 200);
                csr.Emit(OpCodes.Ret);
                csr.MarkLabel(isNotEnoughPlayers);

                ILLabel isNotAllowed = csr.DefineLabel();
                csr.Emit(OpCodes.Ldarg_3);
                csr.Emit(OpCodes.Call, typeof(Plugin).GetMethod(nameof(IsBossSpawnable)));
                csr.Emit(OpCodes.Ldc_I4_3);
                csr.Emit(OpCodes.Bne_Un_S, isNotAllowed);
                csr.Emit(OpCodes.Ldarg_0);
                csr.Emit(OpCodes.Ldarg_3);
                csr.Emit(OpCodes.Call, typeof(Plugin).GetMethod(nameof(AnnounceNotAllowed)));
                csr.Emit(OpCodes.Ldarg_0);
                csr.Emit(OpCodes.Ldarg_3);
                csr.Emit(OpCodes.Call, typeof(Plugin).GetMethod(nameof(GiveBackSummoningItem)));
                csr.Emit(OpCodes.Ldc_I4, 200);
                csr.Emit(OpCodes.Ret);
                csr.MarkLabel(isNotAllowed);
            }

            csr.MarkLabel(nestedElseStatement);
            csr.MarkLabel(passedTheCheck);
        }

        private void OnSpawnWOF(ILContext ctx)
        {
            //WOF has an exclusive spawning method so his spawn message will still be shown up even though we successfully prevented him from spawning.
            //This hook will disable WOF's spawn message, simply return immediately before the spawn message executed as NPC.NewNPC() did all the hard job for us.

            TShock.Log.ConsoleInfo("[BossMGR]: Patching NPC.SpawnWOF()...");
            ILCursor csr = new ILCursor(ctx);
            csr.GotoNext(MoveType.After, i => i.MatchCall<NPC>(nameof(NPC.NewNPC)));
            csr.Index++;
            csr.Emit(OpCodes.Ret);
        }

        private void OnJoin(JoinEventArgs args)
        {
            if (Config.AllowJoinDuringBoss) 
                return;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].boss)
                {
                    TShock.Players[args.Who].Disconnect("The in-game players must defeat the current boss\nbefore you can join.");
                    return;
                }
            }
        }

        private void OnNetGetData(GetDataEventArgs args)
        {
            //This packet will only be fired when a player uses a boss summoning item.
            //It will not be fired if a boss spawns naturally or through doing some various processes.
            //So its safe to give back player the summoning item if the plugin rejects the boss spawning.
            if (args.MsgID == PacketTypes.SpawnBossorInvasion)
            {
                using (BinaryReader br = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
                {
                    short playerID = br.ReadInt16();
                    short type = br.ReadInt16();

                    if (type == -16) //Mechdusa
                    {
                        switch (IsBossSpawnable(127))
                        {
                            case BossSpawnAttemptResultState.IllegalSpawning:
                                AnnounceIllegalSpawning(NPC.GetBossSpawnSource(playerID), 127);
                                TShock.Players[playerID].GiveItem(GetSummoningItemFromBossNetID(127), 1);
                                args.Handled = true;
                                return;
                            case BossSpawnAttemptResultState.NotEnoughPlayers:
                                AnnounceNotEnoughPlayers(NPC.GetBossSpawnSource(playerID), 127);
                                TShock.Players[playerID].GiveItem(GetSummoningItemFromBossNetID(127), 1);
                                args.Handled = true;
                                return;
                            case BossSpawnAttemptResultState.NotAllowed:
                                AnnounceNotAllowed(NPC.GetBossSpawnSource(playerID), 127);
                                TShock.Players[playerID].GiveItem(GetSummoningItemFromBossNetID(127), 1);
                                args.Handled = true;
                                return;

                            default:
                                args.Handled = false;
                                return;
                        }
                    }
                }
            }
        }

        private void OnReload(ReloadEventArgs args)
        {
            Config = Config.Read();
            args.Player.SendInfoMessage("BossManager has been reloaded.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                IL.Terraria.NPC.NewNPC -= OnNewNPC;
                IL.Terraria.NPC.SpawnWOF -= OnSpawnWOF;
                ServerApi.Hooks.ServerJoin.Deregister(this, OnJoin);
                ServerApi.Hooks.NetGetData.Deregister(this, OnNetGetData);
                GeneralHooks.ReloadEvent -= OnReload;
            }

            base.Dispose(disposing);
        }

        private void ListBossCommand(CommandArgs args)
        {
            var BossList = new List<string>();
            {
                if (NPC.downedSlimeKing)
                    BossList.Add("King Slime");

                if (NPC.downedBoss1)
                    BossList.Add("Eye of Cthulhu");

                if (NPC.downedBoss2)
                {
                    if (WorldGen.crimson)
                        BossList.Add("Brain of Cthulhu");
                    else
                        BossList.Add("Eater of Worlds");
                }

                if (NPC.downedDeerclops)
                    BossList.Add("Deerclops");

                if (NPC.downedBoss3)
                    BossList.Add("Skeletron");

                if (NPC.downedQueenBee)
                    BossList.Add("Queen Bee");

                if (Main.hardMode)
                    BossList.Add("Wall of Flesh");

                if (NPC.downedQueenSlime)
                    BossList.Add("Queen Slime");

                if (NPC.downedMechBoss1)
                    BossList.Add("The Destroyer");

                if (NPC.downedMechBoss2)
                    BossList.Add("The Twins");

                if (NPC.downedMechBoss3)
                    BossList.Add("Skeletron Prime");

                if (NPC.downedPlantBoss)
                    BossList.Add("Plantera");

                if (NPC.downedGolemBoss)
                    BossList.Add("Golem");

                if (NPC.downedFishron)
                    BossList.Add("Duke Fishron");

                if (DD2Event.DownedInvasionT3)
                    BossList.Add("Betsy");

                if (NPC.downedEmpressOfLight)
                    BossList.Add("Empress of Light");

                if (NPC.downedAncientCultist)
                    BossList.Add("Lunatic Cultist");

                if (NPC.downedMoonlord)
                    BossList.Add("Moon Lord");
            }
            var EventList = new List<string>();
            {
                if (NPC.downedGoblins)
                    EventList.Add("Goblin Army");

                if (DD2Event.DownedInvasionT1)
                    EventList.Add("Old One's Army T1");

                if (NPC.downedPirates)
                    EventList.Add("Pirate Invasion");

                if (NPC.downedClown)
                    EventList.Add("Blood Moon");

                if (DD2Event.DownedInvasionT2)
                    EventList.Add("Old One's Army T2");

                if (NPC.downedFrost)
                    EventList.Add("Frost Legion");

                if (NPC.downedMartians)
                    EventList.Add("Martian Invasion");

                if (DD2Event.DownedInvasionT3)
                    EventList.Add("Old One's Army T3");

                if (NPC.downedHalloweenTree)
                    EventList.Add("Pumpkin Moon");

                if (NPC.downedChristmasTree)
                    EventList.Add("Frost Moon");

                if (NPC.downedTowerNebula && NPC.downedTowerSolar && NPC.downedTowerStardust && NPC.downedTowerVortex)
                    EventList.Add("The Pillars");
            }

            if (String.IsNullOrEmpty(String.Join(", ", BossList)))
                args.Player.SendInfoMessage("No bosses have been defeated so far...");
            else
                args.Player.SendInfoMessage($"[c/ffc500:Defeated Bosses:] {String.Join(", ", BossList)}");


            if (String.IsNullOrEmpty(String.Join(", ", EventList)))
                args.Player.SendInfoMessage("No events have been defeated so far...");
            else
                args.Player.SendInfoMessage($"[c/ffc500:Defeated Events:] {String.Join(", ", EventList)}");
        }

        private void UndoBossCommand(CommandArgs args)
        {
            string subcommand = args.Parameters.Count == 0 ? "undoboss" : args.Parameters[0].ToLower();

            switch (subcommand)
            {
                case "kingslime":
                case "king":
                case "ks":
                    {
                        NPC.downedSlimeKing = !NPC.downedSlimeKing;
                        args.Player.SendInfoMessage($"Set King Slime as {(NPC.downedSlimeKing ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                        return;
                    }

                case "eyeofcthulhu":
                case "eye":
                case "eoc":
                    {
                        NPC.downedBoss1 = !NPC.downedBoss1;
                        args.Player.SendInfoMessage($"Set Eye of Cthulhu as {(NPC.downedBoss1 ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                        return;
                    }

                case "evilboss":
                case "boc":
                case "eow":
                case "eaterofworlds":
                case "brainofcthulhu":
                case "brain":
                case "eater":
                    {
                        NPC.downedBoss2 = !NPC.downedBoss2;
                        args.Player.SendInfoMessage($"Set {(WorldGen.crimson ? "Brain of Cthulhu" : "Eater of Worlds")} as {(NPC.downedBoss2 ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                        return;
                    }

                case "deerclops":
                case "deer":
                case "dc":
                    {
                        NPC.downedDeerclops = !NPC.downedDeerclops;
                        args.Player.SendInfoMessage($"Set Deerclops as {(NPC.downedDeerclops ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                        return;
                    }

                case "skeletron":
                case "sans":
                    {
                        NPC.downedBoss3 = !NPC.downedBoss3;
                        args.Player.SendInfoMessage($"Set Skeletron as {(NPC.downedBoss3 ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                        return;
                    }

                case "queenbee":
                case "qb":
                    {
                        NPC.downedQueenBee = !NPC.downedQueenBee;
                        args.Player.SendInfoMessage($"Set Queen Bee as {(NPC.downedQueenBee ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                        return;
                    }

                case "hardmode":
                case "wallofflesh":
                case "wof":
                    {
                        Main.hardMode = !Main.hardMode;
                        args.Player.SendInfoMessage($"Set Wall of Flesh (Hardmode) as {(Main.hardMode ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                        args.Player.SendInfoMessage("Note: This is the same as the '/hardmode' command.");
                        return;
                    }

                case "queenslime":
                case "qs":
                    {
                        NPC.downedQueenSlime = !NPC.downedQueenSlime;
                        args.Player.SendInfoMessage($"Set Queen Slime as {(NPC.downedQueenSlime ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                        return;
                    }

                case "mech1":
                case "thedestroyer":
                case "destroyer":
                    {
                        NPC.downedMechBoss1 = !NPC.downedMechBoss1;
                        args.Player.SendInfoMessage($"Set The Destroyer as {(NPC.downedMechBoss1 ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                        return;
                    }

                case "mech2":
                case "thetwins":
                case "twins":
                    {
                        NPC.downedMechBoss2 = !NPC.downedMechBoss2;
                        args.Player.SendInfoMessage($"Set The Twins as {(NPC.downedMechBoss2 ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                        return;
                    }

                case "mech3":
                case "skeletronprime":
                case "prime":
                    {
                        NPC.downedMechBoss3 = !NPC.downedMechBoss3;
                        args.Player.SendInfoMessage($"Set Skeletron Prime as {(NPC.downedMechBoss3 ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                        return;
                    }

                case "plantera":
                    {
                        NPC.downedPlantBoss = !NPC.downedPlantBoss;
                        args.Player.SendInfoMessage($"Set Plantera as {(NPC.downedPlantBoss ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                        return;
                    }

                case "golem":
                    {
                        NPC.downedGolemBoss = !NPC.downedGolemBoss;
                        args.Player.SendInfoMessage($"Set Golem as {(NPC.downedGolemBoss ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                        return;
                    }

                case "duke":
                case "fishron":
                case "dukefishron":
                    {
                        NPC.downedFishron = !NPC.downedFishron;
                        args.Player.SendInfoMessage($"Set Duke Fishron as {(NPC.downedFishron ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                        return;
                    }

                case "cultist":
                case "lunatic":
                case "lunaticcultist":
                    {
                        NPC.downedAncientCultist = !NPC.downedAncientCultist;
                        args.Player.SendInfoMessage($"Set Lunatic Cultist as {(NPC.downedAncientCultist ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                        return;
                    }

                case "betsy":
                    {
                        DD2Event.DownedInvasionT3 = !DD2Event.DownedInvasionT3;
                        args.Player.SendInfoMessage($"Set Betsy as {(DD2Event.DownedInvasionT3 ? "[c/FF0000: Killed" : "[c/00FF00: Not Killed]!")}");
                        return;
                    }

                case "empress":
                case "eol":
                case "empressoflight":
                    {
                        NPC.downedEmpressOfLight = !NPC.downedEmpressOfLight;
                        args.Player.SendInfoMessage($"Set Empress of Light as {(NPC.downedEmpressOfLight ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                        return;
                    }

                case "moonlord":
                case "ml":
                    {
                        NPC.downedMoonlord = !NPC.downedMoonlord;
                        args.Player.SendInfoMessage($"Set Moonlord as {(NPC.downedMoonlord ? "[c/FF0000:Killed]" : "[c/00FF00:Not Killed]")}!");
                        return;
                    }

                default:
                    {
                        args.Player.SendErrorMessage("Please specify which boss to toggle!");
                        args.Player.SendInfoMessage("eg. /undoboss king - toggle king slime");
                        return;
                    }
            }

        }

        private void EnableBossCommand(CommandArgs args)
        {
            string subcommand = args.Parameters.Count == 0 ? "undoboss" : args.Parameters[0].ToLower();

            switch (subcommand)
            {
                case "kingslime":
                case "king":
                case "ks":
                    ToggleBoss(ref Config.AllowKingSlime, args, "King Slime");
                    return;

                case "eyeofcthulhu":
                case "eye":
                case "eoc":
                    ToggleBoss(ref Config.AllowEyeOfCthulhu, args, "Eye of Cthulhu");
                    return;

                case "evilboss":
                case "boc":
                case "eow":
                case "eaterofworlds":
                case "brainofcthulhu":
                case "brain":
                case "eater":
                    ToggleBoss(ref Config.AllowEaterOfWorlds, args, WorldGen.crimson ? "Brain of Cthulhu" : "Eater of Worlds");
                    ToggleBoss(ref Config.AllowBrainOfCthulhu, args, "Brain of Cthulhu");
                    return;

                case "dst":
                case "deerclops":
                case "deer":
                case "dc":
                    ToggleBoss(ref Config.AllowDeerclops, args, "Deerclops");
                    return;

                case "skeletron":
                case "sans":
                    ToggleBoss(ref Config.AllowSkeletron, args, "Skeletron");
                    return;

                case "queenbee":
                case "qb":
                    ToggleBoss(ref Config.AllowQueenBee, args, "Queen Bee");
                    return;

                case "hardmode":
                case "wallofflesh":
                case "wof":
                    ToggleBoss(ref Config.AllowWallOfFlesh, args, "Wall of Flesh/Hardmode");
                    return;

                case "queenslime":
                case "qs":
                    ToggleBoss(ref Config.AllowQueenSlime, args, "Queen Slime");
                    return;

                case "twins":
                case "thetwins":
                case "ret":
                case "spaz":
                    ToggleBoss(ref Config.AllowTheTwins, args, "The Twins");
                    return;

                case "destroyer":
                case "thedestroyer":
                    ToggleBoss(ref Config.AllowTheDestroyer, args, "The Destroyer");
                    return;

                case "skeletronprime":
                case "prime":
                    ToggleBoss(ref Config.AllowSkeletronPrime, args, "Skeletron Prime");
                    return;
                case "plantera":
                    ToggleBoss(ref Config.AllowPlantera, args, "Plantera");
                    return;
                case "golem":
                    ToggleBoss(ref Config.AllowGolem, args, "Golem");
                    return;

                case "duke":
                case "fishron":
                case "dukefishron":
                    ToggleBoss(ref Config.AllowDukeFishron, args, "Duke Fishron");
                    return;

                case "betsy":
                    ToggleBoss(ref Config.AllowBetsy, args, "Betsy");
                    return;

                case "eol":
                case "empress":
                case "empressoflight":
                    ToggleBoss(ref Config.AllowEmpressOfLight, args, "Empress of Light");
                    return;

                case "cultist":
                case "lunatic":
                case "lunaticcultist":
                    ToggleBoss(ref Config.AllowLunaticCultist, args, "Lunatic Cultist");
                    return;

                case "moonlord":
                case "ml":
                case "squid":
                    ToggleBoss(ref Config.AllowMoonLord, args, "Moonlord");
                    return;

                // Add other cases here...

                default:
                    args.Player.SendErrorMessage("Please specify boss to enable:");
                    args.Player.SendInfoMessage("Bosses have pre-set identifiers: eg, /enblb king, /enblb eoc, OR /enblb wof");
                    return;
            }

            void ToggleBoss(ref bool configField, CommandArgs args, string bossName)
            {
                configField = !configField;
                SaveConfigToFile(Config, Path.Combine(TShock.SavePath, "BossManager.json"));
                args.Player.SendInfoMessage($"[BossMGR] Identifier '{bossName}' is now set to {(configField ? "enabled" : "disabled")}");
            }

            void SaveConfigToFile(Config config, string filePath)
            {
                try
                {
                    // Serialize the configuration object to JSON
                    string json = JsonConvert.SerializeObject(config, Formatting.Indented);

                    // Write the JSON data to the specified file
                    File.WriteAllText(filePath, json);

                    // Optionally, you can provide feedback that the save was successful
                    args.Player.SendInfoMessage("BossMGR Configuration saved successfully.");
                    Console.WriteLine("BossMGR Configuration saved successfully.");
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that might occur during the save process
                    args.Player.SendInfoMessage("Error saving BossMGR configuration. Check logs for details");
                    Console.WriteLine($"Error saving BossMGR configuration: {ex.Message}");
                }
            }

        }

        public enum BossSpawnAttemptResultState
        {
            Success,
            IllegalSpawning,
            NotEnoughPlayers,
            NotAllowed
        }
    }
}
