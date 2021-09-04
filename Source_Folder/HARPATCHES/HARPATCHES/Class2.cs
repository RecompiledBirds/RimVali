using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using AvaliMod;
using AlienRace;
using UnityEngine;

namespace AvaliMod.HARTweaks
{

    //There is probably a much better way to do this, but these patches are combinations of whatever we do and the stuff HAR does.
    [StaticConstructorOnStartup]
    public static class test
    {

        static test()

        {
            var harmony = new Harmony("Rimvali.HarPatches");
            Log.Message("Starting HAR patches. [RimVali Compatiblity]");
            try
            {
                //The only one actually related to rimvali.
                harmony.Patch(AccessTools.Method(typeof(RimValiDefChecks),"setup"), null, new HarmonyMethod(typeof(ReturnDataPatch), "ReturnDataRaces"));

                //The rest is all RVR stuff.
                harmony.Patch(AccessTools.Method(typeof(PawnRenderer), "BaseHeadOffsetAt"), null, new HarmonyMethod(typeof(HeadOffsetPatch), "setPos"));
                harmony.Patch(AccessTools.Method(typeof(Corpse), "ButcherProducts"), new HarmonyMethod(typeof(ButcherPatch), "Patch"));
                harmony.Patch(AccessTools.Method(typeof(BodyPatch), "Patch"), new HarmonyMethod(typeof(BodyGenPatch), "Patch"));
                Log.Message($"Patches completed: {harmony.GetPatchedMethods().Count()}");
                RestrictionsPatch.AddRestrictions();
            }
            catch (Exception error)
            {
                Log.Error("Patches failed! [RimVali Compatiblity]");
                Log.Error(error.Message);
                try
                {
                    RestrictionsPatch.AddRestrictions();
                }
                catch(Exception error2)
                {
                    Log.Error("Backup restrictions attempt failed. \n Error: "+error2.Message);
                }
            }
        }
    }
    public class BodyGenPatch
    {
        public static void Patch(ref Pawn pawn)
        {
            Pawn p2 = pawn;
            if (pawn.def is RimValiRaceDef rimValiRace)
            {
                try
                {
                    pawn.story.crownType = CrownType.Average;
                    if ((pawn.story.adulthood != null && DefDatabase<RVRBackstory>.AllDefs.Where(x => x.defName == p2.story.adulthood.identifier).Count() > 0))
                    {
                        RVRBackstory story = DefDatabase<RVRBackstory>.AllDefs.Where(x => x.defName == p2.story.adulthood.identifier).FirstOrDefault();
                        BodyPatch.SetBody(story, ref pawn);
                        return;
                    }
                    else if (DefDatabase<RVRBackstory>.AllDefs.Where(x => x.defName == p2.story.childhood.identifier).Count() > 0)
                    {
                        
                        RVRBackstory story = DefDatabase<RVRBackstory>.AllDefs.Where(x => x.defName == p2.story.childhood.identifier).FirstOrDefault();
                        BodyPatch.SetBody(story, ref pawn);
                        return;
                    }
                    else
                    {
                        //Log.Message(rimValiRace.bodyTypes.RandomElement().defName);
                        Log.Message(rimValiRace.bodyTypes.Count.ToString());
                        pawn.story.bodyType = rimValiRace.bodyTypes.RandomElement();

                    }

                }
                catch (Exception e)
                {
                    Log.Message(e.Message);
                    Log.Message("Trying again...");
                    Patch(ref pawn);
                }
            }
            else
            {
                if (pawn.def.GetType() != typeof(ThingDef_AlienRace))
                {
                    List<BodyTypeDef> getAllAvalibleBodyTypes = new List<BodyTypeDef>();
                    //getAllAvalibleBodyTypes.AddRange((IEnumerable<BodyTypeDef>)Restrictions.bodyTypeRestrictions.Where(x => x.Value.Contains(p2.def)));
                    if (Restrictions.bodyDefs.ContainsKey(p2.def))
                    {
                        getAllAvalibleBodyTypes.AddRange(Restrictions.bodyDefs[p2.def]);
                    }
                    if (getAllAvalibleBodyTypes.NullOrEmpty())
                    {
                        getAllAvalibleBodyTypes.AddRange(new List<BodyTypeDef> { BodyTypeDefOf.Fat, BodyTypeDefOf.Hulk, BodyTypeDefOf.Thin });
                    }
                    if (pawn.gender == Gender.Female)
                    {
                        getAllAvalibleBodyTypes.Add(BodyTypeDefOf.Female);
                    }
                    else
                    {
                        getAllAvalibleBodyTypes.Add(BodyTypeDefOf.Male);
                    }
                    pawn.story.bodyType = getAllAvalibleBodyTypes.RandomElement();
                }
                else
                {
                    AlienRace.HarmonyPatches.GenerateBodyTypePostfix(ref pawn);
                }
                Log.Message($"Pawn bodytype: {pawn.story.bodyType}, class: {pawn.def.thingClass}");
            }
        }
    }
    public class HeadOffsetPatch
    {
        [HarmonyAfter(new string[] { "RimVali.patches.headPatch" })]
        public static void setPos(ref Vector3 __result, Rot4 rotation, PawnRenderer __instance)
        {
            Pawn pawn = __instance.graphics.pawn;
            PawnGraphicSet set = __instance.graphics;
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                //This is an automatic check to see if we can put the head position here.
                //no human required
                if (!(rimValiRaceDef.renderableDefs.Where<RenderableDef>(x => x.defName.ToLower() == "head").Count() > 0))
                {
                    Vector2 offset = new Vector2(0, 0);
                    
                    RenderableDef headDef = rimValiRaceDef.renderableDefs.First(x => x.defName.ToLower() == "head");
                    Vector3 pos = new Vector3(0, 0, 0);
                    pos.y = __result.y;
                    if (headDef.west == null)
                    {
                        headDef.west = headDef.east;
                    }
                    if (pawn.Rotation == Rot4.South)
                    {
                        pos.x = headDef.south.position.x + offset.x;
                        pos.y = headDef.south.position.y + offset.y;
                    }
                    else if (pawn.Rotation == Rot4.North)
                    {
                        pos.x = headDef.north.position.x + offset.x;
                        pos.y = headDef.north.position.y + offset.y;
                    }
                    else if (pawn.Rotation == Rot4.East)
                    {
                        pos.x = headDef.east.position.x + offset.x;
                        pos.y = headDef.east.position.y + offset.y;
                    }
                    else
                    {
                        pos.x = headDef.west.position.x + offset.x;
                        pos.y = headDef.west.position.y + offset.y;
                    }
                    // Log.Message(pos.ToString());
                    __result = __result + pos;
                }

            }
            else
            {
                Vector2 offset = (pawn.def as ThingDef_AlienRace)?.alienRace.graphicPaths.GetCurrentGraphicPath(pawn.ageTracker.CurLifeStage).headOffsetDirectional?.GetOffset(rotation) ?? Vector2.zero;
                __result += new Vector3(offset.x, y: 0, offset.y);
            }
        }
    }
    public class RestrictionsPatch
    {
        public static void AddRestrictions()
        {
            Log.Message("[RVR]: HAR is loaded, merging race restrictions.");
            //I ended up using the same method of storing restrictions that HAR does to make this easier.
            foreach (AlienRace.ThingDef_AlienRace raceDef in DefDatabase<AlienRace.ThingDef_AlienRace>.AllDefs.Where<AlienRace.ThingDef_AlienRace>(x => x is AlienRace.ThingDef_AlienRace))
            {
                foreach (ThingDef thing in raceDef.alienRace.raceRestriction.buildingList)
                {
                    if (!(DefDatabase<ThingDef>.AllDefs.ToList().Contains(thing)))
                    {
                        Log.Error("Could not find thing!");
                        return;
                    }
                    if (!Restrictions.buildingRestrictions.ContainsKey(thing))
                    {
                        Restrictions.buildingRestrictions.Add(thing, new List<string>());
                        Restrictions.buildingRestrictions[thing].Add(raceDef.defName);
                    }
                    else
                    {
                        Restrictions.buildingRestrictions[thing].Add(raceDef.defName);
                    }
                }
                if (raceDef.alienRace.raceRestriction.foodList.Count > 0)
                {
                    foreach (ThingDef thing in raceDef.alienRace.raceRestriction.foodList)
                    {
                        if (!Restrictions.consumableRestrictions.ContainsKey(thing))
                        {
                            Restrictions.consumableRestrictions.Add(thing, new List<ThingDef>());
                            Restrictions.consumableRestrictions[thing].Add(raceDef);
                        }
                        else
                        {
                            Restrictions.consumableRestrictions[thing].Add(raceDef);
                        }
                    }
                }
                if (raceDef.alienRace.raceRestriction.apparelList.Count > 0)
                {
                    foreach (ThingDef thing in raceDef.alienRace.raceRestriction.apparelList)
                    {
                        if (!Restrictions.equipmentRestrictions.ContainsKey(thing))
                        {
                            Restrictions.equipmentRestrictions.Add(thing, new List<ThingDef>());
                            Restrictions.equipmentRestrictions[thing].Add(raceDef);
                        }
                        else
                        {
                            Restrictions.equipmentRestrictions[thing].Add(raceDef);
                        }
                    }
                }
                if (raceDef.alienRace.raceRestriction.traitList.Count > 0)
                {
                    foreach (TraitDef trait in raceDef.alienRace.raceRestriction.traitList)
                    {
                        if (!Restrictions.traitRestrictions.ContainsKey(trait))
                        {
                            Restrictions.traitRestrictions.Add(trait, new List<ThingDef>());
                            Restrictions.traitRestrictions[trait].Add(raceDef);
                        }
                        else
                        {
                            Restrictions.traitRestrictions[trait].Add(raceDef);
                        }
                    }
                }
                if (raceDef.alienRace.raceRestriction.researchList.Count > 0)
                {
                    foreach (AlienRace.ResearchProjectRestrictions research in raceDef.alienRace.raceRestriction.researchList)
                    {
                        foreach (ResearchProjectDef researchProject in research.projects)
                        {
                            if (!Restrictions.researchRestrictions.ContainsKey(researchProject))
                            {
                                Restrictions.researchRestrictions.Add(researchProject, new List<ThingDef>());
                                Restrictions.researchRestrictions[researchProject].Add(raceDef);
                            }
                            else
                            {
                                Restrictions.researchRestrictions[researchProject].Add(raceDef);
                            }
                        }
                    }
                }
            }
        }
    }
    public static class butcherPatch
    {
        static void ButcheredThoughAdder(Pawn pawn, Pawn butchered, bool butcher = true)
        {
            try
            {
                //Backstories
                if (!DefDatabase<RVRBackstory>.AllDefs.Where(x => x.hasButcherThoughtOverrides == true && (x.defName == pawn.story.adulthood.identifier || x.defName == pawn.story.childhood.identifier)).EnumerableNullOrEmpty())
                {

                    butcherAndHarvestThoughts butcherAndHarvestThoughts = DefDatabase<RVRBackstory>.AllDefs.Where(x => x.defName == pawn.story.adulthood.identifier || x.defName == pawn.story.childhood.identifier).First().butcherAndHarvestThoughtOverrides;
                    try
                    {
                        foreach (raceButcherThought rBT in butcherAndHarvestThoughts.butcherThoughts)
                        {
                            if (rBT.race == butchered.def)
                            {

                                if (butcher)
                                {
                                    pawn.needs.mood.thoughts.memories.TryGainMemory(rBT.butcheredPawnThought);
                                    return;
                                }
                                else
                                {
                                    pawn.needs.mood.thoughts.memories.TryGainMemory(rBT.knowButcheredPawn);
                                    return;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e.Message);
                    }

                    if (butcherAndHarvestThoughts.careAboutUndefinedRaces)
                    {
                        if (butcher)
                        {
                            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.ButcheredHumanlikeCorpse);
                            return;
                        }
                        else
                        {
                            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowButcheredHumanlikeCorpse);
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }


            //Races
            if (pawn.def is RimValiRaceDef def)
            {
                foreach (raceButcherThought rBT in def.butcherAndHarvestThoughts.butcherThoughts)
                {
                    if (rBT.race == butchered.def)
                    {

                        if (butcher)
                        {
                            pawn.needs.mood.thoughts.memories.TryGainMemory(rBT.butcheredPawnThought);
                            return;
                        }
                        pawn.needs.mood.thoughts.memories.TryGainMemory(rBT.knowButcheredPawn);
                        return;
                    }
                }
                if (def.butcherAndHarvestThoughts.careAboutUndefinedRaces)
                {
                    if (butcher)
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.ButcheredHumanlikeCorpse);
                        return;
                    }

                    pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowButcheredHumanlikeCorpse);
                    return;
                }
            }

            //If the pawn is not from RVR.
            if (butcher)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.ButcheredHumanlikeCorpse);
                return;
            }
            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowButcheredHumanlikeCorpse);
        }


        [HarmonyBefore(new string[]{"rimworld.erdelf.alien_race.main"})]
        public static bool Patch(Pawn butcher, float efficiency, ref IEnumerable<Thing> __result, Corpse __instance)
        {
            if(__instance.InnerPawn.def is RimValiRaceDef)
            {
                ButcheredThoughAdder(butcher, __instance.InnerPawn);
            }

            AlienRace.HarmonyPatches.ButcherProductsPrefix(butcher, efficiency, ref __result, __instance);
            foreach (Pawn targetPawn in butcher.Map.mapPawns.SpawnedPawnsInFaction(butcher.Faction))
            {
                if (targetPawn.def is RimValiRaceDef rVRDef)
                {
                    ButcheredThoughAdder(butcher, __instance.InnerPawn, false);
                }
            }
            Log.Message("hi");
            return false;
        }
    }
    public class ReturnDataPatch
    {
        public static List<AlienRace.ThingDef_AlienRace> potentialPackRaces = DefDatabase<AlienRace.ThingDef_AlienRace>.AllDefsListForReading;
        public static void ReturnDataRaces()
        {
            RimValiDefChecks.potentialPackRaces= DefDatabase<ThingDef>.AllDefs.Where(x => x.race != null).ToList();
            RimValiDefChecks.potentialPackRaces.AddRange(potentialPackRaces);

        }
    }
}