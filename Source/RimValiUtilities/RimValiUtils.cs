using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using Verse;
using System;
using RimWorld.Planet;
using System.Reflection;

namespace AvaliMod
{
    
    public static class RimValiUtility
    {
        public static string build = "Kesuni 1.0.0";
        public static string modulesFound = "Modules:\n";


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
            foreach (Pawn pawn in pack.pawns)
            {

                List<SkillRecord> list = new List<SkillRecord>();
                if(avoidSkills == null)
                {
                    avoidSkills = new List<SkillDef>();
                }
                foreach (SkillRecord skillRecord in pawn.skills.skills.Where(SkillRec => !avoidSkills.Contains(SkillRec.def)))
                {
                    if (skillRecord.Level > highestSkillLevel)
                    {
                        highestSkillLevel = skillRecord.Level;
                        highestSkill = skillRecord;
                    }
                }

            }
            return highestSkill;
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

        //private static readonly bool enableDebug = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().enableDebugMode;
        public static string dir;
        public static void AssetBundleFinder(DirectoryInfo info)
        {
            foreach (FileInfo file in info.GetFiles())
            {
                if (file.Extension.NullOrEmpty())
                {
                    AssetBundle bundle = AssetBundle.LoadFromFile(file.FullName);
                    if (!(bundle == null))
                    {
                        Log.Message("RimVali loaded bundle: " + bundle.name);
                        UnityEngine.Shader[] shaders = bundle.LoadAllAssets<UnityEngine.Shader>();
                    }
                    else
                    {
                        Log.Message("RimVali was unable to load the bundle: " + file.FullName);
                    }
                }
            }
        }

        public static AssetBundle shaderLoader(string info)
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(info);
            Log.Message("-----------------------------------------");
            Log.Message("Loaded bundle: " + assetBundle.name);
            Log.Message(assetBundle.GetAllAssetNames()[0], false);
            return assetBundle;
        }


        public static Dictionary<Trait, int> Traits(this Pawn pawn)
        {
            IEnumerable<Trait> traits = pawn.story.traits.allTraits;
            Dictionary<Trait, int> traitDataToReturn = new Dictionary<Trait, int>();
            foreach (Trait trait in traits)
            {
                traitDataToReturn.Add(trait, trait.Degree);
            }
            return traitDataToReturn;
        }

        public static float GetRoomQuality(this Pawn pawn)
        {
            Room room = pawn.GetRoom();
            RoomStatWorker_Beauty b = new RoomStatWorker_Beauty();
            return room.GetStat(RoomStatDefOf.Impressiveness);

        }
        public static bool SharedBedroom(this Pawn pawn)
        {
            Room room = pawn.GetRoom();
            if (room != null && room.ContainedBeds.Count() > 0)
            {
                IEnumerable<Building_Bed> beds = room.ContainedBeds;
                return beds.Any(bed => bed.OwnersForReading != null && bed.OwnersForReading.Any(p => p != pawn));
            }
            return false;
        }
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
        public static MethodInfo GetMethod<T>(string methodName, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
        {
            ParameterModifier modifer;
            return typeof(T).GetMethod(methodName, flags);
        }
        public static MethodInfo GetMethod<T>(string methodName, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, ParameterModifier[] modifiers = null)
        {
            return typeof(T).GetMethod(methodName, flags,null,null,modifiers);
        }
        public static void InvokeMethod<T>(string name, T obj, object[] parameters, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
        {
            GetMethod<T>(name, flags)?.Invoke(obj, parameters);
        }
        public static void InvokeMethod<T>(string name, T obj, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
        {
            GetMethod<T>(name, flags)?.Invoke(obj, new object[1]);
        }
        public static T GetVar<T>(string fieldName, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, object obj = null)
        {
            T val = default(T);
            if ((T)typeof(T).GetField(fieldName, flags).GetValue(obj) != null)
            {
                val = (T)typeof(T).GetField(fieldName, flags).GetValue(obj);
            }

            return val;
        }

        public static T GetProp<T>(string propName, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, object obj = null)
        {
            return (T)typeof(T).GetProperty(propName, flags).GetValue(obj);
        }
        public static bool CheckIfPackmatesInRoom(this Pawn pawn) => ((Func<bool>)delegate
        {
            Room room = pawn.GetRoom();
            if (room != null && pawn.Position.Roofed(pawn.Map))
            {
                AvaliPack pack = GetPackWithoutSelf(pawn);
                if (pack != null)
                {
                    foreach (Pawn p in pack.pawns)
                    {
                        if (p.GetRoom() == room)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        })();



        public static int GetPackSize(Pawn pawn) => pawn.GetPack() != null ? pawn.GetPack().pawns.Count() : 0;
        public static IEnumerable<Pawn> AllPawnsOfRaceOnMap(List<ThingDef> races, Map map) => map.mapPawns.AllPawns.Where(x => races.Contains(x.def));

        public static IEnumerable<Pawn> AllPawnsOfRaceOnMap(ThingDef race, Map map) => map.mapPawns.AllPawns.Where(x => x.def == race);


        public static IEnumerable<Pawn> AllPawnsOfRaceInWorld(List<ThingDef> races)=> PawnsFinder.All_AliveOrDead.Where(pawn => !pawn.Dead && races.Contains(pawn.def));
        public static IEnumerable<Pawn> AllPawnsOfRaceInWorld(ThingDef race) => PawnsFinder.All_AliveOrDead.Where(pawn => !pawn.Dead && pawn.def == race);
        public static IEnumerable<Pawn> AllPawnsOfRaceInWorld(ThingDef race, Faction faction) => PawnsFinder.All_AliveOrDead.Where(pawn => !pawn.Dead && pawn.Faction == faction && pawn.def == race);


        public static IEnumerable<Pawn> AllPawnsOfRaceInMapAndFaction(Pawn pawn) => CheckAllPawnsInMapAndFaction(pawn.Map, pawn.Faction).Where(x => x.def == pawn.def);

        public static IEnumerable<Pawn> AllPawnsOfRaceInMapAndFaction(ThingDef race, Map map, Faction faction) => CheckAllPawnsInMapAndFaction(map, faction).Where(x => x.def == race);


        //public static IEnumerable<Pawn> CheckAllPawnsInMapAndFaction(Map map, Faction faction) => PawnsFinder.AllMaps_SpawnedPawnsInFaction(faction).Where(x => x.Map == map);
        public static IEnumerable<Pawn> CheckAllPawnsInMapAndFaction(Map map, Faction faction) => map.mapPawns.AllPawns.Where(x=>x.Faction==faction);

        public static List<Pawn> FetchPawnsOnAllMaps()
        {
            List<Pawn> val = new List<Pawn>();
            if (Current.ProgramState != ProgramState.Entry)
            {
                List<Map> m = Find.Maps;
                if (m.Count == 1)
                {
                    return m[0].mapPawns.AllPawns;
                }
                foreach(Map map in m)
                {
                    val.AddRange(map.mapPawns.AllPawns);
                }
            }
            return val;
        }
        public static List<Pawn> FetchAllAliveOrDeadPawns()
        {
            List<Pawn> val = new List<Pawn>();
            val.AddRange(Find.WorldPawns.AllPawnsAliveOrDead);
            return val;
        }
        
        public static List<Pawn> FetchPawnsSpawnedOnAllMaps()
        {
            List<Pawn> val = new List<Pawn>();
            if (Current.ProgramState != ProgramState.Entry)
            {
                List<Map> m = Find.Maps;
                if (m.Count == 1)
                {
                    return m[0].mapPawns.AllPawns;
                }
                foreach (Map map in m)
                {
                    val.AddRange(map.mapPawns.AllPawnsSpawned);
                }
            }
            return val;
        }
        public static bool IsOfRace(Pawn pawn, ThingDef race) => pawn.def.defName == race.defName;






        public static void RemovePackRelationIfDead(Pawn pawn, List<Pawn> packmates, PawnRelationDef relationDef)
        {
            foreach (Pawn packmate in packmates)
            {
                if (packmate.DestroyedOrNull())
                {
                    pawn.relations.RemoveDirectRelation(relationDef, packmate);
                }
            }
        }


        public static int PawnOfRaceCount(Faction faction, ThingDef race) => PawnsOfRaceInFaction(race, faction).Count();

        public static IEnumerable<Pawn> PawnsOfRaceInFaction(ThingDef race, Faction faction) => FetchPawnsSpawnedOnAllMaps().Where(x => IsOfRace(x, race) && x.Faction==faction);


        public static bool FactionHasRace(ThingDef race, Faction faction) => PawnOfRaceCount(faction, race) > 0;

        /// <summary>
        /// Handles the job of managing pack related functions, such as creating packs for a pawn, making a pawn join packs, etc.
        /// </summary>
        /// <param name="packs"></param>
        /// <param name="pawn"></param>
        /// <param name="racesInPacks"></param>
        /// <param name="packLimit"></param>
        /// <returns></returns>
        #region packs
        public static List<AvaliPack> EiPackHandler(List<AvaliPack> packs, Pawn pawn, int packLimit)
        {
            void createPack(string reason = null)
            {
                AvaliPack newPack = EiCreatePack(pawn);
                if (!packs.Contains(newPack)) { packs.Add(newPack); }
                if (RimValiMod.settings.enableDebugMode && reason != null)
                {

                    Log.Message($"Creating pack for reason: {reason}");
                }
            }

            //We only want to run this if there are packs, otherwise we'll automatically make a "base" pack.
            if (packs.Count > 0 && pawn.Spawned)
            {
                //If the pawn isn't spawned, we don't care.
                if (!pawn.Spawned)
                {
                    return packs;
                }
                IEnumerable<AvaliPack> packsToUse = packs.Where<AvaliPack>(x => x.faction == pawn.Faction && x.pawns.Any(p => p.Map == pawn.Map));
                if (packsToUse.Count() <= 0)
                {
                    createPack("packs is less than or equal to zero");
                }
                AvaliPack pack = pawn.GetPack();
                //This checks if a pack only has one pawn or if it is null, and also if we can join a random pack.
                if (((pack != null && !pack.pawns.NullOrEmpty() && pack.pawns.Count == 1) || pack == null) && packsToUse.Any(p => p != pack && p.pawns.Count < packLimit))
                {
                    AvaliPack pack2 = packsToUse.Where(p => p.pawns.Count < packLimit).RandomElement();
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
                //If we get here, we'll create a new pack.
                else if (!(pack != null && !pack.pawns.NullOrEmpty() && !(pack.pawns.Count == 1)))
                {
                    createPack("No pack for pawn");
                    if (RimValiMod.settings.enableDebugMode)
                    {
                        Log.Message((!(pack != null && !pack.pawns.NullOrEmpty() && !(pack.pawns.Count == 1))).ToString());
                    }
                }

            }
            else
            {
                createPack("No packs in list");
            }
            //Avoids pawns with double packs
            pawn.CleanupPacks();
            //Does pack boosts
            foreach (AvaliPack pack in packs.Where(p => p.pawns.Count > 1))
            {
                if (RimValiMod.settings.enableDebugMode)
                {
                    Log.Message("Updating pack hediffs");
                }
                pack.UpdateHediffForAllMembers();
            }
            //This automatically updates if a pawn can join a pack without opinion and pack loss.
            pawn.UpdatePackAvailibilty();
            return packs;
        }

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
        public static AvaliPack EiCreatePack(Pawn pawn)
        {
            AvaliPackDriver AvaliPackDriver = Current.Game.GetComponent<AvaliPackDriver>();
            AvaliPack PawnPack = new AvaliPack(pawn.Faction);
            PawnPack.name = pawn.Name.ToStringShort + "'s pack";
            if (RimValiMod.settings.enableDebugMode)
            {
                Log.Message("Creating pack: " + PawnPack.name);
            }
            PawnPack.pawns.Add(pawn);
            return PawnPack;
        }
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
        public static AvaliPack GetPack(this Pawn pawn) => pawn != null && Current.Game.GetComponent<AvaliPackDriver>() != null && !Current.Game.GetComponent<AvaliPackDriver>().packs.NullOrEmpty() ? ((Func<AvaliPack>)delegate
      {
          foreach (AvaliPack pack in Current.Game.GetComponent<AvaliPackDriver>().packs)
          {
              if (pack.pawns.Contains(pawn))
              {
                  return pack;
              }
          }

          return null;

      })() : null;


        public static AvaliPack GetPackWithoutSelf(this Pawn pawn)
        {
            AvaliPackDriver AvaliPackDriver = Current.Game.GetComponent<AvaliPackDriver>();
            if (pawn == null)
            {
                if (RimValiMod.settings.enableDebugMode)
                {
                    Log.Error("Pawn check is null!");
                }
                return null;
            }
            if (AvaliPackDriver.packs == null || AvaliPackDriver.packs.Count == 0)
            {
                if (RimValiMod.settings.enableDebugMode)
                {
                    Log.Message("No packs");
                }
                return null;
            }
            //We really should be getting here
            if (AvaliPackDriver.packs.Count > 0)
            {
                foreach (AvaliPack APack in AvaliPackDriver.packs)
                {
                    //Checks if somehow a pack has 0 pawns (never should happen), then checks if the pawn is in it.
                    if (APack.pawns.Count > 0)
                    {
                        if (APack.pawns.Contains(pawn))
                        {
                            AvaliPack returnPack = new AvaliPack(APack.faction);
                            returnPack.pawns.AddRange(APack.pawns);
                            returnPack.pawns.Remove(pawn);
                            //returnPack.size = APack.size;
                            returnPack.deathDates.AddRange(APack.deathDates);
                            returnPack.creationDate = APack.creationDate;
                            return returnPack;
                        }
                    }
                }
            }
            else
            {
                return null;
            }
            //If somehow nothing worked, just return null.
            /*if (enableDebug)
            {
                Log.Message("Didn't find pack, returning null.");
            }*/
            return null;
        }
        #endregion
    }
}