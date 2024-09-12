using Terraria;
using Terraria.ID;
using Terraria.GameContent.Events;
using Terraria.DataStructures;
using TShockAPI;

namespace BossManager
{
    public partial class Plugin
    {
        public static BossSpawnAttemptResultState IsBossSpawnable(int npcID)
        {
            if (Config.PreventIllegalBoss)
            {
                if (!Main.hardMode &&
                    (npcID == NPCID.QueenSlimeBoss ||
                    npcID == NPCID.TheDestroyer ||
                    npcID == NPCID.Retinazer ||
                    npcID == NPCID.Spazmatism ||
                    npcID == NPCID.SkeletronPrime ||
                    npcID == NPCID.DukeFishron))
                {
                    return BossSpawnAttemptResultState.IllegalSpawning;
                }

                if (!NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss3 &&
                    npcID == NPCID.Plantera)
                {
                    return BossSpawnAttemptResultState.IllegalSpawning;
                }

                if (!NPC.downedPlantBoss &&
                    (npcID == NPCID.HallowBoss || npcID == NPCID.EmpressButterfly || npcID == NPCID.Golem))
                {
                    return BossSpawnAttemptResultState.IllegalSpawning;
                }

                if (!DD2Event.ReadyForTier3 && !DD2Event.Ongoing && npcID == NPCID.DD2Betsy)
                {
                    return BossSpawnAttemptResultState.IllegalSpawning;
                } 
                if (!NPC.downedGolemBoss &&
                    (npcID == NPCID.CultistBoss || npcID == NPCID.MoonLordCore))
                {
                    return BossSpawnAttemptResultState.IllegalSpawning;
                }
            }
            
            if (TShock.Utils.GetActivePlayerCount() < Config.RequiredPlayersforBoss)
            {
                if (!NPC.downedSlimeKing && npcID == NPCID.KingSlime ||
                    !NPC.downedBoss1 && npcID == NPCID.EyeofCthulhu ||
                    (!NPC.downedBoss2 && (npcID == NPCID.EaterofWorldsHead || npcID == NPCID.BrainofCthulhu)) ||
                    !NPC.downedDeerclops && npcID == NPCID.Deerclops ||
                    !NPC.downedBoss3 && npcID == NPCID.SkeletronHead ||
                    !NPC.downedQueenBee && npcID == NPCID.QueenBee ||
                    (!Main.hardMode && npcID == NPCID.WallofFlesh) ||
                    !NPC.downedQueenSlime && npcID == NPCID.QueenSlimeBoss ||
                    !NPC.downedMechBoss1 && npcID == NPCID.TheDestroyer ||
                    !NPC.downedMechBoss2 && npcID == NPCID.Retinazer ||
                    !NPC.downedMechBoss2 && npcID == NPCID.Spazmatism ||
                    !NPC.downedMechBoss3 && npcID == NPCID.SkeletronPrime ||
                    !NPC.downedPlantBoss && npcID == NPCID.Plantera ||
                    !NPC.downedGolemBoss && npcID == NPCID.Golem ||
                    !NPC.downedFishron && npcID == NPCID.DukeFishron ||
                    (!DD2Event.ReadyForTier3 && !DD2Event.Ongoing && npcID == NPCID.DD2Betsy) ||
                    !NPC.downedEmpressOfLight && npcID == NPCID.HallowBoss ||
                    !NPC.downedAncientCultist && npcID == NPCID.CultistBoss ||
                    !NPC.downedMoonlord && npcID == NPCID.MoonLordCore)
                {
                    return BossSpawnAttemptResultState.NotEnoughPlayers;
                }
            }

            if (!Config.AllowKingSlime && npcID == NPCID.KingSlime) // King Slime
            {
                return BossSpawnAttemptResultState.NotAllowed;
            }

            if (!Config.AllowEyeOfCthulhu && npcID == NPCID.EyeofCthulhu) // Eye of Cthulhu
            {
                return BossSpawnAttemptResultState.NotAllowed;
            }

            if (!Config.AllowEaterOfWorlds && npcID == NPCID.EaterofWorldsHead) // Eater of Worlds
            {
                return BossSpawnAttemptResultState.NotAllowed;
            }

            if (!Config.AllowBrainOfCthulhu && npcID == NPCID.BrainofCthulhu) // Brain of Cthulhu
            {
                return BossSpawnAttemptResultState.NotAllowed;
            }

            if (!Config.AllowQueenBee && npcID == NPCID.QueenBee) // Queen Bee
            {
                return BossSpawnAttemptResultState.NotAllowed;
            }

            if (!Config.AllowSkeletron && npcID == NPCID.SkeletronHead) // Skeletron
            {
                return BossSpawnAttemptResultState.NotAllowed;
            }

            if (!Config.AllowDeerclops && npcID == NPCID.Deerclops) // Deerclops
            {
                return BossSpawnAttemptResultState.NotAllowed;
            }

            if (!Config.AllowWallOfFlesh && npcID == NPCID.WallofFlesh) // Wall of Flesh
            {
                return BossSpawnAttemptResultState.NotAllowed;
            }

            if (!Config.AllowQueenSlime && npcID == NPCID.QueenSlimeBoss) // Queen Slime
            {
                return BossSpawnAttemptResultState.NotAllowed;
            }

            if (!Config.AllowTheTwins && (npcID == NPCID.Retinazer || npcID == NPCID.Spazmatism)) // The Twins
            {
                return BossSpawnAttemptResultState.NotAllowed;
            }

            if (!Config.AllowTheDestroyer && npcID == NPCID.TheDestroyer) // The Destroyer
            {
                return BossSpawnAttemptResultState.NotAllowed;
            }

            if (!Config.AllowSkeletronPrime && npcID == NPCID.SkeletronPrime) // Skeletron Prime
            {
                return BossSpawnAttemptResultState.NotAllowed;
            }

            if (!Config.AllowPlantera && npcID == NPCID.Plantera) // Plantera
            {
                return BossSpawnAttemptResultState.NotAllowed;
            }

            if (!Config.AllowGolem && npcID == NPCID.Golem) // Golem
            {
                return BossSpawnAttemptResultState.NotAllowed;
            }

            if (!Config.AllowDukeFishron && npcID == NPCID.DukeFishron) // Duke Fishron
            {
                return BossSpawnAttemptResultState.NotAllowed;
            }

            if (!Config.AllowBetsy && npcID == NPCID.DD2Betsy) // Betsy
            {
                return BossSpawnAttemptResultState.NotAllowed;
            }

            if (!Config.AllowEmpressOfLight && npcID == NPCID.HallowBoss) // Empress of Light
            {
                return BossSpawnAttemptResultState.NotAllowed;
            }

            if (!Config.AllowLunaticCultist && npcID == NPCID.CultistBoss) // Lunatic Cultist
            {
                return BossSpawnAttemptResultState.NotAllowed;
            }

            if (!Config.AllowMoonLord && npcID == NPCID.MoonLordCore) // Moon Lord
            {
                return BossSpawnAttemptResultState.NotAllowed;
            }
            
            return BossSpawnAttemptResultState.Success;
        }

        public static void AnnounceIllegalSpawning(IEntitySource source, int npcID)
        {
            string bossName = Lang.GetNPCNameValue(npcID);
            if (bossName == "Skeletron Prime" && Main.zenithWorld)
                bossName = "Mechdusa";
            if (source is EntitySource_BossSpawn e && e.Entity is Player plr)
            {
                TSPlayer player = TShock.Players[plr.whoAmI];
                player.SendErrorMessage($"You can't spawn {bossName} in the current world state");
                TShock.Log.ConsoleInfo($"[BossMGR]: Stopped {player.Name} spawning {bossName}. Reason: Illegal Spawning");
            }
            else
            {
                TSPlayer.All.SendErrorMessage($"Could not spawn {bossName}. Server detected unusual spawning method");
                TShock.Log.ConsoleInfo($"[BossMGR]: Stopped Server spawning {bossName}. Reason: Illegal Spawning");
            }
        }

        public static void AnnounceNotEnoughPlayers(IEntitySource source, int npcID)
        {
            string bossName = Lang.GetNPCNameValue(npcID);
            if (bossName == "Skeletron Prime" && Main.zenithWorld)
                bossName = "Mechdusa";
            if (source is EntitySource_BossSpawn e && e.Entity is Player plr)
            {
                TSPlayer player = TShock.Players[plr.whoAmI];
                player.SendErrorMessage($"The server needs {Config.RequiredPlayersforBoss - TShock.Utils.GetActivePlayerCount()} more player(s) in order to spawn bosses");
                TShock.Log.ConsoleInfo($"[BossMGR]: Stopped {player.Name} spawning {bossName}. Reason: Not Enough Players");
            }
            else
            {
                TSPlayer.All.SendErrorMessage($"Could not spawn {bossName}. Needs {Config.RequiredPlayersforBoss - TShock.Utils.GetActivePlayerCount()} more player(s) in order to spawn bosses");
                TShock.Log.ConsoleInfo($"[BossMGR]: Stopped Server spawning {bossName}. Reason: Not Enough Players");
            }
        }

        public static void AnnounceNotAllowed(IEntitySource source, int npcID)
        {
            string bossName = Lang.GetNPCNameValue(npcID);
            if (bossName == "Skeletron Prime" && Main.zenithWorld)
                bossName = "Mechdusa";
            if (source is EntitySource_BossSpawn e && e.Entity is Player plr)
            {
                TSPlayer player = TShock.Players[plr.whoAmI];
                player.SendErrorMessage($"{bossName} is disabled at the moment");
                TShock.Log.ConsoleInfo($"[BossMGR]: Stopped {player.Name} spawning {bossName}. Reason: Not Allowed");
            }
            else
            {
                TSPlayer.All.SendErrorMessage($"{bossName} is disabled at the moment.");
                TShock.Log.ConsoleInfo($"[BossMGR]: Stopped Server spawning {bossName}. Reason: Not Allowed");
            }
        }

        public static int GetSummoningItemFromBossNetID(int npcID)
        {
            return npcID switch
            {
                NPCID.KingSlime => ItemID.SlimeCrown,
                NPCID.EyeofCthulhu => ItemID.SuspiciousLookingEye,
                NPCID.EaterofWorldsHead => ItemID.WormFood,
                NPCID.BrainofCthulhu => ItemID.BloodySpine,
                NPCID.QueenBee => ItemID.Abeemination,
                NPCID.Deerclops => ItemID.DeerThing,
                NPCID.WallofFlesh => ItemID.GuideVoodooDoll,
                NPCID.QueenSlimeBoss => ItemID.QueenSlimeCrystal,
                NPCID.TheDestroyer => ItemID.MechanicalWorm,
                NPCID.Retinazer => ItemID.MechanicalEye,
                NPCID.SkeletronPrime => (!Main.zenithWorld) ? ItemID.MechanicalSkull : ItemID.MechdusaSummon,
                NPCID.Golem => ItemID.LihzahrdPowerCell,
                NPCID.DukeFishron => ItemID.TruffleWorm,
                NPCID.MoonLordCore => ItemID.CelestialSigil,
                _ => 0
            };
        }

        public static void GiveBackSummoningItem(IEntitySource source, int npcID)
        {
            if (source is EntitySource_BossSpawn e && e.Entity is Player plr)
            {
                TSPlayer player = TShock.Players[plr.whoAmI];
                int itemID = GetSummoningItemFromBossNetID(npcID);
                player.GiveItem(itemID, 1);
            }
        }
    }
}
