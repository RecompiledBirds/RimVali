using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using System;

namespace AvaliMod
{
    
    public static class RimValiUtility
    {
        public static string build = "Kesuni 1.0.0";
        public static string modulesFound = "Modules:\n";

        #region pack skills
        public static SkillRecord GetHighestSkillOfpack(AvaliPack pack)
        {
            int highestSkillLevel = 0;
            SkillRecord highestSkill = null;
            foreach (Pawn pawn in pack.pawns)
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
            if (!pack.pawns.NullOrEmpty())
            {
                foreach (Pawn pawn in pack.pawns.Where(x=>x.skills!=null&&!x.skills.skills.NullOrEmpty()&&x.Spawned))
                {
                    if (avoidSkills == null)
                    {
                        avoidSkills = new List<SkillDef>();
                    }
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
            int packmatesInRoom = 0;
            Room room = pawn.GetRoom();
            AvaliPack avaliPack = pawn.GetPackWithoutSelf();
            if (room != null && avaliPack != null && room.ContainedBeds.Count() > 0)
            {
                IEnumerable<Building_Bed> beds = room.ContainedBeds;
                if (beds.Count() < 2)
                {
                    return false;
                }
                foreach (Building_Bed bed in beds)
                {
                    if (bed.OwnersForReading != null)
                    {
                        foreach (Pawn other in bed.OwnersForReading)
                        {
                            if (avaliPack.pawns.Contains(other))
                            {
                                packmatesInRoom += 1;
                            }
                        }
                    }
                }
            }
            if (packmatesInRoom > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public static bool CheckIfPackmatesInRoom(this Pawn pawn) => ((Func<bool>)delegate
        {
            if (pawn.Spawned && pawn.Map!=null)
            {
                AvaliPack pack = GetPackWithoutSelf(pawn);
                Room room = pawn.GetRoom();
                if (room != null && pawn.Position.Roofed(pawn.Map) && pack != null && !pack.pawns.NullOrEmpty())
                {
                    return pack.pawns.Any(packmate => packmate.Spawned && packmate.Map == pawn.Map && packmate.GetRoom() != null && packmate.GetRoom() == room);

                }
            }
            return false;
        })();
        #endregion

        #region pack gen and handling
        /// <summary>
        /// Handles the job of managing pack related functions, such as creating packs for a pawn, making a pawn join packs, etc.
        /// </summary>
        /// <param name="packs"></param>
        /// <param name="pawn"></param>
        /// <param name="racesInPacks"></param>
        /// <param name="packLimit"></param>
        /// <returns></returns>

        public static List<AvaliPack> EiPackHandler(List<AvaliPack> packs, Pawn pawn, int packLimit)
        {
            if (!pawn.story.traits.HasTrait(AvaliDefs.AvaliPackBroken) && !pawn.Dead)
            {
                #region gen new pack
                void createPack(string reason = null)
                {
                    AvaliPack newPack = EiCreatePack(pawn);
                    if (!packs.Contains(newPack)) { packs.Add(newPack); }
                    if (RimValiMod.settings.enableDebugMode && reason != null)
                    {

                        Log.Message($"Creating pack for reason: {reason}");
                    }
                }
                #endregion
                //We only want to run this if there are packs, otherwise we'll automatically make a "base" pack.
                if (!packs.NullOrEmpty()&&packs.Count> 0)
                {
                    #region getting packs avali-ble to us
                    IEnumerable<AvaliPack> packsToUse = packs.Where<AvaliPack>(x => x.faction == pawn.Faction && x.pawns.Any(p => p.Map == pawn.Map));
                    if (packsToUse.EnumerableNullOrEmpty() || packsToUse.Count() <= 0)
                    {
                        createPack("packs is less than or equal to zero");
                    }
                    #endregion
                    AvaliPack pack = pawn.GetPack();
                    //This checks if a pack only has one pawn or if it is null, and also if we can join a random pack.
                    if ((pack == null || !pack.pawns.NullOrEmpty() && pack.pawns.Count == 1) && packsToUse.Any(p => p != pack && p.pawns.Count < packLimit && p.pawns.Any(x => x.Spawned && x.Map == pawn.Map)))
                    {
                        AvaliPack pack2 = packsToUse.Where(p => p.pawns.Count < packLimit && p.pawns.Any(x => x.Spawned && x.Map == pawn.Map)).RandomElement();
                        if (pack2 != null && pack2.pawns.Count < packLimit)
                        {
                            //If we did have a previous pack, just remove it. It's not needed anymore.
                            if (pack != null)
                            {
                                packs.Remove(pack);
                            }
                            JoinPack(pawn, ref pack2);
                        }
                    }
                }
                else
                {
                    if (packs.NullOrEmpty())
                    {
                        packs = new List<AvaliPack>();
                    }
                    createPack("No packs in list");
                }
                #region cleanup
                //Avoids pawns with double packs
                pawn.CleanupPacks();
                //Does pack boosts
                if(pawn.GetPack() != null)
                {
                    pawn.GetPack().CleanupPack();
                    pawn.GetPack().UpdateHediffForAllMembers();
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
            List<float> opinions = new List<float>();
            foreach (Pawn packmember in pack.pawns)
            {
                opinions.Add(packmember.relations.OpinionOf(pawn));

            }
            return Queryable.Average(opinions.AsQueryable());
        }

        public static AvaliPack JoinPack(Pawn pawn, ref AvaliPack pack)
        {
            Date date = new Date();
            AvaliPackDriver AvaliPackDriver = Current.Game.GetComponent<AvaliPackDriver>();


            if (!AvaliPackDriver.pawnsHaveHadPacks.ContainsKey(pawn) || !AvaliPackDriver.pawnsHaveHadPacks[pawn] && date.ToString() == pack.creationDate.ToString())
            {
                pack.pawns.Add(pawn);
                return pack;
            }
            else
            {
                if (GetPackAvgOP(pack, pawn) >= LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packOpReq)
                {
                    pack.pawns.Add(pawn);
                    return pack;
                }
            }
            return pack;
        }
        #endregion
        public static AvaliPack EiCreatePack(Pawn pawn)
        {
            AvaliPackDriver AvaliPackDriver = Current.Game.GetComponent<AvaliPackDriver>();
            AvaliPack PawnPack = new AvaliPack(pawn.Faction)
            {
                name = pawn.Name.ToStringShort + "'s pack",
                pawns = new List<Pawn> { pawn }
            };
            if (RimValiMod.settings.enableDebugMode)
            {
                Log.Message("Creating pack: " + PawnPack.name);
            }
            return PawnPack;
        }
        #region handling packs
        public static void CleanupPacks(this Pawn pawn)
        {
            AvaliPackDriver AvaliPackDriver = Current.Game.GetComponent<AvaliPackDriver>();
            while (AvaliPackDriver.packs.Where(p => p.pawns.Contains(pawn)).Count() > 1)
            {
                AvaliPackDriver.packs.Remove(AvaliPackDriver.packs.Where(p => p.pawns.Contains(pawn)).Last());
            }
        }
        public static void UpdatePackAvailibilty(this Pawn pawn)
        {
            if (pawn.GetPack() != null && pawn.GetPack().pawns.Count >= 2)
            {
                AvaliPackDriver AvaliPackDriver = Current.Game.GetComponent<AvaliPackDriver>();
                if (!AvaliPackDriver.pawnsHaveHadPacks.ContainsKey(pawn))
                {
                    AvaliPackDriver.pawnsHaveHadPacks.Add(pawn, true);
                }
                AvaliPackDriver.pawnsHaveHadPacks[pawn] = true;
            }
        }
        #endregion
        #region getting pack info
        public static AvaliPack GetPack(this Pawn pawn) => pawn != null && Current.Game.GetComponent<AvaliPackDriver>() != null && !Current.Game.GetComponent<AvaliPackDriver>().packs.NullOrEmpty() ? ((Func<AvaliPack>)delegate
      {
          return Current.Game.GetComponent<AvaliPackDriver>().packs.Find(x => x.pawns.Contains(pawn));
          

      })() : null;


        public static AvaliPack GetPackWithoutSelf(this Pawn pawn)
        {
            AvaliPackDriver AvaliPackDriver = Current.Game.GetComponent<AvaliPackDriver>();
            if (pawn == null | AvaliPackDriver.packs == null || AvaliPackDriver.packs.Count == 0)
            {
                if (RimValiMod.settings.enableDebugMode)
                {
                    Log.Error("Pawn check is null, or no packs!");
                }
                return null;
            }
            //We really should be getting here
            if (AvaliPackDriver.packs.Count > 0)
            {
                AvaliPack pack = AvaliPackDriver.packs.Find(x=>x.pawns.Contains(pawn));
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
            //If somehow nothing worked, just return null.
            /*if (enableDebug)
            {
                Log.Message("Didn't find pack, returning null.");
            }*/
            return null;
        }
        #endregion
        #endregion

        #region debug
        [DebugAction("RimVali","Reset packs",allowedGameStates =AllowedGameStates.PlayingOnMap)]
        public static void ResetPacks()
        {
            AvaliPackDriver AvaliPackDriver = Current.Game.GetComponent<AvaliPackDriver>();
            AvaliPackDriver.packs = new List<AvaliPack>();
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
        }
        #endregion
    }
}