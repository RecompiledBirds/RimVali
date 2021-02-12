using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using Verse;
namespace AvaliMod
{
    public static class RimValiUtility
    {
        public static string build = "Nesuni 0.0.7 ";
        public static string modulesFound ="Modules:\n";
        

        public static SkillRecord GetHighestSkillOfpack(AvaliPack pack)
        {
            int highestSkillLevel = 0;
            SkillRecord highestSkill = null;
            foreach(Pawn pawn in pack.pawns)
            {
   
                List<SkillRecord> list = new List<SkillRecord>();
                foreach(SkillRecord skillRecord in pawn.skills.skills)
                {
                    list.Add(skillRecord);
                    foreach(SkillRecord skill in list)
                    {
                        if(skill.Level > highestSkillLevel)
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
            int highestSkillLevel = 0;
            SkillRecord highestSkill = null;
            foreach (Pawn pawn2 in pack.pawns)
            {
                List<SkillRecord> list = new List<SkillRecord>();
                foreach (SkillRecord skillRecord in pawn2.skills.skills)
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
       
        //private static readonly bool enableDebug = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().enableDebugMode;
        public static string dir;
        public static void AssetBundleFinder(DirectoryInfo info)
         {
             foreach(FileInfo file in info.GetFiles())
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


        public static Dictionary<Trait, int> Traits(Pawn pawn)
        {
            IEnumerable<Trait> traits = pawn.story.traits.allTraits;
            Dictionary<Trait, int> traitDataToReturn = new Dictionary<Trait, int>();
            foreach (Trait trait in traits)
            {
                traitDataToReturn.Add(trait, trait.Degree);
            }
            return traitDataToReturn;
        }


        /*public static bool CheckIfBedRoomHasPackmates(Pawn pawn, PawnRelationDef relationDef)
        {
            // liQdComment - Needs to be changed to non-relation
            int packmatesFound = 0;
            Room room = pawn.GetRoom();
            if (!pawn.Awake())
            {
                if (room.ContainedBeds.Count() > 0)
                {
                    IEnumerable<Building_Bed> beds = room.ContainedBeds;
                    foreach (Building_Bed bed in beds)
                    {
                        if (bed.OwnersForReading != null)
                        {
                            IEnumerable<Pawn> owners = bed.OwnersForReading;
                            foreach (Pawn other in owners)
                            {
                                if (other.relations.DirectRelationExists(relationDef, pawn))
                                {
                                    packmatesFound += 1;
                                }
                            }
                        }
                    }
                }
                if (packmatesFound > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }*/
        public static bool PackInBedroom(Pawn pawn)
        {
            int packmatesInRoom = 0;
            Room room = pawn.GetRoom();
            AvaliPack avaliPack = GetPackWithoutSelf(pawn);
            if (room.ContainedBeds.Count() > 0)
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



        public static bool CheckIfPackmatesInRoom(Pawn pawn)
        {
            Room room = pawn.GetRoom();
            if (!(room == null) && (pawn.Position.Roofed(pawn.Map)))
            {
                AvaliPack pack = GetPackWithoutSelf(pawn);
                foreach (Pawn packmate in pack.pawns)
                {
                    if (packmate.GetRoom() == room)
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }


        public static int GetPackSize(Pawn pawn)
        {
            /*
            int foundMembers = 1;
            IEnumerable<Pawn> relatedPawns = pawn.relations.RelatedPawns;
            foreach (Pawn packmate in relatedPawns)
            {
                if (packmate.relations.DirectRelationExists(relationDef, pawn) || pawn.relations.DirectRelationExists(relationDef, packmate))
                {
                    foundMembers += 1;
                }
            }
            return foundMembers;
            */
            int i = 0;
            try
            {
                i = GetPack(pawn).pawns.Count();
            }
            catch
            {
                //Log.Message("Pawn has no pack");
            }
            return i;
        }
        public static IEnumerable<Pawn> AllPawnsOfRaceOnMap(List<ThingDef> races, Map map) => map.mapPawns.AllPawns.Where(x => races.Contains(x.def));

        public static IEnumerable<Pawn> AllPawnsOfRaceOnMap(ThingDef race, Map map) => map.mapPawns.AllPawns.Where(x => x.def == race);

        public static IEnumerable<Pawn> AllPawnsOfRaceInWorld(List<ThingDef> races)
        {
            IEnumerable<Pawn> pawns = PawnsFinder.All_AliveOrDead;
            IEnumerable<Pawn> caravanPawns = PawnsFinder.AllCaravansAndTravelingTransportPods_AliveOrDead;
            List<Pawn> pawnsToReturn = new List<Pawn>();
            foreach (Pawn pawn in pawns)
            {
                if (races.Contains(pawn.def))
                {
                    pawnsToReturn.Add(pawn);
                }
            }
            foreach(Pawn pawn in caravanPawns)
            {
                if (races.Contains(pawn.def) && !pawnsToReturn.Contains(pawn))
                {
                    pawnsToReturn.Add(pawn);
                }
            }
            return pawnsToReturn;
        }
        public static IEnumerable<Pawn> AllPawnsOfRaceInWorld(ThingDef race)
        {
            IEnumerable<Pawn> pawns = PawnsFinder.All_AliveOrDead.Where<Pawn>(x => !x.Dead);
            IEnumerable<Pawn> caravanPawns = PawnsFinder.AllCaravansAndTravelingTransportPods_AliveOrDead;
            List<Pawn> pawnsToReturn = new List<Pawn>();
            foreach (Pawn pawn in pawns)
            {
                if (pawn.def == race)
                {
                    pawnsToReturn.Add(pawn);
                }
            }
            foreach (Pawn pawn in caravanPawns)
            {
                if (pawn.def == race && !pawnsToReturn.Contains(pawn))
                {
                    pawnsToReturn.Add(pawn);
                }
            }
            return pawnsToReturn;
        }

        public static IEnumerable<Pawn> AllPawnsOfRaceInWorld(ThingDef race, Faction faction)
        {
            IEnumerable<Pawn> pawns = PawnsFinder.All_AliveOrDead.Where<Pawn>(x => x.Faction == faction && !x.Dead);
            IEnumerable<Pawn> caravanPawns = PawnsFinder.AllCaravansAndTravelingTransportPods_AliveOrDead;
            List<Pawn> pawnsToReturn = new List<Pawn>();
            foreach (Pawn pawn in pawns)
            {
                if (pawn.def == race)
                {
                    pawnsToReturn.Add(pawn);
                }
            }
            foreach (Pawn pawn in caravanPawns)
            {
                if (pawn.def == race && !pawnsToReturn.Contains(pawn))
                {
                    pawnsToReturn.Add(pawn);
                }
            }
            return pawnsToReturn;
        }

        public static IEnumerable<Pawn> AllPawnsOfRaceInMapAndFaction(Pawn pawn) => CheckAllPawnsInMapAndFaction(pawn.Map, pawn.Faction).Where(x => x.def == pawn.def);

        public static IEnumerable<Pawn> AllPawnsOfRaceInMapAndFaction(ThingDef race, Map map, Faction faction) => CheckAllPawnsInMapAndFaction(map, faction).Where(x => x.def == race);


        public static IEnumerable<Pawn> CheckAllPawnsInMapAndFaction(Map map, Faction faction) => PawnsFinder.AllMaps_SpawnedPawnsInFaction(faction).Where(x => x.Map == map);


        public static void AddThought(Pawn pawn, ThoughtDef thought)
        {
            pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
        }
        public static void AddThought(Pawn pawn, ThoughtDef thought, int stage)
        {
            pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
            pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(thought).SetForcedStage(stage);
        }
        public static void RemoveThought(Pawn pawn, ThoughtDef thought)
        {
            pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(thought);
        }
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

        public static void ThoughtWithStage(Pawn pawn, ThoughtDef thought, int stage)
        {
            Thought_Memory thought1 = (Thought_Memory)ThoughtMaker.MakeThought(thought);
            pawn.needs.mood.thoughts.memories.TryGainMemory(thought1, null);
            pawn.needs.mood.thoughts.memories.OldestMemoryOfDef(thought).SetForcedStage(stage);

        }

        public static bool AllowDropWhenPawnCountMet(IncidentParms parms, List<ThingDef> things, ThingDef race, int requiredCount)
        {
            List<Thing> thingList = new List<Thing>();
            foreach (ThingDef thingDef in things)
            {
                thingList.Add(ThingMaker.MakeThing(thingDef));
            }
            Map target = (Map)parms.target;
            IntVec3 intVec3 = DropCellFinder.TradeDropSpot(target);
            if (RimValiUtility.PawnOfRaceCount(Faction.OfPlayer, race) >= requiredCount)
            {
                DropPodUtility.DropThingsNear(intVec3, target, (IEnumerable<Thing>)thingList);
            }
            return true;
        }
        public static int PawnOfRaceCount(Faction faction, ThingDef race) => PawnsOfRaceInFaction(race, faction).Count();

        public static IEnumerable<Pawn> PawnsOfRaceInFaction(ThingDef race, Faction faction) => PawnsFinder.AllMaps_SpawnedPawnsInFaction(faction).Where(x=>IsOfRace(x,race));


        public static bool FactionHasRace(ThingDef race, Faction faction) => PawnOfRaceCount(faction, race) > 0;


        public static List<AvaliPack> EiPackHandler(List<AvaliPack> packs, Pawn pawn, IEnumerable<ThingDef> racesInPacks, int packLimit)
        {
            AvaliPackDriver AvaliPackDriver = Current.Game.GetComponent<AvaliPackDriver>();
            void createPack()
            {
                AvaliPack newPack = EiCreatePack(pawn);
                if (!AvaliPackDriver.packs.Contains(newPack)){ packs.Add(newPack); }
                Log.Message("Number of packs: " + packs.Count);
            }
            if (packs.Count > 0)
            {
                if (!pawn.Spawned)
                {
                    return packs;
                }
                List<AvaliPack> packsToUse = packs.Where<AvaliPack>(x => x.faction == pawn.Faction).ToList();
                if (packsToUse.Count <= 0)
                {
                    AvaliPack newPack = EiCreatePack(pawn);
                    if(!AvaliPackDriver.packs.Contains(newPack))
                        packs.Add(newPack);
                    Log.Message("Does this happen?");
                    Log.Message("Number of packs: " + packs.Count);
                }
                foreach (AvaliPack pack in packsToUse)
                {
                    if (pack.pawns.Contains(pawn))
                    {
                        if(!(pack.pawns.Count == 1))
                        {
                            break;
                        }
                        
                    }
                    if (pack.pawns.Count < packLimit)
                    {
                        AvaliPackDriver.packs.Remove(GetPack(pawn));
                        if (JoinPack(pawn, pack) != null)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (pack == packsToUse.Last<AvaliPack>())
                        { createPack(); }
                    }
                }

            }
            else
            {
                createPack();
            }
            return packs;
        }

        public static float GetPackAvgOP(AvaliPack pack, Pawn pawn)
        {
            float avgOP = 0;
           
            foreach(Pawn packmember in pack.pawns)
            {
                avgOP += packmember.relations.OpinionOf(pawn);

            }

            avgOP /= pack.pawns.Count;
            return avgOP;
        }

        public static AvaliPack JoinPack(Pawn pawn, AvaliPack pack)
        {
            Date date = new Date();
            AvaliPackDriver AvaliPackDriver = Current.Game.GetComponent<AvaliPackDriver>();


            if (!AvaliPackDriver.pawnsHaveHadPacks.ContainsKey(pawn) || AvaliPackDriver.pawnsHaveHadPacks[pawn] == false)
            {
                if (date.ToString() == pack.creationDate.ToString())
                {
                    pack.pawns.Add(pawn);
                    AvaliPackDriver.pawnsHaveHadPacks.Add(pawn, true);
                    return pack;
                }

            }
            else
            {
                Log.Message("Pawn was in Dict");
                if(GetPackAvgOP(pack, pawn) > LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packOpReq)
                {
                    pack.pawns.Add(pawn);
                    return pack;
                }
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
            if (!AvaliPackDriver.pawnsHaveHadPacks.ContainsKey(pawn))
            {
                AvaliPackDriver.pawnsHaveHadPacks.Add(pawn, true);
            }else if(AvaliPackDriver.pawnsHaveHadPacks[pawn] == false)
            {
                AvaliPackDriver.pawnsHaveHadPacks[pawn] = true;
            }
            return PawnPack;
        }

        public static AvaliPack GetPack(Pawn pawn)
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
                            return APack;
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

        public static AvaliPack GetPackWithoutSelf(Pawn pawn)
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