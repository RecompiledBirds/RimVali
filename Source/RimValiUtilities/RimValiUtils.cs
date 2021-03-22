using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using Verse;
using System;

namespace AvaliMod
{
    public static class RimValiUtility
    {
        public static string build = "Ki 0.0.8";
        public static string modulesFound = "Modules:\n";


        public static SkillRecord GetHighestSkillOfpack(AvaliPack pack)
        {
            int highestSkillLevel = 0;
            SkillRecord highestSkill = null;
            foreach (Pawn pawn in pack.pawns)
            {

                List<SkillRecord> list = new List<SkillRecord>();
                foreach (SkillRecord skillRecord in pawn.skills.skills)
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


     
        public static bool PackInBedroom(this Pawn pawn)
        {
            int packmatesInRoom = 0;
            Room room = pawn.GetRoom();
            AvaliPack avaliPack = pawn.GetPackWithoutSelf();
            if (room != null &&  avaliPack != null && room.ContainedBeds.Count() > 0)
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
            Room room = pawn.GetRoom();
            if (room != null && pawn.Position.Roofed(pawn.Map))
            {
                AvaliPack pack = GetPackWithoutSelf(pawn);
                if(pack != null)
                {
                    foreach(Pawn p in pack.pawns)
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
       


        public static int GetPackSize(Pawn pawn) => pawn.GetPack() != null ? pawn.GetPack().pawns.Count(): 0;
        public static IEnumerable<Pawn> AllPawnsOfRaceOnMap(List<ThingDef> races, Map map) => map.mapPawns.AllPawns.Where(x => races.Contains(x.def));

        public static IEnumerable<Pawn> AllPawnsOfRaceOnMap(ThingDef race, Map map) => map.mapPawns.AllPawns.Where(x => x.def == race);


        public static IEnumerable<Pawn> AllPawnsOfRaceInWorld(List<ThingDef> races) => PawnsFinder.All_AliveOrDead.Where(pawn => !pawn.Dead && races.Contains(pawn.def));
        public static IEnumerable<Pawn> AllPawnsOfRaceInWorld(ThingDef race) => PawnsFinder.All_AliveOrDead.Where(pawn => !pawn.Dead && pawn.def == race);
        public static IEnumerable<Pawn> AllPawnsOfRaceInWorld(ThingDef race, Faction faction) => PawnsFinder.All_AliveOrDead.Where(pawn => !pawn.Dead && pawn.Faction == faction && pawn.def == race);


        public static IEnumerable<Pawn> AllPawnsOfRaceInMapAndFaction(Pawn pawn) => CheckAllPawnsInMapAndFaction(pawn.Map, pawn.Faction).Where(x => x.def == pawn.def);

        public static IEnumerable<Pawn> AllPawnsOfRaceInMapAndFaction(ThingDef race, Map map, Faction faction) => CheckAllPawnsInMapAndFaction(map, faction).Where(x => x.def == race);


        public static IEnumerable<Pawn> CheckAllPawnsInMapAndFaction(Map map, Faction faction) => PawnsFinder.AllMaps_SpawnedPawnsInFaction(faction).Where(x => x.Map == map);


        /*
        public static IEnumerable<Pawn> GetPackPawns(Pawn pawn, PawnRelationDef relationDef)
        {
            List<Pawn> packmates = new List<Pawn>();
            IEnumerable<Pawn> pawns = pawn.relations.RelatedPawns.ToList<Pawn>();
            foreach (Pawn packmate in pawns)
            {
                if (packmate.relations.DirectRelationExists(relationDef, pawn))
                {
                    packmates.Add(packmate);
                }
            }
            return packmates;
        }
        */
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

        public static IEnumerable<Pawn> PawnsOfRaceInFaction(ThingDef race, Faction faction) => PawnsFinder.AllMaps_SpawnedPawnsInFaction(faction).Where(x=>IsOfRace(x,race));


        public static bool FactionHasRace(ThingDef race, Faction faction) => PawnOfRaceCount(faction, race) > 0;

        /// <summary>
        /// Handles the job of managing pack related functions, such as creating packs for a pawn, making a pawn join packs, etc.
        /// </summary>
        /// <param name="packs"></param>
        /// <param name="pawn"></param>
        /// <param name="racesInPacks"></param>
        /// <param name="packLimit"></param>
        /// <returns></returns>
        public static List<AvaliPack> EiPackHandler(List<AvaliPack> packs, Pawn pawn, IEnumerable<ThingDef> racesInPacks, int packLimit)
        {
            AvaliPackDriver AvaliPackDriver = Current.Game.GetComponent<AvaliPackDriver>();
            void createPack()
            {
                AvaliPack newPack = EiCreatePack(pawn);
                if (!AvaliPackDriver.packs.Contains(newPack)) { packs.Add(newPack); }
                Log.Message("Number of packs: " + packs.Count);
            }

            //We only want to run this if there are packs, otherwise we'll automatically make a "base" pack.
            if (packs.Count > 0)
            {
                //If the pawn isn't spawned, we don't care.
                if (!pawn.Spawned)
                {
                    return packs;
                }
                IEnumerable<AvaliPack> packsToUse = packs.Where<AvaliPack>(x => x.faction == pawn.Faction && x.pawns.Any(p=>p.Map==pawn.Map));
                if (packsToUse.Count() <= 0)
                {
                    createPack();
                }
                AvaliPack pack = packs.Find(x => x.pawns.Contains(pawn));
                //This checks if a pack only has one pawn or if it is null.
                if ((pack != null && !pack.pawns.NullOrEmpty() && pack.pawns.Count == 1) || pack ==null)
                {
                    AvaliPack pack2 = packsToUse.ToList().Find(p => p.pawns.Count < packLimit);
                    if (pack2 != null)
                    {
                        //If we did have a previous pack, just remove it. It's not needed anymore.
                        if (pack != null)
                        {
                            AvaliPackDriver.packs.Remove(pack);
                        }
                        pack2 = JoinPack(pawn, pack2);
                    }
                }
                else
                {
                    createPack();
                }

            }
            else
            {
                createPack();
            }
            pawn.UpdatePackAvailibilty();
            return packs;
        }

        public static float GetPackAvgOP(AvaliPack pack, Pawn pawn)
        {
            List<float> opinions = new List<float>();
            foreach(Pawn packmember in pack.pawns)
            {
                opinions.Add(packmember.relations.OpinionOf(pawn));

            }
            return Queryable.Average(opinions.AsQueryable());
        }

        public static AvaliPack JoinPack(Pawn pawn, AvaliPack pack)
        {
            Date date = new Date();
            AvaliPackDriver AvaliPackDriver = Current.Game.GetComponent<AvaliPackDriver>();


            if (!AvaliPackDriver.pawnsHaveHadPacks.ContainsKey(pawn) || !AvaliPackDriver.pawnsHaveHadPacks[pawn])
            {
                if (date.ToString() == pack.creationDate.ToString())
                {
                    pack.pawns.Add(pawn);
                    return pack;
                }

            }
            else
            {
                Log.Message("Pawn was in Dict");
                if(GetPackAvgOP(pack, pawn) >= LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packOpReq)
                {
                    pack.pawns.Add(pawn);
                    return pack;
                }
                Log.Message(GetPackAvgOP(pack, pawn).ToString() + pawn.Name.ToString());
            }
            return null;
        }
        public static AvaliPack EiCreatePack(Pawn pawn)
        {
            AvaliPackDriver AvaliPackDriver = Current.Game.GetComponent<AvaliPackDriver>();
            AvaliPack PawnPack = new AvaliPack(pawn.Faction);
            PawnPack.name = pawn.Name.ToStringShort + "'s pack";
            Log.Message("Creating pack: " + PawnPack.name);
            PawnPack.pawns.Add(pawn);
            return PawnPack;
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
                Log.Error("Pawn check is null!");
                return null;
            }
            if (AvaliPackDriver.packs == null || AvaliPackDriver.packs.Count == 0)
            {
                Log.Message("No packs");
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
    }
}