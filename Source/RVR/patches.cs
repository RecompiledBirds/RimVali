
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using AlienRace;
using System;
using Verse.AI;
namespace AvaliMod
{
    [StaticConstructorOnStartup]
    public static class Restrictions
    {
        //Normal restrictions
        public static Dictionary<ThingDef, List<ThingDef>> equipmentRestrictions = new Dictionary<ThingDef, List<ThingDef>>();
        public static Dictionary<ThingDef, List<ThingDef>> consumableRestrictions = new Dictionary<ThingDef, List<ThingDef>>();
        public static Dictionary<BuildableDef, List<string>> buildingRestrictions = new Dictionary<BuildableDef, List<string>>();

        public static Dictionary<ResearchProjectDef, List<ThingDef>> researchRestrictions = new Dictionary<ResearchProjectDef, List<ThingDef>>();

        public static Dictionary<TraitDef, List<ThingDef>> traitRestrictions = new Dictionary<TraitDef, List<ThingDef>>();

        public static Dictionary<BodyTypeDef, List<ThingDef>> bodyTypeRestrictions = new Dictionary<BodyTypeDef, List<ThingDef>>();

        public static Dictionary<ThingDef, List<ThingDef>> bedRestrictions = new Dictionary<ThingDef, List<ThingDef>>();

        public static Dictionary<ThoughtDef, List<ThingDef>> thoughtRestrictions = new Dictionary<ThoughtDef, List<ThingDef>>();
        //Whitelists
        public static Dictionary<ThingDef, List<ThingDef>> buildingWhitelists = new Dictionary<ThingDef, List<ThingDef>>();
        public static Dictionary<ThingDef, List<ThingDef>> equipabblbleWhiteLists = new Dictionary<ThingDef, List<ThingDef>>();



        static Restrictions()
        {
            Log.Message("[RVR]: Setting up race restrictions.");
            foreach (RimValiRaceDef raceDef in RimValiDefChecks.races)
            {
                if (raceDef.restrictions.buildables.Count > 0)
                {
                    foreach (ThingDef thing in raceDef.restrictions.buildables)
                    {
                        if (!(DefDatabase<ThingDef>.AllDefs.ToList().Contains(thing)))
                        {
                            Log.Error("Could not find thing!");
                            return;
                        }
                        if (!buildingRestrictions.ContainsKey(thing))
                        {
                            Restrictions.buildingRestrictions.Add(thing, new List<string>());
                            //I originally thought this would be somethine like "Restrictions.buildingRestrictions.SetOrAdd(thing, list<string>.add());".
                            //Turns out its alot more like python then i thought. (python syntax: list[thing].append(racedef.defName) would do the same thing as this.)
                            Restrictions.buildingRestrictions[thing].Add(raceDef.defName);
                        }
                        else
                        {
                            buildingRestrictions[thing].Add(raceDef.defName);
                        }
                    }
                }
                if (raceDef.restrictions.consumables.Count > 0)
                {
                    foreach (ThingDef thing in raceDef.restrictions.consumables)
                    {
                        if (!consumableRestrictions.ContainsKey(thing))
                        {
                            consumableRestrictions.Add(thing, new List<ThingDef>());
                            consumableRestrictions[thing].Add(raceDef);
                        }
                        else
                        {
                            consumableRestrictions[thing].Add(raceDef);
                        }
                    }
                }
                if (raceDef.restrictions.equippables.Count > 0)
                {
                    foreach (ThingDef thing in raceDef.restrictions.equippables)
                    {
                        if (!equipmentRestrictions.ContainsKey(thing))
                        {
                            equipmentRestrictions.Add(thing, new List<ThingDef>());
                            equipmentRestrictions[thing].Add(raceDef);
                        }
                        else
                        {
                            equipmentRestrictions[thing].Add(raceDef);
                        }
                    }
                }
                if (raceDef.restrictions.researchProjectDefs.Count > 0)
                {
                    foreach (ResearchProjectDef research in raceDef.restrictions.researchProjectDefs)
                    {
                        if (!researchRestrictions.ContainsKey(research))
                        {
                            researchRestrictions.Add(research, new List<ThingDef>());
                            researchRestrictions[research].Add(raceDef);
                        }
                        else
                        {
                            researchRestrictions[research].Add(raceDef);
                        }
                    }
                }
                if (raceDef.restrictions.traits.Count > 0)
                {
                    foreach (TraitDef trait in raceDef.restrictions.traits)
                    {
                        if (!traitRestrictions.ContainsKey(trait))
                        {
                            traitRestrictions.Add(trait, new List<ThingDef>());
                            traitRestrictions[trait].Add(raceDef);
                        }
                        else
                        {
                            traitRestrictions[trait].Add(raceDef);
                        }
                    }
                }
                if (raceDef.restrictions.thoughtDefs.Count > 0)
                {
                    foreach (ThoughtDef thought in raceDef.restrictions.thoughtDefs)
                    {
                        if (!thoughtRestrictions.ContainsKey(thought))
                        {
                            thoughtRestrictions.Add(thought, new List<ThingDef>());
                            thoughtRestrictions[thought].Add(raceDef);
                        }
                        else
                        {
                            thoughtRestrictions[thought].Add(raceDef);
                        }
                    }
                }
                if (raceDef.restrictions.equippablesWhitelist.Count > 0)
                {
                    foreach (ThingDef thing in raceDef.restrictions.equippablesWhitelist)
                    {
                        if (!equipabblbleWhiteLists.ContainsKey(thing))
                        {
                            equipabblbleWhiteLists.Add(thing, new List<ThingDef>());
                            equipabblbleWhiteLists[thing].Add(raceDef);
                        }
                        else
                        {
                            equipabblbleWhiteLists[thing].Add(raceDef);
                        }
                    }
                }
                if (raceDef.restrictions.bedDefs.Count > 0)
                {
                    foreach (ThingDef thing in raceDef.restrictions.bedDefs)
                    {
                        if (!bedRestrictions.ContainsKey(thing))
                        {
                            bedRestrictions.Add(thing, new List<ThingDef>());
                            bedRestrictions[thing].Add(raceDef);
                        }
                        else
                        {
                            bedRestrictions[thing].Add(raceDef);
                        }
                    }
                }
                if (raceDef.restrictions.researchProjectDefs.Count > 0)
                {
                    foreach (ResearchProjectDef research in raceDef.restrictions.researchProjectDefs)
                    {
                        if (!researchRestrictions.ContainsKey(research))
                        {
                            researchRestrictions.Add(research, new List<ThingDef>());
                            researchRestrictions[research].Add(raceDef);
                        }
                        else
                        {
                            researchRestrictions[research].Add(raceDef);
                        }
                    }
                }
                if (raceDef.restrictions.bodyTypes.Count > 0)
                {
                    foreach (BodyTypeDef bodyTypeDef in raceDef.restrictions.bodyTypes)
                    {
                        if (!bodyTypeRestrictions.ContainsKey(bodyTypeDef))
                        {
                            bodyTypeRestrictions.Add(bodyTypeDef, new List<ThingDef>());
                            bodyTypeRestrictions[bodyTypeDef].Add(raceDef);
                        }
                        else
                        {
                            Restrictions.bodyTypeRestrictions[bodyTypeDef].Add(raceDef);
                        }
                    }
                }

                if (raceDef.restrictions.modContentRestrictionsApparelWhiteList.Count > 0)
                {

                    foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading.Where(x => raceDef.restrictions.modContentRestrictionsApparelWhiteList.Contains(x.Name) || raceDef.restrictions.modContentRestrictionsApparelWhiteList.Contains(x.PackageId)))
                    {
                        Log.Message(mod.Name);
                        foreach (ThingDef def in mod.AllDefs.Where(x => x is ThingDef thingDef && (thingDef.IsApparel || thingDef.IsWeapon || thingDef.IsMeleeWeapon || thingDef.IsRangedWeapon)))
                        {
                            //Log.Message(def.defName);
                            if (!equipabblbleWhiteLists.ContainsKey(def))
                            {
                                equipabblbleWhiteLists.Add(def, new List<ThingDef>());
                                equipabblbleWhiteLists[def].Add(raceDef);
                                //Log.Message("Adding " + def.defName + " to whitelist: " + raceDef.defName);
                            }
                            else
                            {
                                equipabblbleWhiteLists[def].Add(raceDef);
                            }
                        }
                    }
                }
                if (raceDef.useHumanRecipes)
                {

                    foreach (RecipeDef recipe in DefDatabase<RecipeDef>.AllDefsListForReading.Where<RecipeDef>(x => x.recipeUsers != null && x.recipeUsers.Contains(ThingDefOf.Human)))
                    {
                        recipe.recipeUsers.Add(raceDef);
                        recipe.recipeUsers.RemoveDuplicates();

                    }
                    if (raceDef.recipes == null)
                    {
                        raceDef.recipes = new List<RecipeDef>();
                    }
                    foreach (RecipeDef recipe in ThingDefOf.Human.recipes)
                    {
                        if (!recipe.targetsBodyPart || recipe.appliedOnFixedBodyParts.NullOrEmpty())
                        {
                            raceDef.recipes.Add(recipe);
                        }
                        foreach (BodyPartDef bodyPart in recipe.appliedOnFixedBodyParts)
                        {
                            foreach (BodyPartRecord bodyPartRecord in raceDef.race.body.AllParts)
                            {
                                BodyPartDef def = bodyPartRecord.def;
                                if (def == bodyPart)
                                {
                                    raceDef.recipes.Add(recipe);
                                }
                            }
                        }

                    }
                    raceDef.recipes.RemoveDuplicates();
                }
            }
        }
    }


    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", new Type[] { typeof(PawnGenerationRequest) })]
    public static class generatorPatch
    {
        [HarmonyPrefix]
        public static void GeneratePawn(ref PawnGenerationRequest request)
        {
            if (request.KindDef != null)
            {

                PawnKindDef pawnKindDef = request.KindDef;
                IEnumerable<RimValiRaceDef> races = DefDatabase<RimValiRaceDef>.AllDefsListForReading;

                foreach (RimValiRaceDef race in races)
                {
                    RVRRaceInsertion inserter = race.raceInsertion;
                    if (Rand.Range(0, 100) < inserter.globalChance)
                    {
                        if (pawnKindDef == PawnKindDefOf.Slave)
                        {
                            foreach (Entry entry in inserter.entries)
                            {
                                if (entry.isSlave && Rand.Range(0, 100) < entry.chance)
                                {
                                    PawnKindDef newDef = DefDatabase<PawnKindDef>.GetNamedSilentFail(entry.pawnkind.defName);
                                    if (newDef != null)
                                    {
                                        pawnKindDef = newDef;
                                        request.KindDef = pawnKindDef;
                                    }
                                    break;
                                }
                            }
                        }
                        else if (pawnKindDef == PawnKindDefOf.Villager)
                        {
                            foreach (Entry entry in inserter.entries)
                            {
                                if (entry.isVillager && Rand.Range(0, 100) < entry.chance)
                                {
                                    PawnKindDef newDef = DefDatabase<PawnKindDef>.GetNamedSilentFail(entry.pawnkind.defName);
                                    if (newDef != null)
                                    {
                                        pawnKindDef = newDef;
                                        request.KindDef = pawnKindDef;
                                    }
                                    break;
                                }
                            }
                        }
                        else if (pawnKindDef == PawnKindDefOf.SpaceRefugee)
                        {
                            foreach (Entry entry in inserter.entries)
                            {
                                if (entry.isRefugee && Rand.Range(0, 100) < entry.chance)
                                {
                                    PawnKindDef newDef = DefDatabase<PawnKindDef>.GetNamedSilentFail(entry.pawnkind.defName);
                                    if (newDef != null)
                                    {
                                        pawnKindDef = newDef;
                                        request.KindDef = pawnKindDef;
                                    }
                                    break;
                                }
                            }
                        }
                        else if (pawnKindDef == PawnKindDefOf.Drifter)
                        {
                            foreach (Entry entry in inserter.entries)
                            {
                                if (entry.isWanderer && Rand.Range(0, 100) < entry.chance)
                                {
                                    PawnKindDef newDef = DefDatabase<PawnKindDef>.GetNamedSilentFail(entry.pawnkind.defName);
                                    if (newDef != null)
                                    {
                                        pawnKindDef = newDef;
                                        request.KindDef = pawnKindDef;
                                    }
                                    break;
                                }
                            }
                        }


                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(RestUtility), "CanUseBedEver")]
    public class bedPatch
    {
        [HarmonyPostfix]
        public static void bedPostfix(ref bool __result, Pawn p, ThingDef bedDef)
        {
            if (Restrictions.bedRestrictions.ContainsKey(bedDef))
            {
                if (!Restrictions.bedRestrictions[bedDef].Contains(p.def))
                {
                    __result = false;
                }
            }
        }
    }
    [HarmonyPatch(typeof(PawnApparelGenerator), "GenerateStartingApparelFor")]
    public class apparelPatch
    {
        [HarmonyPriority(100000)]
        [HarmonyPostfix]
        public static void GenerateStartingApparelForPostfix() =>
            Traverse.Create(typeof(PawnApparelGenerator)).Field(name: "allApparelPairs").GetValue<List<ThingStuffPair>>().AddRange(apparel);

        private static HashSet<ThingStuffPair> apparel;
        [HarmonyPriority(100000)]
        [HarmonyPrefix]
        public static void GenerateStartingApparelForPrefix(Pawn pawn)
        {
            Traverse apparelInfo = Traverse.Create(typeof(PawnApparelGenerator)).Field(name: "allApparelPairs");

            apparel = new HashSet<ThingStuffPair>();

            foreach (ThingStuffPair pair in apparelInfo.GetValue<List<ThingStuffPair>>().ListFullCopy())
            {
                ThingDef thing = pair.thing;
                if (Restrictions.equipmentRestrictions.ContainsKey(thing))
                {
                    //Log.Message(t.def.defName + " is in building restrictions.");
                    List<ThingDef> races = Restrictions.equipmentRestrictions[thing];
                    if ((!races.Contains(pawn.def) || Restrictions.equipabblbleWhiteLists.ContainsKey(thing) && !Restrictions.equipabblbleWhiteLists[thing].Contains(pawn.def)))
                    {
                        apparel.Add(pair);
                    }

                }
                if (pawn.def is RimValiRaceDef valiRaceDef)
                {
                    if (valiRaceDef.restrictions.canOnlyUseApprovedApparel)
                    {
                        if (thing.IsApparel)
                        {
                            if ((Restrictions.equipmentRestrictions.ContainsKey(thing) && !Restrictions.equipmentRestrictions[thing].Contains(valiRaceDef)) || (Restrictions.equipabblbleWhiteLists.ContainsKey(thing) && !Restrictions.equipabblbleWhiteLists[thing].Contains(pawn.def)))
                            {
                                apparel.Add(pair);
                            }
                        }
                    }
                }
            }
            foreach (ThingStuffPair pair in apparel)
            {
                apparelInfo.GetValue<List<ThingStuffPair>>().Remove(pair);

            }
        }
    }
    [HarmonyPatch(typeof(TraitSet), "GainTrait")]
    public class traitPatch
    {
        [HarmonyPrefix]
        public static bool traitGain(Trait trait, TraitSet __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (Restrictions.traitRestrictions.ContainsKey(trait.def))
            {
                if (Restrictions.traitRestrictions[trait.def].Contains(pawn.def))
                {
                    return true;
                }
                return false;
            }
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                if (rimValiRaceDef.restrictions.disabledTraits.Contains(trait.def))
                {
                    return false;
                }
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(ThoughtUtility), "GiveThoughtsForPawnOrganHarvested")]
    public static class organPatch
    {
        static void ThoughtAdder(Pawn pawn, Pawn victim, bool guest = false)
        {
            if (pawn.def is RimValiRaceDef def)
            {
                foreach (raceOrganHarvestThought rOHT in def.butcherAndHarvestThoughts.harvestedThoughts)
                {
                    if (rOHT.race == victim.def)
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(rOHT.thought);
                        return;
                    }
                }
                if (def.butcherAndHarvestThoughts.careAboutUndefinedRaces)
                {
                    if (!guest) {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowColonistOrganHarvested);
                        return;
                    }
                    pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowGuestOrganHarvested);
                    return;
                }
            }
            if (!guest)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowColonistOrganHarvested);
                return;
            }
            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowGuestOrganHarvested);
        }



        [HarmonyPostfix]
        public static void patch(Pawn victim)
        {
            if (!victim.RaceProps.Humanlike)
            {
                return;
            }
            ThoughtDef def = null;
            bool isGuest;
            if (victim.IsColonist)
            {
                isGuest = false;
            }
            else if(victim.HostFaction == Faction.OfPlayer){
                isGuest = true;
            }
            foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners)
            {
                if (pawn.needs.mood != null)
                {
                    if (pawn == victim)
                    {
                        if (pawn.def is RimValiRaceDef rDef)
                        {
                            pawn.needs.mood.thoughts.memories.TryGainMemory(rDef.butcherAndHarvestThoughts.myOrganHarvested, null);
                        }
                        else
                        {
                            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.MyOrganHarvested, null);
                        }
                    }else if (def != null)
                    {
                        if(pawn.def is RimValiRaceDef rDef)
                        {
                            
                           
                        }
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(Corpse), "ButcherProducts")]
    public static class butcherPatch
    {
        //Gets the thought for butchering.
        static void butcheredThoughAdder(Pawn pawn, Pawn butchered, bool butcher= true)
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
            if (butcher)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.ButcheredHumanlikeCorpse);
                return;
            }
            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowButcheredHumanlikeCorpse);
        }
        [HarmonyPrefix]
        public static bool patch(Pawn butcher, float efficiency, ref IEnumerable<Thing> __result, Corpse __instance)
        {
            TaleRecorder.RecordTale(TaleDefOf.ButcheredHumanlikeCorpse, new object[] { butcher });
            Pawn deadPawn = __instance.InnerPawn;


            __result = deadPawn.ButcherProducts(butcher, efficiency);

            if (!(deadPawn.def is RimValiRaceDef))
            {
                return false;
            }
            if(butcher.def is RimValiRaceDef def)
            {
                butcheredThoughAdder(butcher, deadPawn);
            }
            foreach(Pawn targetPawn in butcher.Map.mapPawns.SpawnedPawnsInFaction(butcher.Faction))
            {
                if(targetPawn.def is RimValiRaceDef rVRDef)
                {
                    butcheredThoughAdder(butcher, deadPawn,false);
                }
            }





            return false;
        }

    }

    [HarmonyPatch(typeof(ThoughtUtility), "CanGetThought_NewTemp")]
    public static class thoughtPatch
    {
        [HarmonyPostfix]
        public static void CanGetPatch(Pawn pawn, ThoughtDef def, bool checkIfNullified, ref bool __result)
        {
            
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                rimValiRaceDef.replaceThought(ref def);
                if (!rimValiRaceDef.restrictions.thoughtBlacklist.NullOrEmpty())
                {
                    if (rimValiRaceDef.restrictions.thoughtBlacklist.Contains(def))
                    {
                        __result = false;
                        return;
                    }
                }
                if (!Restrictions.thoughtRestrictions.EnumerableNullOrEmpty())
                {
                    if (Restrictions.thoughtRestrictions.ContainsKey(def) && !Restrictions.thoughtRestrictions[def].Contains(pawn.def))
                    {
                        __result = false;
                        return;
                    }
                    //if __result needs to = false, __result = false, otherwise, don't change this
                    //otherwise every single thought possible will get applied at once, which is bad.
                    /*else
                    {
                        __result = true;
                        return;
                    }*/
                }
                
            }
            __result = __result && true;
        }
    }
    [HarmonyPatch(typeof(PawnBioAndNameGenerator), "FillBackstorySlotShuffled")]
    public class storyPatch
    {
        private static float SelectionWeightFactorFromWorkTagsDisabled(WorkTags wt)
        {
            float num = 1f;
            if ((wt & WorkTags.ManualDumb) != WorkTags.None)
            {
                num *= 0.5f;
            }
            if ((wt & WorkTags.ManualSkilled) != WorkTags.None)
            {
                num *= 1f;
            }
            if ((wt & WorkTags.Violent) != WorkTags.None)
            {
                num *= 0.6f;
            }
            if ((wt & WorkTags.Social) != WorkTags.None)
            {
                num *= 0.7f;
            }
            if ((wt & WorkTags.Intellectual) != WorkTags.None)
            {
                num *= 0.4f;
            }
            if ((wt & WorkTags.Firefighting) != WorkTags.None)
            {
                num *= 0.8f;
            }
            return num;
        }
        private static float BackstorySelectionWeight(Backstory bs)
        {
            return SelectionWeightFactorFromWorkTagsDisabled(bs.workDisables);
        }
        private static void FillBackstorySlotShuffled(Pawn pawn, BackstorySlot slot, ref Backstory backstory, Backstory backstoryOtherSlot, List<BackstoryCategoryFilter> backstoryCategories, FactionDef factionType)
        {
            BackstoryCategoryFilter backstoryCategoryFilter = backstoryCategories.RandomElementByWeight((BackstoryCategoryFilter c) => c.commonality);
            if (backstoryCategoryFilter == null)
            {
                Log.Error("Backstory category filter was null");
            }
            if (!(from bs in BackstoryDatabase.ShuffleableBackstoryList(slot, backstoryCategoryFilter).TakeRandom(20)
                  where slot != BackstorySlot.Adulthood || !bs.requiredWorkTags.OverlapsWithOnAnyWorkType(pawn.story.childhood.workDisables)
                  select bs).TryRandomElementByWeight(new Func<Backstory, float>(BackstorySelectionWeight), out backstory))
            {
                Log.Error(string.Concat(new object[]
                {
                    "No shuffled ",
                    slot,
                    " found for ",
                    pawn.ToStringSafe<Pawn>(),
                    " of ",
                    factionType.ToStringSafe<FactionDef>(),
                    ". Choosing random."
                }), false);
                backstory = (from kvp in BackstoryDatabase.allBackstories
                             where kvp.Value.slot == slot
                             select kvp).RandomElement<KeyValuePair<string, Backstory>>().Value;
                foreach (RVRBackstory story in DefDatabase<RVRBackstory>.AllDefsListForReading)
                {
                    if (story.defName == backstory.identifier)
                    {
                        if (!story.CanSpawn(pawn))
                        {
                            FillBackstorySlotShuffled(pawn, slot, ref backstory, backstoryOtherSlot, pawn.Faction.def.backstoryFilters, factionType);
                        }
                    }
                }
            }
        }
        [HarmonyPostfix]
        public static void checkStory(Pawn pawn, BackstorySlot slot, ref Backstory backstory, Backstory backstoryOtherSlot, List<BackstoryCategoryFilter> backstoryCategories, FactionDef factionType)
        {
            foreach (RVRBackstory story in DefDatabase<RVRBackstory>.AllDefsListForReading)
            {
                if (story.defName == backstory.identifier)
                {
                    if (!story.CanSpawn(pawn))
                    {
                        FillBackstorySlotShuffled(pawn, slot, ref backstory, backstoryOtherSlot, pawn.Faction.def.backstoryFilters, factionType);
                    }

                }
            }
        }
    }
    [HarmonyPatch(typeof(MemoryThoughtHandler), "GetFirstMemoryOfDef")]
    public static class thoughtReplacerPatchGetFirstMemoriesOfDef
    {
        [HarmonyPrefix]
        public static void patch(ref ThoughtDef def, MemoryThoughtHandler __instance)
        {
            if (__instance.pawn.def is RimValiRaceDef RVDef)
            {
                RVDef.replaceThought(ref def);
            }
        }
    }
    [HarmonyPatch(typeof(MemoryThoughtHandler), "NumMemoriesOfDef")]
    public static class thoughtReplacerPatchNumMemoriesOfDef
    {
        [HarmonyPrefix]
        public static void patch(ref ThoughtDef def, MemoryThoughtHandler __instance)
        {
            if (__instance.pawn.def is RimValiRaceDef RVDef)
            {
                RVDef.replaceThought(ref def);
            }
        }
    }
    [HarmonyPatch(typeof(MemoryThoughtHandler), "OldestMemoryOfDef")]
    public static class thoughtReplacerPatchOldestMemoriesOfDef
    {
        [HarmonyPrefix]
        public static void patch(ref ThoughtDef def, MemoryThoughtHandler __instance)
        {
            if (__instance.pawn.def is RimValiRaceDef RVDef)
            {
                RVDef.replaceThought(ref def);
            }
        }
    }

    [HarmonyPatch(typeof(MemoryThoughtHandler), "RemoveMemoriesOfDef")]
    public static class thoughtReplacerPatchRemoveRemoriesOfDef
    {
        [HarmonyPrefix]
        public static void patch(ref ThoughtDef def, MemoryThoughtHandler __instance)
        {
            if (__instance.pawn.def is RimValiRaceDef RVDef)
            {
                RVDef.replaceThought(ref def);
            }
        }
    }
    [HarmonyPatch(typeof(MemoryThoughtHandler), "RemoveMemoriesOfDefIf")]
    public static class thoughtReplacerPatchRemoveRemoriesOfDefIf
    {
        [HarmonyPrefix]
        public static void patch(ref ThoughtDef def, MemoryThoughtHandler __instance)
        {
            if (__instance.pawn.def is RimValiRaceDef RVDef)
            {
                RVDef.replaceThought(ref def);
            }
        }
    }
    [HarmonyPatch(typeof(MemoryThoughtHandler), "TryGainMemory", new[] { typeof(Thought_Memory), typeof(Pawn) })]
    public static class memGain
    {
        [HarmonyPrefix]
        public static bool patch(Thought_Memory newThought, MemoryThoughtHandler __instance)
        {
            if (__instance.pawn.def is RimValiRaceDef RVDef)
            {
                Thought_Memory nT = newThought;
                RVDef.replaceThought(ref nT.def);

                newThought = ThoughtMaker.MakeThought(nT.def, newThought.CurStageIndex);
                
                
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SituationalThoughtHandler), "TryCreateThought")]
    public static class thoughtReplacerPatchSituational {
        [HarmonyPrefix]
        //Never gets called???
        public static bool replaceThought(ref ThoughtDef def, SituationalThoughtHandler __instance)
        {
            //Log.Message("test1");
            Pawn pawn = __instance.pawn;
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                rimValiRaceDef.replaceThought(ref def);
                //Log.Message("Test2");
                
            }
            return !Traverse.Create(__instance).Field(name: "tmpCachedThoughts").GetValue<HashSet<ThoughtDef>>().Contains(def);
        }
    }
    [HarmonyPatch(typeof(PawnBioAndNameGenerator), "GeneratePawnName")]
    public static class NameFix
    {
        [HarmonyPrefix]
        public static bool nameFix(ref Name __result, Pawn pawn, NameStyle style = NameStyle.Full, string forcedLastName = null)
        {
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                string nameString = NameGenerator.GenerateName(rimValiRaceDef.race.GetNameGenerator(pawn.gender));
                NameTriple name = NameTriple.FromString(nameString);
                string firstName = name.First;
                string nick = name.Nick;
                string lastName = name.Last;
                if (nick == null)
                {
                    nick = firstName;
                }
                __result = new NameTriple(firstName, nick, lastName);
            }
            else
            {
                return true;
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(PawnRenderer), "BaseHeadOffsetAt")]
    public static class headPatch
    {
        [HarmonyPostfix]
        public static void setPos(ref Vector3 __result, Rot4 rotation, PawnRenderer __instance)
        {
            Pawn pawn = __instance.graphics.pawn;
            
            PawnGraphicSet set = __instance.graphics;
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                //This is an automatic check to see if we can put the head position here.
                //no human required
                if (rimValiRaceDef.bodyPartGraphics.Where(x => x.defName.ToLower() == "head").Count() > 0)
                {
                    
                    Vector2 offset = new Vector2(0, 0);
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
                        } else if (rotation == Rot4.West)
                        {
                            offset = rimValiRaceDef.graphics.headOffsets.west;
                        }
                    }
                    RenderableDef headDef = rimValiRaceDef.bodyPartGraphics.First(x => x.defName.ToLower() == "head");
                    __instance.graphics.headGraphic.drawSize = headDef.south.size;
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
                        pos.z = headDef.south.position.y + offset.y;

                    }
                    else if (pawn.Rotation == Rot4.North)
                    {
                        pos.x = headDef.north.position.x + offset.x;
                        pos.z = headDef.north.position.y + offset.y;
                    }
                    else if (pawn.Rotation == Rot4.East)
                    {
                        pos.x = headDef.east.position.x + offset.x;
                        pos.z = headDef.east.position.y + offset.y;
                    }
                    else
                    {
                        pos.x = headDef.west.position.x + offset.x;
                        pos.z = headDef.west.position.y + offset.y;
                    }
                    //Log.Message(pos.ToString());
                    __result = __result + pos;
                }

            }
        }
    }

    //Generation patch for bodytypes
    [HarmonyPatch(typeof(PawnGenerator), "GenerateBodyType_NewTemp")]
    public static class BodyPatch
    {
        [HarmonyPostfix]
        public static void bodyPatch(ref Pawn pawn)
        {
            if (pawn.def is RimValiRaceDef rimValiRace)
            {
                try
                {

                    pawn.story.crownType = CrownType.Average;
                    pawn.story.bodyType = rimValiRace.mainSettings.bodyTypeDefs[UnityEngine.Random.Range(0, rimValiRace.mainSettings.bodyTypeDefs.Count - 1)];

                }
                catch (Exception e)
                {
                    Log.Message(e.Message);
                    Log.Message("Trying again...");
                    bodyPatch(ref pawn);
                }
            }
            else
            {
                List<BodyTypeDef> bodyTypesAvailible = new List<BodyTypeDef>();
                bodyTypesAvailible.AddRange(DefDatabase<BodyTypeDef>.AllDefsListForReading.Where(x => !Restrictions.bodyTypeRestrictions.ContainsKey(x)));
                pawn.story.bodyType = bodyTypesAvailible.RandomElement();
            }
        }
    }

    //I dont think these patches interefere with HAR, nor should HAR patches interefere with these?

    //Was going to patch WillEat, but this seems better? I'd imagine they still *could* eat it by force if i patched WillEat.
    [HarmonyPatch(typeof(RaceProperties), "CanEverEat", new[] { typeof(ThingDef) })]
    public static class FoodPatch
    {
        [HarmonyPostfix]
        public static void edible(ref bool __result, RaceProperties __instance, ThingDef t)
        {
            if (Restrictions.consumableRestrictions.ContainsKey(t))
            {
                List<ThingDef> races = Restrictions.consumableRestrictions[t];
                ThingDef pawn = DefDatabase<ThingDef>.AllDefs.Where<ThingDef>(x => x.race != null && x.race == __instance).First();
                if (!races.Contains(pawn))
                {
                    JobFailReason.Is(pawn.label + " " + "CannotEat".Translate());
                    __result = false;
                }
                else
                {
                    //No "Consume grass" for you.
                    __result = __result && true;
                }
            }
            else
            {
                __result = __result && true;
            }
        }
    }
    //Cant patch CanEquip, apparently. This still works though.
    [HarmonyPatch(typeof(EquipmentUtility), "CanEquip_NewTmp")]
    public static class ApparelPatch
    {
        [HarmonyPostfix]
        public static void equipable(ref bool __result, Thing thing, Pawn pawn, ref string cantReason)
        {

            if (Restrictions.equipmentRestrictions.ContainsKey(thing.def))
            {
                //Log.Message(t.def.defName + " is in building restrictions.");
                List<ThingDef> races = Restrictions.equipmentRestrictions[thing.def];
                if ((!races.Contains(pawn.def) || !Restrictions.equipabblbleWhiteLists[thing.def].Contains(pawn.def)))
                {
                    __result = false;
                    cantReason = pawn.def.label + " " + "CannotWear".Translate();
                    return;
                }

            }
            if (pawn.def is RimValiRaceDef valiRaceDef)
            {
                if (valiRaceDef.restrictions.canOnlyUseApprovedApparel)
                {
                    if (thing.def.IsApparel)
                    {
                        if ((Restrictions.equipmentRestrictions.ContainsKey(thing.def) && Restrictions.equipmentRestrictions[thing.def].Contains(valiRaceDef)) || (Restrictions.equipabblbleWhiteLists.ContainsKey(thing.def) && Restrictions.equipabblbleWhiteLists[thing.def].Contains(pawn.def)))
                        {
                            __result = true;
                            cantReason = pawn.def.label + " " + "CannotWear".Translate();
                            return;
                        }
                        else
                        {
                            __result = false;
                            cantReason = pawn.def.label + " " + "CannotWear".Translate();
                            return;
                        }
                    }
                }
            }
            __result = true;
            return;
        }
    }

    [HarmonyPatch(typeof(GenConstruct), "CanConstruct")]
    //This was confusing at first, but it works.
    public static class ConstructPatch
    {
        [HarmonyPostfix]
        public static void constructable(Thing t, Pawn p, ref bool __result)
        {
            //Log.Message(t.def.ToString());
            if (Restrictions.buildingRestrictions.ContainsKey(t.def.entityDefToBuild))
            {
                //Log.Message(t.def.defName + " is in building restrictions.");
                List<string> races = Restrictions.buildingRestrictions[t.def.entityDefToBuild];
                if (!races.Contains(p.def.defName))
                {
                    __result = false;
                    JobFailReason.Is(p.def.label + " " + "CannotBuild".Translate());
                    return;
                }
                else
                {
                    __result = true && __result;
                    return;
                }
            }
            __result = true && __result;
            return;
        }
    }
    [HarmonyPatch(typeof(PawnGraphicSet), "ResolveAllGraphics")]
    public static class ResolvePatch
    {

        [HarmonyPrefix]
        public static bool ResolveGraphics(PawnGraphicSet __instance)
        {
            Pawn pawn = __instance.pawn;
            if (pawn.def is RimValiRaceDef rimvaliRaceDef)
            {
                graphics graphics = rimvaliRaceDef.graphics;
                colorComp colorComp = pawn.TryGetComp<colorComp>();

                if (colorComp.colors == null || colorComp.colors.Count() == 0)
                {
                    rimvaliRaceDef.GenColors(pawn);
                }
                if (!ColorInfo.sets.ContainsKey(pawn.GetHashCode().ToString()))
                {
                    ColorInfo.sets.Add(pawn.GetHashCode().ToString(), __instance);
                }
                List<Colors> colors = graphics.colorSets;
                if (graphics.skinColorSet != null)
                {
                    TriColor_ColorGenerators generators = colors.First<Colors>(x => x.name == graphics.skinColorSet).colorGenerator;
                    Color color1 = generators.firstColor.NewRandomizedColor();
                    Color color2 = generators.secondColor.NewRandomizedColor();
                    Color color3 = generators.thirdColor.NewRandomizedColor();
                    AvaliGraphic nakedGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(graphics.bodyTex, ContentFinder<Texture2D>.Get(graphics.bodyTex + "_south") == null ? AvaliShaderDatabase.Tricolor :
                                                                 AvaliShaderDatabase.Tricolor, graphics.bodySize, color1, color2, color3);
                    __instance.nakedGraphic = nakedGraphic;

                    //Find the pawns head graphic and set it..
                    AvaliGraphic headGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(graphics.headTex, ContentFinder<Texture2D>.Get(graphics.headTex + "_south") == null ? AvaliShaderDatabase.Tricolor :
                                                                 AvaliShaderDatabase.Tricolor, graphics.headSize, color1, color2, color3);
                    __instance.headGraphic = headGraphic;

                    //First, let's get the pawns hair texture.
                    AvaliGraphic hairGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(__instance.pawn.story.hairDef.texPath, ContentFinder<Texture2D>.Get(graphics.headTex + "_south") == null ? AvaliShaderDatabase.Tricolor :
                                                                 AvaliShaderDatabase.Tricolor, graphics.headSize, pawn.story.SkinColor);
                    //Should the race have hair?
                    if (!rimvaliRaceDef.hasHair)
                    {
                        //This leads to a blank texture. So the pawn doesnt have hair, visually. I might (and probably should) change this later.
                        hairGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>("avali/Heads/AvaliHead");

                    }
                    __instance.hairGraphic = hairGraphic;

                    __instance.headStumpGraphic = hairGraphic;
                    //__instance.MatsBodyBaseAt(pawn.Rotation,bodyCondition=__instance.)
                }
                else
                {

                    //This is the "body" texture of the pawn.

                    AvaliGraphic nakedGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(graphics.bodyTex, ContentFinder<Texture2D>.Get(graphics.bodyTex + "_south") == null ? AvaliShaderDatabase.Tricolor :
                                                                 AvaliShaderDatabase.Tricolor, graphics.bodySize, pawn.story.SkinColor, Color.green, Color.red);
                    __instance.nakedGraphic = nakedGraphic;

                    //Find the pawns head graphic and set it..
                    AvaliGraphic headGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(graphics.headTex, ContentFinder<Texture2D>.Get(graphics.headTex + "_south") == null ? AvaliShaderDatabase.Tricolor :
                                                                 AvaliShaderDatabase.Tricolor, graphics.headSize, pawn.story.SkinColor, Color.green, Color.red);
                    __instance.headGraphic = headGraphic;

                    //First, let's get the pawns hair texture.
                    AvaliGraphic hairGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(__instance.pawn.story.hairDef.texPath, ContentFinder<Texture2D>.Get(graphics.headTex + "_south") == null ? AvaliShaderDatabase.Tricolor :
                                                                 AvaliShaderDatabase.Tricolor, graphics.headSize, pawn.story.SkinColor);

                    //Should the race have hair?
                    if (!rimvaliRaceDef.hasHair)
                    {
                        //This leads to a blank texture. So the pawn doesnt have hair, visually. I might (and probably should) change this later.
                        hairGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>("avali/Heads/AvaliHead");

                    }
                    __instance.hairGraphic = hairGraphic;
                }
                __instance.ResolveApparelGraphics();
                //PortraitsCache.SetDirty(pawn);
                return false;
            }
            return true;
        }
    }


    //This patch helps with automatic resizing, and apparel graphics
    [HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
    public static class Avali_ApparelGraphicRecordGetter_TryGetGraphicApparel_AvaliSpecificHat_Patch
    {
        [HarmonyPostfix]
        public static void Avali_SpecificHatPatch(
          ref Apparel apparel,
          ref BodyTypeDef bodyType,
          ref ApparelGraphicRecord rec)
        {
           
            if (bodyType != AvaliMod.AvaliDefs.Avali && bodyType != AvaliMod.AvaliDefs.Avali)
                return;
            if (apparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead)
            {
                string path = apparel.def.apparel.wornGraphicPath + "_" + bodyType.defName;
                if (apparel.Wearer.def is RimValiRaceDef def)
                {
                    Pawn pawn = apparel.Wearer;
                    if (!((UnityEngine.Object)ContentFinder<Texture2D>.Get(path + "_north", false) == (UnityEngine.Object)null) && !((UnityEngine.Object)ContentFinder<Texture2D>.Get(path + "_east", false) == (UnityEngine.Object)null) && !((UnityEngine.Object)ContentFinder<Texture2D>.Get(path + "_south", false) == (UnityEngine.Object)null))
                    {
                        Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.Cutout,apparel.def.graphicData.drawSize/def.bodyPartGraphics.First(x=>x.defName.ToLower()=="head").south.size, apparel.DrawColor);
                        rec = new ApparelGraphicRecord(graphic, apparel);
                    }
                }
                else
                {
                    if (apparel.Wearer.def is RimValiRaceDef defTwo)
                    {
                        Pawn pawn = apparel.Wearer;
                        
                        if (!((UnityEngine.Object)ContentFinder<Texture2D>.Get(path + "_north", false) == (UnityEngine.Object)null) && !((UnityEngine.Object)ContentFinder<Texture2D>.Get(path + "_east", false) == (UnityEngine.Object)null) && !((UnityEngine.Object)ContentFinder<Texture2D>.Get(path + "_south", false) == (UnityEngine.Object)null))
                        {
                            Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.Cutout, apparel.def.graphicData.drawSize/defTwo.bodyPartGraphics.First(x => x.defName.ToLower() == "head").south.size, apparel.DrawColor);
                            rec = new ApparelGraphicRecord(graphic, apparel);
                        }
                    }
                    else
                    {
                        if (!((UnityEngine.Object)ContentFinder<Texture2D>.Get(path + "_north", false) == (UnityEngine.Object)null) && !((UnityEngine.Object)ContentFinder<Texture2D>.Get(path + "_east", false) == (UnityEngine.Object)null) && !((UnityEngine.Object)ContentFinder<Texture2D>.Get(path + "_south", false) == (UnityEngine.Object)null))
                        {
                            Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.Cutout, apparel.def.graphicData.drawSize, apparel.DrawColor);
                            rec = new ApparelGraphicRecord(graphic, apparel);
                        }
                    }
                }
            }
            else if (!apparel.def.apparel.wornGraphicPath.NullOrEmpty())
            {
                string str = apparel.def.apparel.wornGraphicPath + "_" + bodyType.defName;
                if ((UnityEngine.Object)ContentFinder<Texture2D>.Get(str + "_north", false) == (UnityEngine.Object)null || (UnityEngine.Object)ContentFinder<Texture2D>.Get(str + "_east", false) == (UnityEngine.Object)null || (UnityEngine.Object)ContentFinder<Texture2D>.Get(str + "_south", false) == (UnityEngine.Object)null)
                {
                    Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(apparel.def.apparel.wornGraphicPath, ShaderDatabase.Cutout, apparel.def.graphicData.drawSize, apparel.DrawColor);
                    rec = new ApparelGraphicRecord(graphic, apparel);
                }
            }
        }
    }

    //Render renderables the correct way in the portrait. 
    [HarmonyPatch(typeof(PawnRenderer), "RenderPortrait")]
    static class RenderPatch
    {
        [HarmonyPostfix]
        static void Portrait(PawnRenderer __instance)
        {
            try
            {
                Pawn pawn = __instance.graphics.pawn;
                Vector3 zero = Vector3.zero;
                if (pawn.def is RimValiRaceDef rimValiRaceDef)
                {
                    RenderPatchTwo.RenderBodyParts(true, zero, __instance, Rot4.South);
                }
            }
            catch(Exception error)
            {
                //Achivement get! How did we get here?
                Log.Error("Something has gone terribly wrong! Error: \n" + error.Message);
            }
        }
    }

    public static class ColorInfo
    {
        public static Dictionary<string, PawnGraphicSet> sets = new Dictionary<string, PawnGraphicSet>();
    }

  [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool) }) ]
    static class RenderPatchTwo
    {

        public static void RenderBodyParts(bool portrait, float angle, Vector3 vector, PawnRenderer pawnRenderer, Rot4 rotation)
        {
            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
            Pawn pawn = pawnRenderer.graphics.pawn;
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {

                foreach (RenderableDef renderable in rimValiRaceDef.bodyPartGraphics)
                {
                    if (renderable.CanShow(pawn))
                    {
                        colorComp colorComp = pawn.TryGetComp<colorComp>();
                        Vector3 offset = new Vector3();
                        Vector2 size = new Vector2();
                        if (renderable.west == null)
                        {
                            renderable.west = new BodyPartGraphicPos();
                            renderable.west.position.x = -renderable.east.position.x;
                            renderable.west.position.y = -renderable.east.position.y;
                            renderable.west.size = renderable.east.size;
                            renderable.west.layer = renderable.east.layer;
                        }
                        if (rotation == Rot4.East)
                        {
                            offset = new Vector3(renderable.east.position.x, renderable.east.layer, renderable.east.position.y);
                            size = renderable.east.size;
                        }
                        else if (rotation == Rot4.North)
                        {
                            offset = new Vector3(renderable.north.position.x, renderable.north.layer, renderable.north.position.y);
                            size = renderable.north.size;
                        }
                        else if (rotation == Rot4.South)
                        {
                            offset = new Vector3(renderable.south.position.x, renderable.south.layer, renderable.south.position.y);
                            size = renderable.south.size;
                        }
                        else if (rotation == Rot4.West)
                        {
                            offset = new Vector3(renderable.west.position.x, renderable.west.layer, renderable.west.position.y);
                            size = renderable.west.size;
                        }
                        if (renderable.useColorSet != null)
                        {
                            graphics graphics = rimValiRaceDef.graphics;
                            List<Colors> colors = graphics.colorSets;
                            TriColor_ColorGenerators generators = colors.First<Colors>(x => x.name == graphics.skinColorSet).colorGenerator;
                            /*Color color1 = generators.firstColor.NewRandomizedColor();
                            Color color2 = generators.secondColor.NewRandomizedColor();
                            Color color3 = generators.thirdColor.NewRandomizedColor();*/
                            Color color1 = Color.red;
                            Color color2 = Color.green;
                            Color color3 = Color.blue;
                            string colorSetToUse = renderable.useColorSet;
                            if (colorComp.colors.ContainsKey(colorSetToUse))
                            {
                                color1 = colorComp.colors[colorSetToUse].colorOne;
                                color2 = colorComp.colors[colorSetToUse].colorTwo;
                                color3 = colorComp.colors[colorSetToUse].colorThree;
                            }
                            else
                            {
                                Log.ErrorOnce("Pawn graphics does not contain color set: " + renderable.useColorSet + " for " + renderable.defName + ", going to fallback RGB colors. (These should look similar to your mask colors)", 1);
                            }

                            AvaliGraphic graphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(renderable.texPath(pawn), ContentFinder<Texture2D>.Get(renderable.texPath(pawn) + "south", false) == null ? AvaliShaderDatabase.Tricolor : AvaliShaderDatabase.Tricolor, size, color1, color2, color3);
                            GenDraw.DrawMeshNowOrLater(graphic.MeshAt(rotation), vector + offset.RotatedBy(Mathf.Acos(Quaternion.Dot(Quaternion.identity, quaternion)) * 2f * 57.29578f),
                            Quaternion.AngleAxis(0, Vector3.up) * quaternion, graphic.MatAt(rotation), portrait);
                        }
                        else
                        {
                            AvaliGraphic graphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(renderable.texPath(pawn), ContentFinder<Texture2D>.Get(renderable.texPath(pawn) + "south", false) == null ? AvaliShaderDatabase.Tricolor : AvaliShaderDatabase.Tricolor, size, pawn.story.SkinColor);
                            GenDraw.DrawMeshNowOrLater(graphic.MeshAt(rotation), vector + offset.RotatedBy(Mathf.Acos(Quaternion.Dot(Quaternion.identity, quaternion)) * 2f * 57.29578f),
                            Quaternion.AngleAxis(0, Vector3.up) * quaternion, graphic.MatAt(rotation), portrait);
                        }
                    }
                }
            }
        }
        public static void RenderBodyParts(bool portrait, Vector3 vector, PawnRenderer pawnRenderer, Rot4 rotation)
        {

            Pawn pawn = pawnRenderer.graphics.pawn;
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {

                foreach (RenderableDef renderable in rimValiRaceDef.bodyPartGraphics)
                {
                    if (renderable.CanShowPortrait(pawn))
                    {
                        colorComp colorComp = pawn.TryGetComp<colorComp>();
                        Vector3 offset = new Vector3();
                        Vector2 size = new Vector2();
                        if (renderable.west == null)
                        {
                            renderable.west = new BodyPartGraphicPos();
                            renderable.west.position.x = -renderable.east.position.x;
                            renderable.west.position.y = renderable.east.position.y;
                            renderable.west.size = renderable.east.size;
                            renderable.west.layer = renderable.east.layer;
                        }
                        if (rotation == Rot4.East)
                        {
                            offset = new Vector3(renderable.east.position.x, renderable.east.layer, renderable.east.position.y);
                            size = renderable.east.size;
                        }
                        else if (rotation == Rot4.North)
                        {
                            offset = new Vector3(renderable.north.position.x, renderable.north.layer, renderable.north.position.y);
                            size = renderable.north.size;
                        }
                        else if (rotation == Rot4.South)
                        {
                            offset = new Vector3(renderable.south.position.x, renderable.south.layer, renderable.south.position.y);
                            size = renderable.south.size;
                        }
                        else if (rotation == Rot4.West)
                        {
                            offset = new Vector3(renderable.west.position.x, renderable.west.layer, renderable.west.position.y);
                            size = renderable.west.size;
                        }
                        if (renderable.useColorSet != null)
                        {
                            graphics graphics = rimValiRaceDef.graphics;
                            List<Colors> colors = graphics.colorSets;
                            TriColor_ColorGenerators generators = colors.First<Colors>(x => x.name == graphics.skinColorSet).colorGenerator;
                            /*Color color1 = generators.firstColor.NewRandomizedColor();
                            Color color2 = generators.secondColor.NewRandomizedColor();
                            Color color3 = generators.thirdColor.NewRandomizedColor();*/
                            Color color1 = Color.red;
                            Color color2 = Color.green;
                            Color color3 = Color.blue;
                            string colorSetToUse = renderable.useColorSet;
                            if (colorComp.colors.ContainsKey(colorSetToUse))
                            {
                                color1 = colorComp.colors[colorSetToUse].colorOne;
                                color2 = colorComp.colors[colorSetToUse].colorTwo;
                                color3 = colorComp.colors[colorSetToUse].colorThree;
                            }

                            AvaliGraphic graphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(renderable.texPath(pawn), ContentFinder<Texture2D>.Get(renderable.texPath(pawn) + "south", false) == null ? AvaliShaderDatabase.Tricolor : AvaliShaderDatabase.Tricolor, size, color1, color2, color3);
                            GenDraw.DrawMeshNowOrLater(graphic.MeshAt(rotation), offset + vector.RotatedBy(Mathf.Acos(Quaternion.Dot(Quaternion.identity, Quaternion.identity)) * 2f * 57.29578f),
                            Quaternion.AngleAxis(0, Vector3.up), graphic.MatAt(rotation), portrait);
                        }
                        else
                        {
                            AvaliGraphic graphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(renderable.texPath(pawn), ContentFinder<Texture2D>.Get(renderable.texPath(pawn) + "south", false) == null ? AvaliShaderDatabase.Tricolor : AvaliShaderDatabase.Tricolor, size, pawn.story.SkinColor);
                            GenDraw.DrawMeshNowOrLater(graphic.MeshAt(rotation), offset + vector.RotatedBy(Mathf.Acos(Quaternion.Dot(Quaternion.identity, Quaternion.identity)) * 2f * 57.29578f),
                            Quaternion.AngleAxis(0, Vector3.up), graphic.MatAt(rotation), portrait);
                        }
                    }
                }
            }
        }
        [HarmonyPostfix]
        static void RenderPawnInternal(Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump, bool invisible, PawnRenderer __instance)
        {
            Pawn pawn = __instance.graphics.pawn;
            PawnGraphicSet graphics = __instance.graphics;
            
            if (__instance.graphics.pawn.def is RimValiRaceDef)
            {
                if (!portrait)
                {
                    RenderBodyParts(portrait, angle, rootLoc, __instance, __instance.graphics.pawn.Rotation);


                }

            }
        }
    }
    
    [HarmonyPatch(typeof(WorkGiver_Researcher), "ShouldSkip")]
    public class researchPatch
    {
        [HarmonyPatch]
        static void research(Pawn pawn, ref bool __result)
        {
            if (Find.ResearchManager.currentProj != null)
            {
                if (Restrictions.researchRestrictions.Count() > 0 && Restrictions.researchRestrictions.ContainsKey(Find.ResearchManager.currentProj))
                {
                    if (!Restrictions.researchRestrictions[Find.ResearchManager.currentProj].Contains(pawn.def))
                    {
                        __result = false;
                    }
                    else
                    {
                        __result = true && __result;
                    }
                }
            }
        }
    }
}