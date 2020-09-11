using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
namespace AvaliMod
{
    public static class RimValiUtility
    {
        public static Dictionary<SkillRecord, float> Skills(Pawn pawn)
        {
            Dictionary<SkillRecord, float> skillNums = new Dictionary<SkillRecord, float>();
            List<SkillRecord> skills = pawn.skills.skills;
            foreach (SkillRecord skill in skills)
            {
                skillNums.Add(skill, skill.XpTotalEarned);
            }
            return skillNums;
        }

        public static Dictionary<Pawn, PawnRelationDef> PawnRelations(Pawn pawn)
        {
            Dictionary<Pawn, PawnRelationDef> relatedPawnsToReturn = new Dictionary<Pawn, PawnRelationDef>();
            List<Pawn> pawns = pawn.relations.RelatedPawns.ToList<Pawn>();
            foreach(Pawn relatedPawn in pawns)
            {
                int onRelation = 0;
                while(onRelation > relatedPawn.relations.RelatedPawns.Count())
                {
                    foreach(PawnRelationDef relationDef in RimValiRelationsFound.relationsFound)
                    {
                        if(relatedPawn.relations.DirectRelationExists(relationDef, pawn))
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
            List<Trait> traits = pawn.story.traits.allTraits;
            Dictionary<Trait, int> traitDataToReturn = new Dictionary<Trait, int>();
            foreach(Trait trait in traits)
            {
                traitDataToReturn.Add(trait, trait.Degree);
            }
            return traitDataToReturn;
        }
        
                


        public static bool CheckIfPackmatesInRoom(Pawn pawn, PawnRelationDef relationDef)
        {
            Room room = pawn.GetRoom();
            if(!(room == null) && (pawn.Position.Roofed(pawn.Map)))
            {
                List<Pawn> pawns = RimValiUtility.GetPackPawns(pawn, relationDef);
                foreach(Pawn packmate in pawns)
                {
                    if(packmate.GetRoom(RegionType.Set_Passable) == room)
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
            List<Pawn> relatedPawns = pawn.relations.RelatedPawns.ToList<Pawn>();
            foreach (Pawn packmate in relatedPawns)
            {
                if (packmate.relations.DirectRelationExists(relationDef, pawn) || pawn.relations.DirectRelationExists(relationDef, packmate))
                {
                    foundMembers += 1;
                }
            }
            return foundMembers;
        }

        public static IEnumerable<Pawn> CheckAllPawnsInMapAndFaction(Map map, Faction faction)
        {
            List<Pawn> pawns = PawnsFinder.AllMaps_SpawnedPawnsInFaction(faction);
            List<Pawn> pawnsInMap = new List<Pawn>();
            foreach (Pawn pawn in pawns)
            {
                if(pawn.Map == map)
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
                    List<Building_Bed> beds = room.ContainedBeds.ToList<Building_Bed>();
                    foreach (Building_Bed bed in beds)
                    {
                        if (bed.OwnersForReading != null)
                        {
                            List<Pawn> owners = bed.OwnersForReading;
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
                if(packmatesFound > 0)
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

        public static List<Pawn> GetPackPawns(Pawn pawn, PawnRelationDef relationDef)
        {
            List<Pawn> packmates = new List<Pawn>();
            List<Pawn> pawns = pawn.relations.RelatedPawns.ToList<Pawn>();
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






        public static void RemovePackRelationIfDead(Pawn pawn, Pawn packmate, Pawn packmate2, PawnRelationDef relationDef)
        {
            if (packmate.Dead || packmate2.Dead || packmate.DestroyedOrNull() || packmate2.DestroyedOrNull() || pawn.Dead || pawn.DestroyedOrNull())
            {
                packmate.relations.RemoveDirectRelation(relationDef, packmate2);
                packmate2.relations.RemoveDirectRelation(relationDef, packmate);

                packmate.relations.RemoveDirectRelation(relationDef, pawn);
                packmate2.relations.RemoveDirectRelation(relationDef, pawn);
                pawn.relations.RemoveDirectRelation(relationDef, packmate);
                pawn.relations.RemoveDirectRelation(relationDef, packmate2);
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
            List<Pawn> pawns = PawnsFinder.AllMaps_SpawnedPawnsInFaction(faction);
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

        public static List<Pawn> PawnsOfRaceInFaction(ThingDef race, Faction faction)
        {
            List<Pawn> pawns = PawnsFinder.AllMaps_SpawnedPawnsInFaction(faction);
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

        public static void CollectPackmates(Pawn pawn, Pawn pawn2, PawnRelationDef relationDef)
        {
            //Grab a list of all of their relations.
            IEnumerable<Pawn> firstRelatedPawns = pawn.relations.RelatedPawns;
            IEnumerable<Pawn> secondRelatedPawns = pawn2.relations.RelatedPawns;
            foreach (Pawn pawnFound in firstRelatedPawns)
            {
                //if a packmate relation exists between pawn and firstRelatedPawns[relatedItem]
                if (pawn.relations.DirectRelationExists(relationDef, pawnFound))
                {
                    //and the other pawn does not have it
                    if (!pawn2.relations.DirectRelationExists(relationDef, pawnFound))
                    {
                        //add the relation to both
                        pawn2.relations.AddDirectRelation(relationDef, pawnFound);
                        pawnFound.relations.AddDirectRelation(relationDef, pawn2);
                    }
                }
            }
            //same thing, except pawn2 is in place of pawn, and secondRelatedPawns replaces firstRelated
            foreach (Pawn foundPawn in secondRelatedPawns)
            {
                if (pawn2.relations.DirectRelationExists(relationDef, foundPawn))
                {
                    if (!pawn.relations.DirectRelationExists(relationDef, foundPawn))
                    {
                        pawn.relations.AddDirectRelation(relationDef, foundPawn);
                        foundPawn.relations.AddDirectRelation(relationDef, pawn);
                    }
                }
            }
        }

        public static void TrimPack(Pawn pawn, Pawn pawn2, PawnRelationDef relationDef, Faction faction = null, int limit = 5)
        {
            //Grab a list of all of their relations.
            if (faction == null)
            {
                faction = pawn.Faction;
            }
            IEnumerable<Pawn> firstRelatedPawns = pawn.relations.RelatedPawns;
            IEnumerable<Pawn> secondRelatedPawns = pawn2.relations.RelatedPawns;
            foreach (Pawn pawnFound in firstRelatedPawns)
            {
                //if a packmate relation exists between pawn and firstRelatedPawns[relatedItem]
                if (pawn.relations.DirectRelationExists(relationDef, pawnFound) && GetPackSize(pawn, relationDef) > limit)
                {
                    //and the other pawn does not have it
                    if (pawn2.relations.DirectRelationExists(relationDef, pawnFound))
                    {
                        //remove the relation from both
                        pawn2.relations.TryRemoveDirectRelation(relationDef, pawnFound);
                        pawnFound.relations.TryRemoveDirectRelation(relationDef, pawn2);
                    }
                }
            }
            //same thing, except pawn2 is in place of pawn, and secondRelatedPawns replaces firstRelated
            foreach (Pawn foundPawn in secondRelatedPawns)
            {
                if (pawn2.relations.DirectRelationExists(relationDef, foundPawn) && GetPackSize(pawn2, relationDef) > limit)
                {
                    if (pawn.relations.DirectRelationExists(relationDef, foundPawn))
                    {
                        pawn.relations.TryRemoveDirectRelation(relationDef, foundPawn);
                        foundPawn.relations.TryRemoveDirectRelation(relationDef, pawn);
                    }
                }
            }
        }



        public static void MakePack(Pawn pawn, PawnRelationDef relationDef, List<ThingDef> racesInPacks, int packLimit)
        {
            IEnumerable<Pawn> packMates = PawnsFinder.AllMaps_SpawnedPawnsInFaction(pawn.Faction);
            if (pawn.Spawned)
            {
                foreach (Pawn packmate in packMates)
                {
                    if (!pawn.relations.DirectRelationExists(relationDef, packmate))
                    {
                        foreach (Pawn packmate2 in packMates)
                        {
                            if (!(packmate == packmate2))
                            {
                                foreach (ThingDef raceDef in racesInPacks)
                                {
                                    if (packmate2.def.defName == raceDef.defName)
                                    {
                                        foreach (ThingDef raceDef2 in racesInPacks)
                                        {
                                            if (packmate.def.defName == raceDef2.defName)
                                            {
                                                if (!(packmate2.relations.DirectRelationExists(relationDef, packmate)))
                                                {
                                                    if (!(GetPackSize(packmate, relationDef) >= packLimit))
                                                    {
                                                        packmate2.relations.AddDirectRelation(relationDef, packmate);
                                                        packmate.relations.AddDirectRelation(relationDef, packmate2);
                                                        CollectPackmates(packmate, packmate2, relationDef);
                                                    }
                                                    TrimPack(packmate, packmate2, relationDef, packmate.Faction, packLimit);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}