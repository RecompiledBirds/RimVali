using System.Collections.Generic;
using System.Linq;
using RimValiCore;
using RimValiCore.RVR;
using RimWorld;
using Verse;
using Enumerable = System.Linq.Enumerable;

namespace AvaliMod
{
    public static class RimValiUtility
    {
        public static string build = "Eiku 1.2.0";

        private static AvaliPackDriver driver;
        private static AvaliUpdater thoughtDriver;
        public static IEnumerable<ModuleDef> foundModules = new List<ModuleDef>();

        public static string FoundModulesString =>
            "RimValiModules".TranslateSimple() + "\n" +
            string.Join("\n", Enumerable.Select(foundModules, module => module.name));


        public static HashSet<Pawn> PawnsInWorld
        {
            get
            {
                return Enumerable.ToHashSet(Enumerable.Where(
                    RimValiCore.RimValiUtility.AllPawnsOfRaceInWorld(RimValiDefChecks.PotentialPackRaces),
                    x => !x.story.traits.HasTrait(AvaliDefs.AvaliPackBroken) && x.Spawned));
            }
        }

        public static AvaliPackDriver Driver
        {
            get
            {
                if (driver == null)
                {
                    driver = Current.Game.World.GetComponent<AvaliPackDriver>();
                }

                return driver;
            }
        }

        public static AvaliUpdater AvaliThoughtDriver
        {
            get
            {
                if (thoughtDriver == null)
                {
                    thoughtDriver = Find.World.GetComponent<AvaliUpdater>();
                }

                return thoughtDriver;
            }
        }

        public static void SetDriver(AvaliPackDriver avaliPackDriver)
        {
            driver = avaliPackDriver;
        }

        #region pack skills

        public static SkillRecord GetHighestSkillOfpack(AvaliPack pack)
        {
            var highestSkillLevel = 0;
            SkillRecord highestSkill = null;
            foreach (Pawn pawn in pack.GetAllNonNullPawns)
            {
                var list = new List<SkillRecord>();
                foreach (SkillRecord skillRecord in Enumerable.Where(pawn.skills.skills, x =>
                             Enumerable.Any(DefDatabase<AvaliPackSkillDef>.AllDefs, y => y.skill == x.def)))
                {
                    list.Add(skillRecord);
                    foreach (SkillRecord skill in list)
                    {
                        if (skill.Level > highestSkillLevel)
                        {
                            highestSkillLevel = skill.Level;
                            highestSkill = skill;
                        }
                    }
                }
            }

            return highestSkill;
        }


        public static SkillRecord GetHighestSkillOfpack(AvaliPack pack, List<SkillDef> avoidSkills)
        {
            var highestSkillLevel = 0;
            SkillRecord highestSkill = null;
            if (!pack.GetAllNonNullPawns.EnumerableNullOrEmpty())
            {
                foreach (Pawn pawn in pack.GetAllNonNullPawns.Where(x =>
                             x.skills != null && !x.skills.skills.NullOrEmpty() && x.Spawned))
                {
                    if (avoidSkills == null)
                    {
                        avoidSkills = new List<SkillDef>();
                    }

                    IEnumerable<SkillRecord> records =
                        Enumerable.Where(pawn.skills.skills, SkillRec => !avoidSkills.Contains(SkillRec.def));
                    if (!records.EnumerableNullOrEmpty())
                    {
                        foreach (SkillRecord skillRecord in records)
                        {
                            if (skillRecord != null && skillRecord.Level > highestSkillLevel)
                            {
                                highestSkillLevel = skillRecord.Level;
                                highestSkill = skillRecord;
                            }
                        }
                    }
                }
            }

            return highestSkill;
        }

        public static SkillRecord GetHighestSkillOfpack(Pawn pawn)
        {
            AvaliPack pack = Driver.GetCurrentPack(pawn);
            if (pack != null)
            {
                return GetHighestSkillOfpack(pack);
            }

            return null;
        }

        #endregion


        #region room

        public static bool PackInBedroom(this Pawn pawn)
        {
            Room room = pawn.GetRoom();
            AvaliPack avaliPack = pawn.GetPackWithoutSelf();
            return room != null && avaliPack != null && Enumerable.Count(room.ContainedBeds) > 0 &&
                   room.ContainedBeds.Any(bed =>
                       bed.OwnersForReading != null && bed.OwnersForReading.Any(p =>
                           p != pawn && !avaliPack.GetAllNonNullPawns.EnumerableNullOrEmpty() &&
                           avaliPack.GetAllNonNullPawns.Contains(p)));
        }

        public static bool CheckIfPackmatesInRoom(this Pawn pawn)
        {
            if (pawn.Spawned && pawn.Map != null)
            {
                AvaliPack pack = GetPackWithoutSelf(pawn);
                Room room = pawn.GetRoom();
                return room != null && pawn.Position.Roofed(pawn.Map) && pack != null &&
                       !pack.GetAllNonNullPawns.EnumerableNullOrEmpty() && pack.GetAllNonNullPawns.Any(packmate =>
                           packmate.Spawned && packmate.Map == pawn.Map && packmate.GetRoom() != null &&
                           packmate.GetRoom() == room);
            }

            return false;
        }

        #endregion

        #region pack gen and handling

        public static void CreatePack(Pawn pawn, string reason = null)
        {
            AvaliPack newPack = EiCreatePack(pawn);

            driver.AddPack(newPack);
            if (RimValiMod.settings.enableDebugMode && reason != null)
            {
                Log.Message($"Creating pack for reason: {reason}");
            }
        }


        /// <summary>
        ///     Collect an IEnumberable of all packs this pawn could join, assuming they are not part of a pack.
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static IEnumerable<AvaliPack> FindAllUsablePacks(Pawn pawn)
        {
            return driver.Packs.Where(pack => !pack.GetAllNonNullPawns.EnumerableNullOrEmpty() &&
                                              pack.GetAllNonNullPawns.Count < RimValiMod.settings.maxPackSize &&
                                              pack.faction == pawn.Faction &&
                                              pack.GetAllNonNullPawns.Any(x =>
                                                  x.Spawned && x.Alive() && x.Map == pawn.Map));
        }

        /// <summary>
        ///     Handles the job of managing pack related functions, such as creating packs for a pawn, making a pawn join packs,
        ///     etc.
        /// </summary>
        /// <param name="packs"></param>
        /// <param name="pawn"></param>
        /// <param name="racesInPacks"></param>
        /// <param name="packLimit"></param>
        /// <returns></returns>
        public static void KioPackHandler(Pawn pawn)
        {
            if (pawn != null && pawn.Alive() && !pawn.story.traits.HasTrait(AvaliDefs.AvaliPackBroken))
            {
                AvaliPack pack = Driver.GetCurrentPack(pawn);
                bool hasPack = pack != null && pack.GetAllNonNullPawns.Count > 1;
                if (!Driver.Packs.EnumerableNullOrEmpty())
                {
                    IEnumerable<AvaliPack> packsToUse = FindAllUsablePacks(pawn);
                    if (!packsToUse.EnumerableNullOrEmpty())
                    {
                        if (hasPack)
                        {
                            PackTransferal transferal = ShouldTransferToOtherPack(pawn);
                            if (transferal.isRecommended)
                            {
                                Driver.SwitchPack(pawn, transferal.recommendedPack);
                            }
                            else if (ShouldLeavePack(pawn))
                            {
                                LeavePack(pawn);
                            }
                        }
                        else
                        {
                            AvaliPack joinablePack = packsToUse.RandomElement();
                            if (joinablePack != null && !joinablePack.pawns.EnumerableNullOrEmpty())
                            {
                                JoinPack(pawn, ref joinablePack, out _);
                            }
                        }
                    }
                    else
                    {
                        CreatePack(pawn, "packs list was null or empty");
                    }
                }
                else
                {
                    CreatePack(pawn, "packs list was null or empty");
                }
            }
        }

        #region get opinion and joining

        public static float GetAveragePackOpinion(AvaliPack pack, Pawn pawn)
        {
            if (pack != null && pawn != null)
            {
                HashSet<Pawn> nonNullMembers = pack.GetAllNonNullPawns;

                return nonNullMembers.Sum(packMember => packMember.relations.OpinionOf(pawn)) /
                       (float)(nonNullMembers.Count() - 1);
            }

            return 0.0f;
        }

        public static void JoinPack(Pawn pawn, ref AvaliPack pack, out bool joined)
        {
            var date = new Date();
            joined = false;
            if (pack.pawns.Count < RimValiMod.settings.maxPackSize)
            {
                if (!driver.PawnsHasHadPack(pawn) && date.ToString() == pack.creationDate.ToString())
                {
                    driver.AddPawnToPack(pawn, ref pack);
                    joined = true;
                }
                else if (GetAveragePackOpinion(pack, pawn) >= RimValiMod.settings.packOpReq)
                {
                    driver.AddPawnToPack(pawn, ref pack);

                    joined = true;
                }
            }
        }

        #endregion

        public static AvaliPack EiCreatePack(Pawn pawn)
        {
            var PawnPack = new AvaliPack(pawn,Driver.GetNewPackID());
            if (RimValiMod.settings.enableDebugMode)
            {
                Log.Message("Creating pack: " + PawnPack.name);
            }

            return PawnPack;
        }

        #region kicking a pawn from a pack and or pack transfering

        public struct PackTransferal
        {
            public bool isRecommended;
            public AvaliPack recommendedPack;
        }

        public static PackTransferal ShouldTransferToOtherPack(Pawn pawn)
        {
            IEnumerable<AvaliPack> packs = driver.Packs.Where(x =>
                GetAveragePackOpinion(x, pawn) > 30 && x.faction == pawn.Faction &&
                x.GetAllNonNullPawns.Any(p => p.Map == pawn.Map));
            if (ShouldLeavePack(pawn) && packs.Count() > 0)
            {
                foreach (AvaliPack pack in packs)
                {
                    if (GetAveragePackOpinion(pack, pawn) > 30)
                    {
                        return new PackTransferal
                        {
                            isRecommended = true,
                            recommendedPack = pack,
                        };
                    }
                }
            }

            return new PackTransferal
            {
                isRecommended = false,
            };
        }


        public static bool ShouldLeavePack(Pawn pawn)
        {
            AvaliPack pack = Driver.GetCurrentPack(pawn);

            return driver.GetPackCount(pawn) > 1 && pack != null && GetAveragePackOpinion(pack, pawn) < 30;
        }

        public static void LeavePack(Pawn pawn)
        {
            AvaliPack pack = Driver.GetCurrentPack(pawn);
            if (pack == null)
            {
                return;
            }

            pack.pawns.Remove(pawn);

            if (pack.leaderPawn == pawn)
            {
                if (pack.pawns.Count > 0)
                {
                    pack.leaderPawn = pack.pawns.ToList()[0];
                }
            }
        }

        #endregion


        #region getting pack info

        public static AvaliPack GetPackWithoutSelf(this Pawn pawn)
        {
            if (pawn == null)
            {
                return null;
            }

            if (Driver.Packs.EnumerableNullOrEmpty())
            {
                if (RimValiMod.settings.enableDebugMode)
                {
                    Log.Error("Pawn check is null, or no packs!");
                }

                return null;
            }

            AvaliPack pack = Driver.GetCurrentPack(pawn);
            if (pack != null)
            {
                var returnPack = new AvaliPack(pawn,pack.ID);
                returnPack.pawns.AddRange(pack.pawns);
                returnPack.pawns.Remove(pawn);
                returnPack.name = pack.name;
                returnPack.leaderPawn = pack.leaderPawn;
                //returnPack.size = APack.size;
                returnPack.deathDates.AddRange(pack.deathDates);
                returnPack.creationDate = pack.creationDate;
                return returnPack;
            }

            return null;
        }

        #endregion

        #endregion

        #region debug

        [DebugAction("RimVali", "Reset pack loss", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ResetPackLoss()
        {
            foreach (Pawn pawn in PawnsInWorld)
            {
                var packComp = pawn.TryGetComp<PackComp>();
                if (packComp != null)
                {
                    packComp.ticksSinceLastInPack = 0;
                    Log.Message($"Reset {pawn.Name}'s pack loss");
                }
            }
        }

        public static bool IsAvali(this Pawn pawn)
        {
            return pawn.def == AvaliDefs.RimVali;
        }

        public static bool IsPackBroken(this Pawn pawn)
        {
            return pawn.story.traits.HasTrait(AvaliDefs.AvaliPackBroken) || pawn.story.AllBackstories.Any(x=>x.GetTags().Contains("PackBroken"));
        }

        [DebugAction("RimVali", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ResetPacks()
        {
            Driver.ResetPacks();
        }

        [DebugAction("RimVali", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SubtractAirdropTime()
        {
            AirDropHandler.timeToDrop -= 10 * 25;
        }

        [DebugAction("RimVali", "Airdrop now", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void DropNow()
        {
            AirDropHandler.timeToDrop = 0;
        }

        #endregion
    }
}
