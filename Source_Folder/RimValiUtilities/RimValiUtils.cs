using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using System;
using RimValiCore;

namespace AvaliMod
{
    
    public static class RimValiUtility
    {
        public static string build = "Kio 1.0.3";
        public static string modulesFound = "Modules:\n";

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
                foreach (Pawn pawn in pack.GetAllNonNullPawns.Where(x=>x.skills!=null&&!x.skills.skills.NullOrEmpty()&&x.Spawned))
                {
                    if (avoidSkills == null){avoidSkills = new List<SkillDef>();}
                    IEnumerable<SkillRecord> records = pawn.skills.skills.Where(SkillRec => !avoidSkills.Contains(SkillRec.def));
                    if (!records.EnumerableNullOrEmpty())
                    {
                        foreach (SkillRecord skillRecord in records)
                        {
                            if (skillRecord != null && skillRecord.Level != null && skillRecord.Level > highestSkillLevel)
                            {
                                highestSkillLevel = skillRecord.Level;
                                highestSkill = skillRecord;
                            }
                        }
                    }

                }
                return highestSkill;
            }
            return null;
        }

        public static SkillRecord GetHighestSkillOfpack(Pawn pawn)
        {
            AvaliPack pack = GetPack(pawn);
            if (pack != null){return GetHighestSkillOfpack(pack);}
            return null;
        }
        #endregion

        


        #region room
        public static bool PackInBedroom(this Pawn pawn)
        {
            Room room = pawn.GetRoom();
            AvaliPack avaliPack = pawn.GetPackWithoutSelf();
            if (room != null && avaliPack != null && room.ContainedBeds.Count() > 0)
            {
                IEnumerable<Building_Bed> beds = room.ContainedBeds;
                return beds.Any(bed => bed.OwnersForReading != null && bed.OwnersForReading.Any(p => p != pawn && !avaliPack.GetAllNonNullPawns.EnumerableNullOrEmpty() && avaliPack.GetAllNonNullPawns.Contains(p)));
                
            }
            return false;
        }

        public static bool CheckIfPackmatesInRoom(this Pawn pawn)
        {
            if (pawn.Spawned && pawn.Map != null)
            {
                AvaliPack pack = GetPackWithoutSelf(pawn);
                Room room = pawn.GetRoom();
                if (room != null && pawn.Position.Roofed(pawn.Map) && pack != null && !pack.GetAllNonNullPawns.EnumerableNullOrEmpty()) { return pack.GetAllNonNullPawns.Any(packmate => packmate.Spawned && packmate.Map == pawn.Map && packmate.GetRoom() != null && packmate.GetRoom() == room); }
            }
            return false;
        }
        #endregion

        #region pack gen and handling
        public static void createPack(ref HashSet<AvaliPack> packs, Pawn pawn,string reason = null)
        {
            AvaliPack newPack = EiCreatePack(pawn);
            if (!packs.Contains(newPack)) { packs.Add(newPack); }
            if (RimValiMod.settings.enableDebugMode && reason != null) { Log.Message($"Creating pack for reason: {reason}"); }
        }

        /// <summary>
        /// Handles the job of managing pack related functions, such as creating packs for a pawn, making a pawn join packs, etc.
        /// </summary>
        /// <param name="packs"></param>
        /// <param name="pawn"></param>
        /// <param name="racesInPacks"></param>
        /// <param name="packLimit"></param>
        /// <returns></returns>

        public static HashSet<AvaliPack> KioPackHandler(ref HashSet<AvaliPack> packs, Pawn pawn, int packLimit)
        {
            if (pawn != null && pawn.Spawned && pawn.Alive() && pawn.story!=null && pawn.story.traits!=null && !pawn.story.traits.HasTrait(AvaliDefs.AvaliPackBroken))
            {
                bool hasPack = pawn.GetPack() != null;
                if (!packs.EnumerableNullOrEmpty())
                {
                    IEnumerable<AvaliPack> packsToUse = packs;
                    packsToUse = packs.Where(pack => pack != null && !pack.GetAllNonNullPawns.EnumerableNullOrEmpty() && 
                                            (hasPack ? pack != pawn.GetPack() : true) 
                                            && pack.GetAllNonNullPawns.Count < packLimit && 
                                            pack.faction == pawn.Faction && 
                                            pack.GetAllNonNullPawns.Any(packmate => packmate.Spawned && packmate.Alive() && packmate.Map != null && packmate.Map == pawn.Map));
                      
                    //We only want to run this if there are packs, otherwise we'll automatically make a "base" pack.
                    if (!packsToUse.EnumerableNullOrEmpty())
                    {
                        AvaliPack pack2 = packsToUse.RandomElement();
                        AvaliPack pack = hasPack ? pawn.GetPack() : null;
                        //This checks if a pack only has one pawn or if it is null, and also if we can join a random pack.
                        if ((pack == null || (pack.GetAllNonNullPawns.EnumerableNullOrEmpty() || pack.GetAllNonNullPawns.Count == 1)) && (pack2 != null && !pack2.pawns.EnumerableNullOrEmpty()))
                        {
                            if (pack != null) { packs.Remove(pack); } 
                            JoinPack(pawn, ref pack2);
                        }
                    }
                    else{createPack(ref packs,pawn,"No packs in toUse list");}
                }
                if (packs.EnumerableNullOrEmpty())
                {
                    if (packs == null){packs = new HashSet<AvaliPack>();}
                    createPack(ref packs, pawn, "packs list was null or empty");
                }
                #region cleanup
                //Avoids pawns with double packs
                pawn.CleanupPacks();
                AvaliPack pawnPack = pawn.GetPack();
                //Does pack boosts
                if (pawnPack != null && pawnPack.GetAllNonNullPawns.Count>1)
                {
                    pawnPack.CleanupPack(pawn);
                    pawnPack.UpdateHediffForAllMembers();
                }
                //This automatically updates if a pawn can join a pack without opinion and pack loss.
                pawn.UpdatePackAvailibilty();
                #endregion
            }
            return packs;
        }
        #region get opinion and joining
        public static float GetPackAvgOP(AvaliPack pack, Pawn pawn)
        {
            HashSet<float> opinions = new HashSet<float>();

            foreach (Pawn packmember in pack.GetAllNonNullPawns) { opinions.Add(packmember.relations.OpinionOf(pawn)); }
            return Queryable.Average(opinions.AsQueryable());
        }

        public static AvaliPack JoinPack(Pawn pawn, ref AvaliPack pack)
        {
            Date date = new Date();
            AvaliPackDriver AvaliPackDriver = Current.Game.GetComponent<AvaliPackDriver>();


            if ((!AvaliPackDriver.pawnsHaveHadPacks.ContainsKey(pawn) || !AvaliPackDriver.pawnsHaveHadPacks[pawn]) && date.ToString() == pack.creationDate.ToString()){pack.pawns.Add(pawn);}
            else if (GetPackAvgOP(pack, pawn) >= LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packOpReq){pack.pawns.Add(pawn);}
            return pack;
        }
        #endregion
        public static AvaliPack EiCreatePack(Pawn pawn)
        {
            AvaliPack PawnPack = new AvaliPack(pawn.Faction)
            {
                name = pawn.Name.ToStringShort + "'s pack",
                pawns = new HashSet<Pawn> { pawn }
            };
            if (RimValiMod.settings.enableDebugMode){Log.Message("Creating pack: " + PawnPack.name);}
            return PawnPack;
        }
        #region handling packs
        public static void CleanupPacks(this Pawn pawn)
        {
            if (pawn.hasPack())
            {
                AvaliPackDriver AvaliPackDriver = Current.Game.GetComponent<AvaliPackDriver>();
                while (AvaliPackDriver.packs.Where(p => p.GetAllNonNullPawns.Contains(pawn)).Count() > 1) { AvaliPackDriver.packs.Remove(AvaliPackDriver.packs.Where(p => p.GetAllNonNullPawns.Contains(pawn)).Last()); }

                AvaliPack pack = pawn.GetPack();
                pack.pawns.RemoveWhere(packPawn => packPawn == null);
            }
        }
        public static void UpdatePackAvailibilty(this Pawn pawn)
        {
            if (pawn.GetPack() != null && pawn.GetPack().GetAllNonNullPawns.Count >= 2)
            {
                AvaliPackDriver AvaliPackDriver = Current.Game.GetComponent<AvaliPackDriver>();
                if (!AvaliPackDriver.pawnsHaveHadPacks.ContainsKey(pawn)){ AvaliPackDriver.pawnsHaveHadPacks.Add(pawn, true); }
                AvaliPackDriver.pawnsHaveHadPacks[pawn] = true;
            }
        }
        #endregion
        #region getting pack info
        public static AvaliPack GetPack(this Pawn pawn) {
            if (pawn == null)
            {
                return null;    
            }
            AvaliPackDriver driver = Current.Game.GetComponent<AvaliPackDriver>();
            if (driver.packs.EnumerableNullOrEmpty())
            {
                if (RimValiMod.settings.enableDebugMode) { Log.Error("Pawn check is null, or no packs!"); }
                return null;
            }
            
            if (driver != null && driver.packs.Any(x=>x.GetAllNonNullPawns.Contains(pawn))) {
                AvaliPack pack = driver.packs.First(x => x.GetAllNonNullPawns.Contains(pawn));
                return pack;
            }
            return null;
        }

        public static bool hasPack(this Pawn pawn)
        {
            return pawn.GetPack() != null && !pawn.GetPack().GetAllNonNullPawns.EnumerableNullOrEmpty();
        }
        public static AvaliPack GetPackWithoutSelf(this Pawn pawn)
        {
            if (pawn == null)
            {
                return null;
            }
            AvaliPackDriver AvaliPackDriver = Current.Game.GetComponent<AvaliPackDriver>();
            if (AvaliPackDriver.packs.EnumerableNullOrEmpty())
            {
                if (RimValiMod.settings.enableDebugMode){Log.Error("Pawn check is null, or no packs!");}
                return null;
            }
            //We really should be getting here
            if (AvaliPackDriver.packs.Any(x=>x.GetAllNonNullPawns.Contains(pawn)))
            {
                AvaliPack pack = AvaliPackDriver.packs.First(x=>x.GetAllNonNullPawns.Contains(pawn));
                if (pack != null)
                {
                    AvaliPack returnPack = new AvaliPack(pack.faction);
                    returnPack.pawns.AddRange(pack.pawns);
                    returnPack.pawns.Remove(pawn);
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
        /*
        
        [DebugAction("RimVali","Reset packs",allowedGameStates =AllowedGameStates.PlayingOnMap)]
        public static void ResetPacks()
        {
            AvaliPackDriver AvaliPackDriver = Current.Game.GetComponent<AvaliPackDriver>();
            AvaliPackDriver.packs = new HashSet<AvaliPack>();
            AvaliPackDriver.pawnsHaveHadPacks = new Dictionary<Pawn, bool>();
            AvaliPackDriver.pawns = new List<Pawn>();
            AvaliPackDriver.bools = new List<bool>();
            Log.Message("Packs reset");
        }

        [DebugAction("RimVali", "Reset pack loss", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ResetPackLoss()
        {
            foreach (Pawn pawn in RimValiCore.RimValiUtility.AllPawnsOfRaceInWorld(new List<ThingDef> { AvaliDefs.RimVali, AvaliDefs.IWAvaliRace }))
            {
                PackComp packComp = pawn.TryGetComp<PackComp>();
                if(packComp != null)
                {
                    packComp.ticksSinceLastInpack = 0;
                    Log.Message($"Reset {pawn.Name}'s pack loss");
                }
            }
        }*/
        #endregion
    }
    /*
     * I need to figure out what on earth is going on with my references.
     * ;-;
    public class RimValiDebug
    {
        [DebugAction("General", null, false, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SubtractTime()
        {
            AirDropHandler.timeToDrop = 10 * 25;
        }

        [DebugAction("RimVali", "Airdrop now", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public void DropNow()
        {
            AirDropHandler.timeToDrop = 0;
        }
    }*/
}