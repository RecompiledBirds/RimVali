using RimValiCore;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace AvaliMod
{

    public static class RimValiUtility
    {
        public static string build = "Einu 1.0.9";
        public static string modulesFound = "Modules:\n";
        private static AvaliPackDriver driver;
        private static AvaliUpdater thoughtDriver;
        private static ThreadQueue threadQueue;

        public static HashSet<Pawn> PawnsInWorld => RimValiCore.RimValiUtility.AllPawnsOfRaceInWorld(RimValiDefChecks.PotentialPackRaces).Where(x => !x.story.traits.HasTrait(AvaliDefs.AvaliPackBroken) && x.Spawned).ToHashSet();
        
        public static ThreadQueue ThreadQueue
        {
            get
            {
                if (threadQueue == null)
                {
                    threadQueue = Current.Game.World.GetComponent<ThreadQueue>();
                }
                return threadQueue;
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
        public static void SetDriver(AvaliPackDriver avaliPackDriver)
        {
            driver = avaliPackDriver;
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

        #region pack skills
        public static SkillRecord GetHighestSkillOfpack(AvaliPack pack)
        {
            int highestSkillLevel = 0;
            SkillRecord highestSkill = null;
            foreach (Pawn pawn in pack.GetAllNonNullPawns)
            {

                List<SkillRecord> list = new List<SkillRecord>();
                foreach (SkillRecord skillRecord in pawn.skills.skills.Where(x => DefDatabase<AvaliPackSkillDef>.AllDefs.Any(y => y.skill == x.def)))
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
            int highestSkillLevel = 0;
            SkillRecord highestSkill = null;
            if (!pack.GetAllNonNullPawns.EnumerableNullOrEmpty())
            {
                foreach (Pawn pawn in pack.GetAllNonNullPawns.Where(x => x.skills != null && !x.skills.skills.NullOrEmpty() && x.Spawned))
                {
                    if (avoidSkills == null) { avoidSkills = new List<SkillDef>(); }
                    IEnumerable<SkillRecord> records = pawn.skills.skills.Where(SkillRec => !avoidSkills.Contains(SkillRec.def));
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
            AvaliPack pack = GetPack(pawn);
            if (pack != null) { return GetHighestSkillOfpack(pack); }
            return null;
        }
        #endregion




        #region room
        public static bool PackInBedroom(this Pawn pawn)
        {
            Room room = pawn.GetRoom();
            AvaliPack avaliPack = pawn.GetPackWithoutSelf();
            return room != null && avaliPack != null && room.ContainedBeds.Count() > 0 && room.ContainedBeds.Any(bed => bed.OwnersForReading != null && bed.OwnersForReading.Any(p => p != pawn && !avaliPack.GetAllNonNullPawns.EnumerableNullOrEmpty() && avaliPack.GetAllNonNullPawns.Contains(p)));
        }

        public static bool CheckIfPackmatesInRoom(this Pawn pawn)
        {
            if (pawn.Spawned && pawn.Map != null)
            {
                AvaliPack pack = GetPackWithoutSelf(pawn);
                Room room = pawn.GetRoom();
                return room != null && pawn.Position.Roofed(pawn.Map) && pack != null && !pack.GetAllNonNullPawns.EnumerableNullOrEmpty() && pack.GetAllNonNullPawns.Any(packmate => packmate.Spawned && packmate.Map == pawn.Map && packmate.GetRoom() != null && packmate.GetRoom() == room);
            }
            return false;
        }
        #endregion

        #region pack gen and handling
        public static void CreatePack(Pawn pawn, string reason = null)
        {
            AvaliPack newPack = EiCreatePack(pawn);

            if (!driver.ContainsPack(newPack)) { driver.AddPack(newPack); }
            if (RimValiMod.settings.enableDebugMode && reason != null) { Log.Message($"Creating pack for reason: {reason}"); }
        }


        public static IEnumerable<AvaliPack> FindAllUsablePacks(Pawn pawn)
        {
            int packLimit = RimValiMod.settings.maxPackSize;
            HashSet<AvaliPack> packs = Driver.Packs;
            IEnumerable<AvaliPack> packsToUse = packs;
            packsToUse = packs.Where(pack => pack != null && !pack.GetAllNonNullPawns.EnumerableNullOrEmpty() &&
                                    (!Driver.HasPack(pawn) || pack != pawn.GetPack())
                                    && pack.GetAllNonNullPawns.Count < packLimit &&
                                    pack.faction == pawn.Faction &&
                                    pack.GetAllNonNullPawns.Any(packmate => packmate.Spawned && packmate.Alive() && packmate.Map != null && packmate.Map == pawn.Map));
            return packsToUse;
        }

        /// <summary>
        /// Handles the job of managing pack related functions, such as creating packs for a pawn, making a pawn join packs, etc.
        /// </summary>
        /// <param name="packs"></param>
        /// <param name="pawn"></param>
        /// <param name="racesInPacks"></param>
        /// <param name="packLimit"></param>
        /// <returns></returns>

        public static void KioPackHandler(Pawn pawn)
        {
            if (pawn != null && pawn.Spawned && pawn.Alive() && pawn.story != null && pawn.story.traits != null && !pawn.story.traits.HasTrait(AvaliDefs.AvaliPackBroken))
            {

                bool hasPack = Driver.HasPack(pawn);
                if (!Driver.Packs.EnumerableNullOrEmpty())
                {
                    IEnumerable<AvaliPack> packsToUse = FindAllUsablePacks(pawn);

                    //We only want to run this if there are packs, otherwise we'll automatically make a "base" pack.
                    if (!packsToUse.EnumerableNullOrEmpty())
                    {
                        AvaliPack pack2 = packsToUse.RandomElement();
                        AvaliPack pack = hasPack ? pawn.GetPack() : null;

                        if (hasPack)
                        {
                            PackTransferal packTransferal = ShouldTransferToOtherPack(pawn);
                            if (packTransferal.isRecommended)
                            {
                                Driver.TransferPawnToPack(pawn, packTransferal.recommendedPack);
                            }
                            if (ShouldLeavePack(pawn) && !packTransferal.isRecommended)
                            {
                                LeavePack(pawn);
                            }
                        }
                        //This checks if a pack only has one pawn or if it is null, and also if we can join a random pack.
                        if ((!hasPack || pack.GetAllNonNullPawns.EnumerableNullOrEmpty() || pack.GetAllNonNullPawns.Count == 1) && pack2 != null && !pack2.pawns.EnumerableNullOrEmpty())
                        {
                            if (RimValiMod.settings.enableDebugMode)
                            {
                                Log.Message($"{pawn.Name.ToStringShort} is trying to join {pack2.name}");
                            }
                            JoinPack(pawn, ref pack2, out bool Joined);
                            if (Joined)
                            {
                                goto JoinedPack;
                            }
                        }

                        CreatePack(pawn, "pawn cant join a pack");

                    JoinedPack: if (RimValiMod.settings.enableDebugMode) { Log.Message($"{pawn.Name.ToStringShort} joined a pack"); }
                    }
                    else if (!Driver.HasPack(pawn)) { 
                        CreatePack(pawn, "No packs in toUse list"); 
                    }
                }
                if (Driver.Packs.EnumerableNullOrEmpty())
                {
                    CreatePack(pawn, "packs list was null or empty");
                }
                #region cleanup
                //Avoids pawns with double packs
                // driver.CleanupBadPacks();
                Driver.CleanupPawnPacks(pawn);
                #endregion
            }
        }
        #region get opinion and joining
        public static float GetPackAvgOP(AvaliPack pack, Pawn pawn)
        {
            HashSet<float> opinions = new HashSet<float>();

            foreach (Pawn packmember in pack.GetAllNonNullPawns)
            {
                if (packmember != pawn)
                {
                    opinions.Add(packmember.relations.OpinionOf(pawn));
                }
            }
            return Queryable.Average(opinions.AsQueryable());
        }

        public static AvaliPack JoinPack(Pawn pawn, ref AvaliPack pack, out bool Joined)
        {
            Date date = new Date();
            Joined = false;
            if ((!driver.PawnHasHadPack(pawn)) && date.ToString() == pack.creationDate.ToString())
            {

                if (pack.pawns.Count == 1 && !driver.PawnHasHadPack(pack.pawns.First()))
                {
                    Driver.MakePawnHavePack(pack.pawns.First());
                }
                Driver.TransferPawnToPack(pawn, pack);
                Driver.MakePawnHavePack(pawn);
                Joined = true;

            }
            else if (GetPackAvgOP(pack, pawn) >= LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packOpReq)
            {
                Driver.TransferPawnToPack(pawn, pack);
                Joined = true;
            }

            return pack;
        }
        #endregion
        public static AvaliPack EiCreatePack(Pawn pawn)
        {

            AvaliPack PawnPack = new AvaliPack(pawn);
            if (RimValiMod.settings.enableDebugMode) { Log.Message("Creating pack: " + PawnPack.name); }
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
            IEnumerable<AvaliPack> packs = driver.Packs.Where(x => GetPackAvgOP(x, pawn) > 30 && x.faction == pawn.Faction && x.GetAllNonNullPawns.Any(p => p.Map == pawn.Map));
            if (ShouldLeavePack(pawn) && packs.Count() > 0)
            {
                foreach (AvaliPack pack in packs)
                {
                    if (GetPackAvgOP(pack, pawn) > 30)
                    {
                        return new PackTransferal
                        {
                            isRecommended = true,
                            recommendedPack = pack
                        };
                    }
                }
            }
            return new PackTransferal
            {
                isRecommended = false
            };
        }


        public static bool ShouldLeavePack(Pawn pawn)
        {
            AvaliPack pack = pawn.GetPack();

            return Driver.PawnHasHadMoreThenOnePack(pawn) && pack != null && GetPackAvgOP(pack, pawn) < 30;
        }

        public static void LeavePack(Pawn pawn)
        {
            AvaliPack pack = pawn.GetPack();
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
        public static AvaliPack GetPack(this Pawn pawn)
        {
            if (pawn == null)
            {
                return null;
            }
            if (Driver.Packs.EnumerableNullOrEmpty())
            {
                if (RimValiMod.settings.enableDebugMode) { 
                    Log.Error("Pawn check is null, or no packs!"); 
                }
                return null;
            }

            if (driver != null && driver.Packs.Any(x => x.GetAllNonNullPawns.Contains(pawn)))
            {
                AvaliPack pack = driver.Packs.First(x => x.GetAllNonNullPawns.Contains(pawn));
                return pack;
            }
            return null;
        }


        public static AvaliPack GetPackWithoutSelf(this Pawn pawn)
        {
            if (pawn == null)
            {
                return null;
            }

            if (Driver.Packs.EnumerableNullOrEmpty())
            {
                if (RimValiMod.settings.enableDebugMode) { Log.Error("Pawn check is null, or no packs!"); }
                return null;
            }
            //We really should be getting here
            if (Driver.Packs.Any(x => x.GetAllNonNullPawns.Contains(pawn)))
            {
                AvaliPack pack = Driver.Packs.First(x => x.GetAllNonNullPawns.Contains(pawn));
                if (pack != null)
                {
                    AvaliPack returnPack = new AvaliPack(pawn);
                    returnPack.pawns.AddRange(pack.pawns);
                    returnPack.pawns.Remove(pawn);
                    returnPack.name = pack.name;
                    returnPack.leaderPawn = pack.leaderPawn;
                    //returnPack.size = APack.size;
                    returnPack.deathDates.AddRange(pack.deathDates);
                    returnPack.creationDate = pack.creationDate;
                    return returnPack;
                }

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
                PackComp packComp = pawn.TryGetComp<PackComp>();
                if (packComp != null)
                {
                    packComp.ticksSinceLastInpack = 0;
                    Log.Message($"Reset {pawn.Name}'s pack loss");
                }
            }
        }



        [DebugAction("RimVali", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ResetPacks()
        {
            Driver.ResetPacks();
        }
        [DebugAction("RimVali", null, false, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SubtractAirdropTime()
        {
            AirDropHandler.timeToDrop -= 10 * 25;
        }

        [DebugAction("RimVali", "Airdrop now", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void DropNow()
        {
            AirDropHandler.timeToDrop = 0;
        }





        [DebugAction("Nesi", "Message test", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void MessageTest()
        {
            Assembly a = Assembly.Load(RimValiMod.GetDir + "/PresentationFramework.dll");
            //AppDomain.CurrentDomain.Load()
            if (a == null)
            {
                Log.Error("Assembly is null!");
            }
            //RimValiCore.RimValiUtility.InvokeMethod(a,"Show","MessageBox",obj, out int result, new object[] {str1, "Nesi" });
            /*MessageBoxResult res = MessageBox.Show("Are you watching? \n -Nesi", "Nesi");
            if (res == MessageBoxResult.Yes)
            {
                MessageBox.Show("I hope you enjoy the show! \n -Nesi");
            }
            else
            {
                MessageBox.Show("Oh.. alright. I hope you understand I don't like to be ignored. \n -Nesi");
            }*/
        }
        #endregion
    }
}