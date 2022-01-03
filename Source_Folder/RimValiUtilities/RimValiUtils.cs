using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using System;
using RimValiCore;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;

namespace AvaliMod
{

    public static class RimValiUtility
    {
        public static string build = "Kisu 1.1.0";
        public static string modulesFound = "RimValiModules".Translate()+"\n";

        static AvaliPackDriver driver;
        static AvaliUpdater thoughtDriver;


        public static HashSet<Pawn> PawnsInWorld
        {
            get
            {
                return RimValiCore.RimValiUtility.AllPawnsOfRaceInWorld(RimValiDefChecks.PotentialPackRaces).Where(x => !x.story.traits.HasTrait(AvaliDefs.AvaliPackBroken) && x.Spawned).ToHashSet();
            }
        }
        public static AvaliPackDriver Driver
        {
            get
            {
                if (driver == null)
                    driver = Current.Game.World.GetComponent<AvaliPackDriver>();
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
                    thoughtDriver = Find.World.GetComponent<AvaliUpdater>();
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
            AvaliPack pack = Driver.GetCurrentPack(pawn);
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

            driver.AddPack(newPack);
            if (RimValiMod.settings.enableDebugMode && reason != null) { Log.Message($"Creating pack for reason: {reason}"); }
        }




        /// <summary>
        /// Collect an IEnumberable of all packs this pawn could join, assuming they are not part of a pack.
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static IEnumerable<AvaliPack> FindAllUsablePacks(Pawn pawn)
        {
            return driver.Packs.Where(pack => !pack.GetAllNonNullPawns.EnumerableNullOrEmpty() && pack.GetAllNonNullPawns.Count < RimValiMod.settings.maxPackSize && pack.faction == pawn.Faction && pack.GetAllNonNullPawns.Any(x => x.Spawned && x.Alive() && x.Map == pawn.Map));
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
            if (pawn != null && pawn.Alive() && !pawn.story.traits.HasTrait(AvaliDefs.AvaliPackBroken))
            {
                AvaliPack pack = Driver.GetCurrentPack(pawn);
                bool hasPack = pack != null && (pack.GetAllNonNullPawns.Count > 1);
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
                                JoinPack(pawn, ref joinablePack, out bool Joined);
                            }
                        }
                    }
                    else
                    {
                        Log.Message("packs to use fail");
                        CreatePack(pawn, "packs list was null or empty");
                    }
                }
                else
                {
                    Log.Message("normal fail");
                    CreatePack(pawn, "packs list was null or empty");
                }
            }
        }
        #region get opinion and joining
        public static float GetPackAvgOP(AvaliPack pack, Pawn pawn)
        {
            if (pack != null)
            {
                float sum = 0;
                int cont = pack.GetAllNonNullPawns.Count;
                for (int a = 0; a<cont; a++)

                sum /= cont;
                return sum;
            }
            return 0;
        }

        public static void JoinPack(Pawn pawn, ref AvaliPack pack, out bool Joined)
        {
            
            Date date = new Date();
            Joined = false;
            if (pack.pawns.Count < RimValiMod.settings.maxPackSize)
            {
                if ((!driver.PawnsHasHadPack(pawn)) && date.ToString() == pack.creationDate.ToString())
                {
                    driver.AddPawnToPack(pawn, ref pack);
                    Joined = true;

                }
                else if (GetPackAvgOP(pack, pawn) >= RimValiMod.settings.packOpReq)
                {
                    driver.AddPawnToPack(pawn, ref pack);

                    Joined = true;
                }
            };
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
            if (ShouldLeavePack(pawn) && packs.Count()>0)
            {
                foreach (AvaliPack pack in packs)
                {
                    if (GetPackAvgOP(pack, pawn) > 30)
                    {
                        return new PackTransferal
                        {
                            isRecommended = true,
                            recommendedPack=pack
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
            AvaliPack pack = Driver.GetCurrentPack(pawn);

            return driver.GetPackCount(pawn)>1 && pack != null && GetPackAvgOP(pack, pawn) < 30;
        }

        public static void LeavePack(Pawn pawn)
        {
            AvaliPack pack = Driver.GetCurrentPack(pawn);
            if (pack == null)
                return;

            pack.pawns.Remove(pawn);

            if (pack.leaderPawn == pawn)
                if (pack.pawns.Count > 0)
                    pack.leaderPawn = pack.pawns.ToList()[0];
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
                if (RimValiMod.settings.enableDebugMode) { Log.Error("Pawn check is null, or no packs!"); }
                return null;
            }

            AvaliPack pack = Driver.GetCurrentPack(pawn);
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
       


        [DebugAction("RimVali", allowedGameStates =AllowedGameStates.PlayingOnMap)]
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
            Assembly a = Assembly.Load(RimValiMod.GetDir+ "/PresentationFramework.dll");
            //AppDomain.CurrentDomain.Load()
            if (a == null)
            {
                Log.Error("Assembly is null!");
            }
            string str1 = "Are you watching? \n -Nesi";
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