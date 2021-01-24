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
                harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GenerateBodyType_NewTemp"), null, new HarmonyMethod(typeof(BodyGenPatch), "genBody"));
                harmony.Patch(AccessTools.Method(typeof(Corpse), "ButcherProducts"), new HarmonyMethod(typeof(butcherPatch), "patch"));
                Log.Message("Patches completed!");
                RestrictionsPatch.AddRestrictions();
            }
            catch (Exception error)
            {
                Log.Message("Patches failed! [RimVali Compatiblity]");
                Log.Message(error.Message);
                try
                {
                    RestrictionsPatch.AddRestrictions();
                }
                catch(Exception error2)
                {
                    Log.Message("Backup restrictions attempt failed. \n Error: "+error2.Message);
                }
            }
        }
    }
    //This is essentially a combination of the HAR and RVR patches. That's all it is.
    public class BodyGenPatch
    {
        public static void genBody(ref Pawn pawn)
        {
            if (pawn.def is RimValiRaceDef rimValiRace)
            {
                try
                {
                    pawn.story.bodyType =rimValiRace.mainSettings.bodyTypeDefs[/*randChoice.Next(rimValiRace.mainSettings.bodyTypeDefs.Count-1*/ UnityEngine.Random.Range(0, rimValiRace.mainSettings.bodyTypeDefs.Count - 1)];
                }
                catch (Exception e)
                {
                    Log.Message(e.Message);
                    genBody(ref pawn);
                }
            }
            else
            {
                //HAR methods
                if(BackstoryDef.checkBodyType.Contains(pawn.story.GetBackstory(BackstorySlot.Adulthood)))
                pawn.story.bodyType = DefDatabase<BodyTypeDef>.GetRandom();

                if (pawn.def is ThingDef_AlienRace alienProps &&
                    !alienProps.alienRace.generalSettings.alienPartGenerator.alienbodytypes.NullOrEmpty() &&
                        !alienProps.alienRace.generalSettings.alienPartGenerator.alienbodytypes.Contains(pawn.story.bodyType))
                    pawn.story.bodyType = alienProps.alienRace.generalSettings.alienPartGenerator.alienbodytypes.RandomElement();
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
                if (!(rimValiRaceDef.bodyPartGraphics.Where<RenderableDef>(x => x.defName.ToLower() == "head").Count() > 0))
                {
                    Vector2 offset = new Vector2(0, 0);
                    if (rimValiRaceDef.graphics.headOffsets != null)
                    {
                        if (rimValiRaceDef.graphics.headOffsets != null)
                        {
                            if (rotation == Rot4.South)
                            {
                                offset = rimValiRaceDef.graphics.headOffsets.south;
                            }
                            else if (rotation == Rot4.East)
                            {
                                offset = rimValiRaceDef.graphics.headOffsets.east;
                            }
                            else if (rotation == Rot4.North)
                            {
                                offset = rimValiRaceDef.graphics.headOffsets.north;
                            }
                            else if (rotation == Rot4.West)
                            {
                                offset = rimValiRaceDef.graphics.headOffsets.west;
                            }
                        }
                    }
                    RenderableDef headDef = rimValiRaceDef.bodyPartGraphics.First(x => x.defName.ToLower() == "head");
                    Vector3 pos = new Vector3(0, 0, 0);
                    pos.y = __result.y;
                    if (rimValiRaceDef.graphics.headSize == null)
                    {
                        rimValiRaceDef.graphics.headSize = headDef.south.size;
                    }
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
        static void butcheredThoughAdder(Pawn pawn, Pawn butchered, bool butcher = true)
        {
            if (pawn.def is RimValiRaceDef def)
            {
                foreach (raceButcherThought rBT in def.butcherAndHarvestThoughts.butcherThoughts)
                {
                    if (rBT.race == butchered.def)
                    {

                        if (butcher)
                        {
                            pawn.needs.mood.thoughts.memories.TryGainMemory(rBT.butcheredPawnThought);
                        }
                        else
                        {
                            pawn.needs.mood.thoughts.memories.TryGainMemory(rBT.knowButcheredPawn);
                        }
                        return;
                    }
                }
                if (def.butcherAndHarvestThoughts.careAboutUndefinedRaces)
                {
                    if (butcher)
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.ButcheredHumanlikeCorpse);
                    }
                    else
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowButcheredHumanlikeCorpse);
                    }
                }
                return;
            }
        }
        [HarmonyAfter(new string[] { "RimVali.patches.butcherPatch" })]
        public static bool patch(Pawn butcher, float efficiency, ref IEnumerable<Thing> __result, Corpse __instance)
        {
            AlienRace.HarmonyPatches.ButcherProductsPrefix(butcher, efficiency, ref __result, __instance);
            butcheredThoughAdder(butcher, __instance.InnerPawn);
            foreach (Pawn targetPawn in butcher.Map.mapPawns.SpawnedPawnsInFaction(butcher.Faction))
            {
                if (targetPawn.def is RimValiRaceDef rVRDef)
                {
                    butcheredThoughAdder(butcher, __instance.InnerPawn, false);
                }
            }
            Debug.Log("hi");
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