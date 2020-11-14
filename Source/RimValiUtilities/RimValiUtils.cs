using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.IO;
using System.Threading;
using Verse;
namespace AvaliMod
{
    public static class RimValiUtility
    {
        public static string build = "Ei 0.0.2";
        public static string dir;
       /* public static void AssetBundleFinder(DirectoryInfo info)
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
        }*/

        public static AssetBundle shaderLoader(string info)
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(info);
            Log.Message("-----------------------------------------");
            Log.Message("Loaded bundle: " + assetBundle.name);
            Log.Message(assetBundle.GetAllAssetNames()[0], false);
            return assetBundle;
        }
        public static Dictionary<SkillRecord, float> Skills(Pawn pawn)
        {
            Dictionary<SkillRecord, float> skillNums = new Dictionary<SkillRecord, float>();
            IEnumerable<SkillRecord> skills = pawn.skills.skills;
            foreach (SkillRecord skill in skills)
            {
                skillNums.Add(skill, skill.XpTotalEarned);
            }
            return skillNums;
        }

        public static Dictionary<Pawn, PawnRelationDef> PawnRelations(Pawn pawn)
        {
            Dictionary<Pawn, PawnRelationDef> relatedPawnsToReturn = new Dictionary<Pawn, PawnRelationDef>();

            IEnumerable<Pawn> pawns = pawn.relations.RelatedPawns;
            foreach (Pawn relatedPawn in pawns)
            {
                int onRelation = 0;
                while (onRelation > relatedPawn.relations.RelatedPawns.Count())
                {
                    foreach (PawnRelationDef relationDef in RimValiRelationsFound.relationsFound)
                    {
                        if (relatedPawn.relations.DirectRelationExists(relationDef, pawn))
                        {
                            relatedPawnsToReturn.Add(relatedPawn, relationDef);
                        }
                    }
                    onRelation += 1;
                }
            }
            return null;
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




        public static bool CheckIfPackmatesInRoom(Pawn pawn, PawnRelationDef relationDef)
        {
            Room room = pawn.GetRoom();
            if (!(room == null) && (pawn.Position.Roofed(pawn.Map)))
            {
                IEnumerable<Pawn> pawns = RimValiUtility.GetPackPawns(pawn, relationDef);
                foreach (Pawn packmate in pawns)
                {
                    if (packmate.GetRoom(RegionType.Set_Passable) == room)
                    {
                        return true;
                    }
                }
            }
            return false;
        }



        public static int GetPackSize(Pawn pawn, PawnRelationDef relationDef)
        {
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
        }

        public static IEnumerable<Pawn> AllPawnsOfRaceOnMap(ThingDef race, Map map)
        {
            IEnumerable<Pawn> pawns = map.mapPawns.AllPawns;
            List<Pawn> pawnsToReturn = new List<Pawn>();
            foreach (Pawn pawn in pawns)
            {
                if (pawn.def == race)
                {
                    pawnsToReturn.Add(pawn);
                }
            }
            return pawnsToReturn;
        }

        //Added in the "Keo" build. 
        public static IEnumerable<Pawn> AllPawnsOfRaceInMapAndFaction(Pawn pawn, Faction faction)
        {
            List<Pawn> pawnsInMap = new List<Pawn>();
            IEnumerable<Pawn> pawns = CheckAllPawnsInMapAndFaction(pawn.Map, pawn.Faction);
            foreach(Pawn pawn1 in pawns)
            {
                if(pawn1.def == pawn.def)
                {
                    pawnsInMap.Add(pawn1);
                }
            }
            return pawnsInMap;
        }

        public static IEnumerable<Pawn> CheckAllPawnsInMapAndFaction(Map map, Faction faction)
        {
            IEnumerable<Pawn> pawns = PawnsFinder.AllMaps_SpawnedPawnsInFaction(faction);
            List<Pawn> pawnsInMap = new List<Pawn>();
            foreach (Pawn pawn in pawns)
            {
                if (pawn.Map == map)
                {
                    pawnsInMap.Add(pawn);
                }
            }
            IEnumerable<Pawn> pawnsFound = pawnsInMap;
            return pawnsFound;
        }



        public static bool CheckIfBedRoomHasPackmates(Pawn pawn, PawnRelationDef relationDef)
        {
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
        }


        public static void AddThought(Pawn pawn, ThoughtDef thought)
        {
            pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
        }

        public static void RemoveThought(Pawn pawn, ThoughtDef thought)
        {
            pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(thought);
        }

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
        public static bool IsOfRace(Pawn pawn, ThingDef race)
        {
            if (pawn.def.defName == race.defName)
            {
                return true;
            }
            else
            {
                return false;
            }
        }






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

        public static int PawnOfRaceCount(Faction faction, ThingDef race)
        {

            IEnumerable<Pawn> pawns = PawnsFinder.AllMaps_SpawnedPawnsInFaction(faction);
            int foundMatches = 0;
            foreach (Pawn pawn in pawns)
            {
                if (IsOfRace(pawn, race))
                {
                    foundMatches += 1;
                }
            }
            return foundMatches;
        }

        public static IEnumerable<Pawn> PawnsOfRaceInFaction(ThingDef race, Faction faction)
        {
            IEnumerable<Pawn> pawns = PawnsFinder.AllMaps_SpawnedPawnsInFaction(faction);
            List<Pawn> pawnsToReturn = null;
            foreach (Pawn pawn in pawns)
            {
                if (IsOfRace(pawn, race))
                {
                    pawnsToReturn.Add(pawn);
                }
            }
            return pawnsToReturn;
        }

        public static bool FactionHasRace(ThingDef race, Faction faction)
        {
            if (PawnOfRaceCount(faction, race) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        public static List<AvaliPack> EiPackHandler(List<AvaliPack> packs, Pawn pawn, IEnumerable<ThingDef> racesInPacks, int packLimit)
        {
            void createNewPack()
            {
                AvaliPack NewPack = EiCreatePack(pawn);
                Log.Message("Creating new pack- " + NewPack.name);
                packs.Add(NewPack);
            }
            AvaliPack PawnPack = null;
            //Log.Message("Checking all packs.");
            if (packs.Count > 0)
            {
                foreach (AvaliPack pack in packs)
                {
                    if (pack.pawns.Contains(pawn))
                    {
                        PawnPack = pack;
                        //Log.Message("Found pack for pawn: " + pawn.Name);
                        //Log.Message(pack.name);
                        break;
                    }
                    if (pack.size < packLimit && pawn.Faction == pack.faction)
                    {
                        PawnPack = JoinPack(pawn, pack);
                        packs.Replace<AvaliPack>(pack, PawnPack);
                        break;
                    }
                }

                if (PawnPack == null)
                {
                    createNewPack();

                }
            }
            else
            {
                Log.Message("No packs found, creating new pack.");
                createNewPack();
            }
            return packs;
        }

      
        public static AvaliPack JoinPack(Pawn pawn, AvaliPack pack, int reqOpinionPawn = 30, int reqOpionionPack = 30)
        {
            pack.pawns.Add(pawn);
            pack.size++;
            return pack;
        }
        public static AvaliPack EiCreatePack(Pawn pawn)
        {
            
            AvaliPack PawnPack = new AvaliPack();
            PawnPack.name = pawn.Name.ToStringShort + "'s pack";
            PawnPack.size = 1;
            PawnPack.faction = pawn.Faction;
            //Log.Message(PawnPack.name);
            //Log.Message(pawn.Name.ToString());
            PawnPack.pawns.Add(pawn);
            return PawnPack;
        }

        public static AvaliPack GetPack(Pawn pawn)
        {
            //Verify the pawn somehow isnt null. Really shouldnt be an issue, but another "saftey check"
            if(pawn == null)
            {
                Log.Error("Pawn check is null!");
                return null;
            }
            //This really shouldnt be needed, but it's a failsafe if something messes up.
            if(AvaliPackDriver.packs == null || AvaliPackDriver.packs.Count == 0)
            {
                return null;
            }
            //We really should be getting here
            if(AvaliPackDriver.packs.Count > 0)
            {
                foreach(AvaliPack APack in AvaliPackDriver.packs)
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
            Log.Message("Didn't find pack, returning null.");
            return null;
        }
    }
}