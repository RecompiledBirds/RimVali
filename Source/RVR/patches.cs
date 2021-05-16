
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using Verse.AI;
using System.Reflection;
using System.Threading.Tasks;

namespace AvaliMod
{
    #region ApparelUtility HasPartsToWear patch
    [HarmonyPatch(typeof(ApparelUtility), "HasPartsToWear")]
    public static class ApparelUtilityPatch
    {
        [HarmonyPostfix]
        static void Patch(Pawn p, ThingDef apparel, ref bool __result)
        {
            __result = Restrictions.checkRestrictions(Restrictions.equipmentRestrictions, apparel, p.def);
            if (p.def is RimValiRaceDef valiRaceDef && valiRaceDef.restrictions.canOnlyUseApprovedApparel && !ApparelPatch.CanWearHeavyRestricted(apparel, p))
            {
                __result = false;
            }
            __result = __result && true;
        }
    }
    #endregion 
    #region Research restriction patch

    [HarmonyPatch(typeof(WorkGiver_Researcher), "ShouldSkip")]
    public class ResearchPatch
    {
        [HarmonyPostfix]
        static void Research(Pawn pawn, ref bool __result)
        {
            //Log.Message("test");
            if (Find.ResearchManager.currentProj != null)
            {
                // Log.Message($"Is blacklisted: {(Restrictions.factionResearchBlacklist.ContainsKey(pawn.Faction.def) && Restrictions.factionResearchBlacklist[pawn.Faction.def].Any(res => res.proj == Find.ResearchManager.currentProj))}");
                if (!Restrictions.checkRestrictions(Restrictions.researchRestrictions, Find.ResearchManager.currentProj, pawn.def) || (Restrictions.factionResearchRestrictions.ContainsKey(pawn.Faction.def) && !Restrictions.factionResearchRestrictions[pawn.Faction.def].Any(res => res.proj == Find.ResearchManager.currentProj)) || (Restrictions.factionResearchBlacklist.ContainsKey(pawn.Faction.def) && Restrictions.factionResearchBlacklist[pawn.Faction.def].Any(res => res.proj == Find.ResearchManager.currentProj)))
                {
                    HackingGameComp hackingGameComp = Current.Game.World.GetComponent<HackingGameComp>();
                    if (hackingGameComp != null && !hackingGameComp.hackProjects.EnumerableNullOrEmpty() && !(hackingGameComp.hackProjects.ContainsKey(Find.ResearchManager.currentProj) || hackingGameComp.hackProjects[Find.ResearchManager.currentProj] == false))
                    {

                        __result = false;
                    }
                }
                __result = true && __result;
            }
        }
    }
    #endregion
    #region Restrictions
    #region FactionResearch
    public class FacRes
    {
        public ResearchProjectDef proj;
        public bool hackable;

        public FacRes(ResearchProjectDef projectDef, bool canBeHacked)
        {
            hackable = canBeHacked;
            proj = projectDef;
        }
    }
    #endregion
    [StaticConstructorOnStartup]
    public static class Restrictions
    {
        //We use the same method HAR uses for handling restrictions for compatiblity stuff.
        //Normal restrictions
        public static Dictionary<ThingDef, List<ThingDef>> equipmentRestrictions = new Dictionary<ThingDef, List<ThingDef>>();
        public static Dictionary<ThingDef, List<ThingDef>> consumableRestrictions = new Dictionary<ThingDef, List<ThingDef>>();
        public static Dictionary<ThingDef, List<ThingDef>> consumableRestrictionsWhiteList = new Dictionary<ThingDef, List<ThingDef>>();
        public static Dictionary<BuildableDef, List<string>> buildingRestrictions = new Dictionary<BuildableDef, List<string>>();

        public static Dictionary<ResearchProjectDef, List<ThingDef>> researchRestrictions = new Dictionary<ResearchProjectDef, List<ThingDef>>();

        public static Dictionary<TraitDef, List<ThingDef>> traitRestrictions = new Dictionary<TraitDef, List<ThingDef>>();

        public static Dictionary<BodyTypeDef, List<ThingDef>> bodyTypeRestrictions = new Dictionary<BodyTypeDef, List<ThingDef>>();

        public static Dictionary<ThingDef, List<ThingDef>> bedRestrictions = new Dictionary<ThingDef, List<ThingDef>>();

        public static Dictionary<ThoughtDef, List<ThingDef>> thoughtRestrictions = new Dictionary<ThoughtDef, List<ThingDef>>();
        //Whitelists
        public static Dictionary<ThingDef, List<ThingDef>> buildingWhitelists = new Dictionary<ThingDef, List<ThingDef>>();
        public static Dictionary<ThingDef, List<ThingDef>> equipabblbleWhiteLists = new Dictionary<ThingDef, List<ThingDef>>();


        public static Dictionary<ThingDef, List<BodyTypeDef>> bodyDefs = new Dictionary<ThingDef, List<BodyTypeDef>>();

        //Faction restrictions
        public static Dictionary<FactionDef, List<FacRes>> factionResearchRestrictions = new Dictionary<FactionDef, List<FacRes>>();
        public static Dictionary<FactionDef, List<FacRes>> factionResearchBlacklist = new Dictionary<FactionDef, List<FacRes>>();
        /// <summary>
        /// A way of condensing most of our restriction logic into one nice little function. May have uses elsewhere, as well.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="pairs"> A dictionary, using Dictionary<T,List<V>>.</param>
        /// <param name="item">The key to check.</param>
        /// <param name="race">We check if this is in the list.</param>
        /// <returns>Returns true if:
        /// pairs contains item, and pairs[item] contains race
        /// otherwise returns false.
        /// </returns>
        public static bool checkRestrictions<T, V>(Dictionary<T, List<V>> pairs, T item, V race, bool keyNotInReturn = true) {
            if (pairs.ContainsKey(item))
            {
                if (pairs[item] is List<V>)
                {
                    if (pairs[item].Contains(race))
                    {
                        return true;
                    }
                }
            }
            //For things that arent in the dictionary
            if (!pairs.ContainsKey(item))
            {
                return keyNotInReturn;
            }
            return false;
        }

        /// <summary>
        /// Adds a restriction to the given restriction dictionary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="pairs">The dictionary to modify</param>
        /// <param name="item">The item to restrict</param>
        /// <param name="race">The race to give it</param>
        /// <returns>Returns true if it can, returns false if it can't.</returns>
        public static bool AddRestriction<T, V>(ref Dictionary<T, List<V>> pairs, T item, V race)
        {
            if (!pairs.ContainsKey(item)) {
                pairs.Add(item, new List<V>());
                pairs[item].Add(race);
            }
            else
            {
                if (pairs[item] is List<V>)
                {
                    pairs[item].Add(race);
                    return true;
                }
            }
            return false;
        }
        #region Assemble restrictions
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
                        AddRestriction(ref buildingRestrictions, thing, raceDef.defName);
                    }
                }
                if (raceDef.restrictions.consumables.Count > 0)
                {
                    foreach (ThingDef thing in raceDef.restrictions.consumables)
                    {
                        AddRestriction(ref consumableRestrictions, thing, raceDef);
                    }
                }
                if (raceDef.restrictions.equippables.Count > 0)
                {
                    foreach (ThingDef thing in raceDef.restrictions.equippables)
                    {
                        AddRestriction(ref equipmentRestrictions, thing, raceDef);
                    }
                }
                if (raceDef.restrictions.researchProjectDefs.Count > 0)
                {
                    foreach (ResearchProjectDef research in raceDef.restrictions.researchProjectDefs)
                    {
                        AddRestriction(ref researchRestrictions, research, raceDef);
                    }
                }
                if (raceDef.restrictions.traits.Count > 0)
                {
                    foreach (TraitDef trait in raceDef.restrictions.traits)
                    {
                        AddRestriction(ref traitRestrictions, trait, raceDef);
                    }
                }
                if (raceDef.restrictions.thoughtDefs.Count > 0)
                {
                    foreach (ThoughtDef thought in raceDef.restrictions.thoughtDefs)
                    {
                        AddRestriction(ref thoughtRestrictions, thought, raceDef);
                    }
                }
                if (raceDef.restrictions.equippablesWhitelist.Count > 0)
                {
                    foreach (ThingDef thing in raceDef.restrictions.equippablesWhitelist)
                    {
                        AddRestriction(ref equipabblbleWhiteLists, thing, raceDef);
                    }
                }
                if (raceDef.restrictions.bedDefs.Count > 0)
                {
                    foreach (ThingDef thing in raceDef.restrictions.bedDefs)
                    {
                        AddRestriction(ref bedRestrictions, thing, raceDef);
                    }
                }

                if (raceDef.restrictions.bodyTypes.Count > 0)
                {
                    foreach (BodyTypeDef bodyTypeDef in raceDef.restrictions.bodyTypes)
                    {
                        AddRestriction(ref bodyTypeRestrictions, bodyTypeDef, raceDef);
                    }
                }

                if (raceDef.restrictions.modContentRestrictionsApparelWhiteList.Count > 0)
                {

                    foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading.Where(x => raceDef.restrictions.modContentRestrictionsApparelWhiteList.Contains(x.Name) || raceDef.restrictions.modContentRestrictionsApparelWhiteList.Contains(x.PackageId)))
                    {
                        foreach (ThingDef def in mod.AllDefs.Where(x => x is ThingDef thingDef && (thingDef.IsApparel || thingDef.IsWeapon || thingDef.IsMeleeWeapon || thingDef.IsRangedWeapon)))
                        {
                            AddRestriction(ref equipabblbleWhiteLists, def, raceDef);
                        }
                    }
                }

                if (raceDef.restrictions.modContentRestrictionsApparelList.Count > 0)
                {

                    foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading.Where(x => raceDef.restrictions.modContentRestrictionsApparelList.Contains(x.Name) || raceDef.restrictions.modContentRestrictionsApparelList.Contains(x.PackageId)))
                    {
                        foreach (ThingDef def in mod.AllDefs.Where(x => x is ThingDef thingDef && (thingDef.IsApparel || thingDef.IsWeapon || thingDef.IsMeleeWeapon || thingDef.IsRangedWeapon)))
                        {
                            AddRestriction(ref equipmentRestrictions, def, raceDef);
                        }
                    }
                }


                if (raceDef.restrictions.modResearchRestrictionsList.Count > 0)
                {

                    foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading.Where(x => raceDef.restrictions.modResearchRestrictionsList.Contains(x.Name) || raceDef.restrictions.modResearchRestrictionsList.Contains(x.PackageId)))
                    {
                        foreach (ResearchProjectDef research in mod.AllDefs.Where(x => x is ResearchProjectDef))
                        {
                            AddRestriction(ref researchRestrictions, research, raceDef);
                        }
                    }
                }


                if (raceDef.restrictions.modTraitRestrictions.Count > 0)
                {

                    foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading.Where(x => raceDef.restrictions.modTraitRestrictions.Contains(x.Name) || raceDef.restrictions.modTraitRestrictions.Contains(x.PackageId)))
                    {
                        foreach (TraitDef trait in mod.AllDefs.Where(x => x is TraitDef))
                        {
                            AddRestriction(ref traitRestrictions, trait, raceDef);
                        }
                    }
                }


                if (raceDef.restrictions.modBuildingRestrictions.Count > 0)
                {

                    foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading.Where(x => raceDef.restrictions.modBuildingRestrictions.Contains(x.Name) || raceDef.restrictions.modBuildingRestrictions.Contains(x.PackageId)))
                    {
                        foreach (ThingDef def in mod.AllDefs.Where(x => x is ThingDef thingDef)) {
                            AddRestriction(ref buildingRestrictions, def, raceDef.defName);
                        }
                    }
                }
                foreach(BodyTypeDef bDef in raceDef.bodyTypes)
                {
                    AddRestriction(ref bodyDefs, raceDef, bDef);
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
                    List<BodyPartDef> defs = new List<BodyPartDef>();
                    foreach (BodyPartRecord rec in raceDef.race.body.AllParts)
                    {
                        defs.Add(rec.def);
                    }
                    foreach (RecipeDef recipe in ThingDefOf.Human.recipes.Where(recipe => recipe.targetsBodyPart || !recipe.appliedOnFixedBodyParts.NullOrEmpty()))
                    {
                        foreach (BodyPartDef bodyPart in Enumerable.Intersect(recipe.appliedOnFixedBodyParts, defs))
                        {
                            raceDef.recipes.Add(recipe);
                        }

                    }
                    raceDef.recipes.RemoveDuplicates();
                }
            }
            Log.Message("[RVR]: Setting up faction restrictions.");
            foreach (FactionResearchRestrictionDef factionResearchRestrictionDef in DefDatabase<FactionResearchRestrictionDef>.AllDefsListForReading)
            {
                foreach (FactionResearchRestriction factionResearchRestriction in factionResearchRestrictionDef.factionResearchRestrictions)
                {
                    FacRes newRes = new FacRes(factionResearchRestriction.researchProj, factionResearchRestriction.isHackable);
                    if (!factionResearchRestrictions.ContainsKey(factionResearchRestriction.factionDef))
                    {
                        
                        factionResearchRestrictions.Add(factionResearchRestriction.factionDef, new List<FacRes>());
                    }
                    factionResearchRestrictions[factionResearchRestriction.factionDef].Add(newRes);
                }

                foreach (FactionResearchRestriction factionResearchRestriction in factionResearchRestrictionDef.factionResearchRestrictionBlackList)
                {
                    FacRes newRes = new FacRes(factionResearchRestriction.researchProj, factionResearchRestriction.isHackable);
                    if (!factionResearchBlacklist.ContainsKey(factionResearchRestriction.factionDef))
                    {
                        factionResearchBlacklist.Add(factionResearchRestriction.factionDef, new List<FacRes>());
                    }
                    factionResearchBlacklist[factionResearchRestriction.factionDef].Add(newRes);
                }
            }
            #endregion
        }
    }
    #endregion
    #region Pawnkind replacement
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
                for (int raceIndex = 0; raceIndex < races.Count()-1; raceIndex++)
                {
                    RimValiRaceDef race = races.ToList()[raceIndex];
                    RVRRaceInsertion inserter = race.raceInsertion;
                    if (Rand.Range(0, 100) < inserter.globalChance)
                    {
                        if (pawnKindDef == PawnKindDefOf.Slave)
                        {
                            foreach (Entry entry in inserter.entries)
                            {
                                if (entry.isSlave && Rand.Range(0, 100) < entry.chance)
                                {

                                    if (entry.pawnkind != null)
                                    {
                                        pawnKindDef = entry.pawnkind;
                                        request.KindDef = pawnKindDef;
                                        request.ForceBodyType = race.bodyTypes.RandomElement();
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

                                    if (entry.pawnkind != null)
                                    {
                                        pawnKindDef = entry.pawnkind;
                                        request.KindDef = pawnKindDef;
                                        race.bodyTypes.RandomElement();
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
                                    if (entry.pawnkind != null)
                                    {
                                        pawnKindDef = entry.pawnkind;
                                        request.KindDef = pawnKindDef;
                                        race.bodyTypes.RandomElement();
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
                                    if (entry.pawnkind != null)
                                    {
                                        pawnKindDef = entry.pawnkind;
                                        request.KindDef = pawnKindDef;
                                        race.bodyTypes.RandomElement();

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
    #endregion
    #region Bed patch
    [HarmonyPatch(typeof(RestUtility), "CanUseBedEver")]
    public class BedPatch
    {
        [HarmonyPostfix]
        public static void BedPostfix(ref bool __result, Pawn p, ThingDef bedDef)
        {
            if (!Restrictions.checkRestrictions(Restrictions.bedRestrictions, bedDef, p.def))
            {
                __result = false;
            }
        }
    }
    #endregion
    #region Apparel gen patch
    [HarmonyPatch(typeof(PawnApparelGenerator), "GenerateStartingApparelFor")]
    public class apparelPatch
    {
        [HarmonyPrefix]
        public static void GenerateStartingApparelForPrefix(Pawn pawn)
        {
            Traverse apparelInfo = Traverse.Create(typeof(PawnApparelGenerator)).Field(name: "allApparelPairs");
            foreach (ThingStuffPair pair in apparelInfo.GetValue<List<ThingStuffPair>>().ListFullCopy())
            {
                ThingDef thing = pair.thing;
                if (!Restrictions.checkRestrictions(Restrictions.equipmentRestrictions, thing, pawn.def) && !Restrictions.checkRestrictions(Restrictions.equipabblbleWhiteLists, thing, pawn.def))
                {
                    apparelInfo.GetValue<List<ThingStuffPair>>().Remove(pair);
                }

                if (pawn.def is RimValiRaceDef valiRaceDef && valiRaceDef.restrictions.canOnlyUseApprovedApparel && thing.IsApparel && !ApparelPatch.CanWearHeavyRestricted(thing, pawn))
                {
                    apparelInfo.GetValue<List<ThingStuffPair>>().Remove(pair);
                }
            } 
        }
    }
    #endregion
    #region Trait patch
    [HarmonyPatch(typeof(TraitSet), "GainTrait")]
    public class traitPatch
    {
        [HarmonyPrefix]
        public static bool traitGain(Trait trait, TraitSet __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                if (rimValiRaceDef.restrictions.disabledTraits.Contains(trait.def))
                {
                    return false;
                }
            }
            if (Restrictions.checkRestrictions(Restrictions.traitRestrictions, trait.def, pawn.def))
            {
                return true;
            }
            return false;
        }
    }
    #endregion
    #region Organ harvest patch
    [HarmonyPatch(typeof(ThoughtUtility), "GiveThoughtsForPawnOrganHarvested")]
    public static class OrganPatch
    {



        [HarmonyPostfix]
        public static void Patch(Pawn victim)
        {
            if (!victim.RaceProps.Humanlike)
            {
                return;
            }
            if (victim.def is RimValiRaceDef raceDef)
            {
                victim.needs.mood.thoughts.memories.TryGainMemory(raceDef.butcherAndHarvestThoughts.myOrganHarvested, null);
            }
            else
            {
                victim.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.MyOrganHarvested, null);
            }
            foreach (Pawn pawn in victim.Map.mapPawns.AllPawnsSpawned)
            {
                if (pawn.needs.mood != null)
                {
                    if (pawn != victim)
                    {
                        if (pawn.def is RimValiRaceDef rDef)
                        {
                            foreach (raceOrganHarvestThought thoughts in rDef.butcherAndHarvestThoughts.harvestedThoughts)
                            {
                                if (victim.def == thoughts.race)
                                {
                                    if (victim.IsColonist && (thoughts.colonistThought != null))
                                    {
                                        pawn.needs.mood.thoughts.memories.TryGainMemory(thoughts.colonistThought);
                                    }
                                    else if (!victim.IsColonist && thoughts.guestThought != null)
                                    {
                                        pawn.needs.mood.thoughts.memories.TryGainMemory(thoughts.guestThought);
                                    }
                                    else if (thoughts.colonistThought != null)
                                    {
                                        pawn.needs.mood.thoughts.memories.TryGainMemory(thoughts.colonistThought);
                                    }
                                    else
                                    {
                                        Log.Error("Undefined thought in " + rDef.defName + " butcherAndHarvestThoughts/harvestedThoughts!");
                                    }
                                }
                                else if (rDef.butcherAndHarvestThoughts.careAboutUndefinedRaces)
                                {
                                    if (victim.IsColonist)
                                    {
                                        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowColonistOrganHarvested);
                                    }
                                    else
                                    {
                                        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowGuestOrganHarvested);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion
    #region FactionGen patch
    [HarmonyPatch(typeof(Faction), "TryMakeInitialRelationsWith")]
    public static class FactionGenPatch
    {
        [HarmonyPostfix]
        public static void patch(Faction __instance, Faction other)
        {
            foreach (FactionStartRelationDef def in DefDatabase<FactionStartRelationDef>.AllDefs.Where(fac => fac.faction == __instance.def))
            {

                foreach (FacRelation relation in def.relations)
                {
                    if (other.def == relation.otherFaction)
                    {
                        FactionRelation rel = other.RelationWith(__instance);
                        rel.goodwill = relation.relation;
                    }
                }
            }
        }
    }
    #endregion
    #region Health offset patch
    [HarmonyPatch(typeof(BodyPartDef), "GetMaxHealth")]
    public static partial class BodyPartHealthPatch
    {

        [HarmonyPostfix]
        public static void patch(ref float __result, Pawn pawn, BodyPartDef __instance)
        {
            float num = 0f;
            float otherNum = 0f;
            if (pawn.health.hediffSet.hediffs != null)
            {
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs.Where(hediff => hediff.Part != null && hediff.Part.def == __instance))
                {
                    if (hediff.CurStage != null && !hediff.CurStage.statOffsets.NullOrEmpty<StatModifier>())
                    {
                        foreach (StatModifier statModifier in hediff.CurStage.statOffsets.Where((StatModifier x) => x.stat != null && x.stat.defName == "HealthIncreasePercent"))
                        {
                            num += statModifier.value;
                        }
                        foreach (StatModifier statModifier in hediff.CurStage.statOffsets.Where((StatModifier x) => x.stat != null && x.stat.defName == "HealthIncreaseAdd"))
                        {
                            otherNum += statModifier.value;
                        }
                    }
                }
            }
            if (num > 0)
            {
                __result = (float)Mathf.CeilToInt((float)__instance.hitPoints * pawn.HealthScale * num) + otherNum;
            }
            else
            {
                __result = (float)Mathf.CeilToInt((float)__instance.hitPoints * pawn.HealthScale) + otherNum;
            }
            return;
        }
    }
    #endregion
    #region Cannibalism patch
    [HarmonyPatch(typeof(FoodUtility), "ThoughtsFromIngesting")]
    public static class IngestingPatch
    {
        [HarmonyPostfix]
        public static void Patch(Pawn ingester, Thing foodSource, ThingDef foodDef, ref List<ThoughtDef> __result)
        {
            bool cannibal = ingester.story.traits.HasTrait(TraitDefOf.Cannibal);
            CompIngredients ingredients = foodSource.TryGetComp<CompIngredients>();

            #region raw
            if (ingester.def is RimValiRaceDef def && FoodUtility.IsHumanlikeMeat(foodDef))
            {

                if (def.getAllCannibalThoughtRaces().Contains(foodDef.ingestible.sourceDef))
                {
                    __result.Replace(cannibal ? ThoughtDefOf.AteHumanlikeMeatDirectCannibal : ThoughtDefOf.AteHumanlikeMeatDirect, def.getEatenThought(foodDef.ingestible.sourceDef, true, cannibal));
                }
                else
                {
                    if (!def.cannibalismThoughts.careAbountUndefinedRaces)
                    {


                        __result.Remove(cannibal ? ThoughtDefOf.AteHumanlikeMeatDirectCannibal : ThoughtDefOf.AteHumanlikeMeatDirect);

                    }
                }
            }
            #endregion
            if (ingredients == null)
            {
                return;
            }
            #region cooked
            if (ingester.def is RimValiRaceDef rDef)
            {
                if (ingredients.ingredients != null && ingredients.ingredients.Any(f => FoodUtility.IsHumanlikeMeat(f)))
                {

                    if (ingredients.ingredients.Any(fS => fS.ingestible != null && fS.ingestible.sourceDef != null && rDef.getAllCannibalThoughtRaces().Contains(fS.ingestible.sourceDef)))
                    {


                        __result.Replace(cannibal ? ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal : ThoughtDefOf.AteHumanlikeMeatAsIngredient , rDef.getEatenThought(ingredients.ingredients.First(x => rDef.getAllCannibalThoughtRaces().Contains(x.ingestible.sourceDef)), false, cannibal));
                    }
                    else
                    {
                        if (!rDef.cannibalismThoughts.careAbountUndefinedRaces)
                        {

                            __result.Remove(cannibal ? ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal : ThoughtDefOf.AteHumanlikeMeatAsIngredient );

                        }
                    }
                }
            }
            #endregion cooked
        }
    }
    #endregion
    #region Butcher patch
    [HarmonyPatch(typeof(Corpse), "ButcherProducts")]
    public static class ButcherPatch
    {
        //Gets the thought for butchering.
        static void ButcheredThoughAdder(Pawn pawn, Pawn butchered, bool butcher = true)
        {
            if (butchered.RaceProps.Humanlike)
            {
                Log.Message("0");
                #region stories
                try
                {
                    //Backstories
                    if (!DefDatabase<RVRBackstory>.AllDefs.Where(x => x.hasButcherThoughtOverrides == true && (x.defName == pawn.story.adulthood.identifier || x.defName == pawn.story.childhood.identifier)).EnumerableNullOrEmpty())
                    {

                        butcherAndHarvestThoughts butcherAndHarvestThoughts = DefDatabase<RVRBackstory>.AllDefs.Where(x => x.defName == pawn.story.adulthood.identifier || x.defName == pawn.story.childhood.identifier).First().butcherAndHarvestThoughtOverrides;
                        try
                        {
                            if (butcherAndHarvestThoughts.butcherThoughts.Any(x => x.race == butchered.def))
                            {
                                raceButcherThought rBT = butcherAndHarvestThoughts.butcherThoughts.Find(x=>x.race==butchered.def);
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
                #endregion
                #region races
                #region RVR races
                //Races
                if (pawn.def is RimValiRaceDef def)
                {
                    Log.Message("1 rDef");
                    butcherAndHarvestThoughts butcherAndHarvestThoughts = def.butcherAndHarvestThoughts;
                    if (butcherAndHarvestThoughts.butcherThoughts.Any(x => x.race == butchered.def))
                    {
                        raceButcherThought rBT = butcherAndHarvestThoughts.butcherThoughts.Find(x => x.race == butchered.def);
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
                    if (def.butcherAndHarvestThoughts.careAboutUndefinedRaces)
                    {
                        Log.Message("2 rDef");
                        if (butcher)
                        {
                            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.ButcheredHumanlikeCorpse);
                            return;
                        }
                        Log.Message("3 rDef");
                        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowButcheredHumanlikeCorpse);
                        return;
                    }
                }
                #endregion 
                Log.Message("1 otherDef");
                //If the pawn is not from RVR.
                if (!(pawn.def is RimValiRaceDef))
                {
                    if (butcher)
                    {
                        Log.Message("2 otherDef");
                        //why tf isn't this happening?? the log.message happens
                        pawn.needs.mood.thoughts.memories.TryGainMemory(AvaliDefs.ButcheredHumanlikeCorpse, null);
                        return;
                    }
                    Log.Message("3 otherDef");
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AvaliDefs.KnowButcheredHumanlikeCorpse, null);
                }
                #endregion
            }
        }




        [HarmonyPrefix]
        public static bool Patch(Pawn butcher, float efficiency, ref IEnumerable<Thing> __result, Corpse __instance)
        {
            if (Harmony.HasAnyPatches("rimworld.erdelf.alien_race.main"))
            {
                return true;
            }
            TaleRecorder.RecordTale(TaleDefOf.ButcheredHumanlikeCorpse, new object[] { butcher });
            Pawn deadPawn = __instance.InnerPawn;


            __result = deadPawn.ButcherProducts(butcher, efficiency);
            /*
            if (!(deadPawn.def is RimValiRaceDef))
            {
                return false;
            }
            */
            bool butcheredThought = false;
            if (butcher.def is RimValiRaceDef def)
            {
                ButcheredThoughAdder(butcher, deadPawn, true);
                butcheredThought = true;
            }
            foreach (Pawn targetPawn in butcher.Map.mapPawns.SpawnedPawnsInFaction(butcher.Faction))
            {
                if (targetPawn != butcher)
                {
                    Log.Message(targetPawn.Name.ToStringFull);
                    ButcheredThoughAdder(targetPawn, deadPawn, false);
                    
                }else if (!butcheredThought)
                {
                    Log.Message($"Butcher: {targetPawn.Name.ToStringFull}");
                    ButcheredThoughAdder(targetPawn, deadPawn);
                }
            }





            return false;
        }

    }
    #endregion
    #region Backstory patch
    
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
    #endregion
    #region Thought patches
    [HarmonyPatch(typeof(ThoughtUtility), "CanGetThought_NewTemp")]
    public static class thoughtPatch
    {
        [HarmonyPostfix]
        public static void CanGetPatch(Pawn pawn, ThoughtDef def, bool checkIfNullified, ref bool __result)
        {

            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                rimValiRaceDef.ReplaceThought(ref def);
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
                }

            }
            __result = __result && true;
        }
    }
    [HarmonyPatch(typeof(MemoryThoughtHandler), "GetFirstMemoryOfDef")]
    public static class thoughtReplacerPatchGetFirstMemoriesOfDef
    {
        [HarmonyPrefix]
        public static void Patch(ref ThoughtDef def, MemoryThoughtHandler __instance)
        {
            if (__instance.pawn.def is RimValiRaceDef RVDef)
            {
                RVDef.ReplaceThought(ref def);
            }
        }
    }
    [HarmonyPatch(typeof(MemoryThoughtHandler), "NumMemoriesOfDef")]
    public static class thoughtReplacerPatchNumMemoriesOfDef
    {
        [HarmonyPrefix]
        public static void Patch(ref ThoughtDef def, MemoryThoughtHandler __instance)
        {
            if (__instance.pawn.def is RimValiRaceDef RVDef)
            {
                RVDef.ReplaceThought(ref def);
            }
        }
    }
    [HarmonyPatch(typeof(MemoryThoughtHandler), "OldestMemoryOfDef")]
    public static class thoughtReplacerPatchOldestMemoriesOfDef
    {
        [HarmonyPrefix]
        public static void Patch(ref ThoughtDef def, MemoryThoughtHandler __instance)
        {
            if (__instance.pawn.def is RimValiRaceDef RVDef)
            {
                RVDef.ReplaceThought(ref def);
            }
        }
    }

    [HarmonyPatch(typeof(MemoryThoughtHandler), "RemoveMemoriesOfDef")]
    public static class thoughtReplacerPatchRemoveRemoriesOfDef
    {
        [HarmonyPrefix]
        public static void Patch(ref ThoughtDef def, MemoryThoughtHandler __instance)
        {
            if (__instance.pawn.def is RimValiRaceDef RVDef)
            {
                RVDef.ReplaceThought(ref def);
            }
        }
    }
    [HarmonyPatch(typeof(MemoryThoughtHandler), "RemoveMemoriesOfDefIf")]
    public static class thoughtReplacerPatchRemoveRemoriesOfDefIf
    {
        [HarmonyPrefix]
        public static void Patch(ref ThoughtDef def, MemoryThoughtHandler __instance)
        {
            if (__instance.pawn.def is RimValiRaceDef RVDef)
            {
                RVDef.ReplaceThought(ref def);
            }
        }
    }
    [HarmonyPatch(typeof(MemoryThoughtHandler), "TryGainMemory", new[] { typeof(Thought_Memory), typeof(Pawn) })]
    public static class MemGain
    {
        [HarmonyPrefix]
        public static bool Patch(Thought_Memory newThought, MemoryThoughtHandler __instance)
        {
            if (__instance.pawn.def is RimValiRaceDef RVDef)
            {
                Thought_Memory nT = newThought;
                RVDef.ReplaceThought(ref nT.def);

                newThought = ThoughtMaker.MakeThought(nT.def, newThought.CurStageIndex);


            }
            return true;
        }
    }


    [HarmonyPatch(typeof(SituationalThoughtHandler), "TryCreateThought")]
    public static class ThoughtReplacerPatchSituational {
        [HarmonyPrefix]
        public static void ReplaceThought(ref ThoughtDef def, SituationalThoughtHandler __instance)
        {
            Pawn pawn = __instance.pawn;
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                rimValiRaceDef.ReplaceThought(ref def);

            }
        }
    }
    #endregion
    #region Name patch
    [HarmonyPatch(typeof(PawnBioAndNameGenerator), "GeneratePawnName")]
    public static class NameFix
    {
        [HarmonyPrefix]
        public static bool Patch(ref Name __result, Pawn pawn, NameStyle style = NameStyle.Full, string forcedLastName = null)
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
    #endregion
    #region Base Head Offset patch
    [HarmonyPatch(typeof(PawnRenderer), "BaseHeadOffsetAt")]
    public static class HeadPatch
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
                rimValiRaceDef.HeadOffsetPawn(__instance, ref __result);
                
            }
        }
    }
    #endregion
    #region Body gen patch
    //Generation patch for bodytypes
    [HarmonyPatch(typeof(PawnGenerator), "GenerateBodyType_NewTemp")]
    public static class BodyPatch
    {
        public static void SetBody(RVRBackstory story, ref Pawn pawn)
        {
            RimValiRaceDef rimValiRace = pawn.def as RimValiRaceDef;
            if (story.bodyDefOverride != null)
                pawn.RaceProps.body = story.bodyDefOverride;
            if (story.bodyType != null)
                pawn.story.bodyType = story.bodyType;
            else
                pawn.story.bodyType = rimValiRace.bodyTypes.RandomElement();
        }
        [HarmonyPostfix]
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
                        SetBody(story, ref pawn);
                        return;
                    }
                    else if (DefDatabase<RVRBackstory>.AllDefs.Where(x => x.defName == p2.story.childhood.identifier).Count() > 0)
                    {
                        RVRBackstory story = DefDatabase<RVRBackstory>.AllDefs.Where(x => x.defName == p2.story.childhood.identifier).FirstOrDefault();
                        SetBody(story, ref pawn);
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
                if (pawn.def.GetType().Name != "ThingDef_AlienRace")
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
            }
        }
    }
    #endregion
    #region Food Eating
    //I dont think these patches interefere with HAR, nor should HAR patches interefere with these?

    //Was going to patch WillEat, but this seems better? I'd imagine they still *could* eat it by force if i patched WillEat.
    [HarmonyPatch(typeof(RaceProperties), "CanEverEat", new[] { typeof(ThingDef) })]
    public static class FoodPatch
    {
        [HarmonyPostfix]
        public static void edible(ref bool __result, RaceProperties __instance, ThingDef t)
        {

            ThingDef pawn = __instance.AnyPawnKind.race;
            if (!Restrictions.checkRestrictions(Restrictions.consumableRestrictions, t, pawn) && !Restrictions.checkRestrictions(Restrictions.consumableRestrictionsWhiteList, t, pawn))
            {
                JobFailReason.Is(pawn.label + " " + "CannotEat".Translate(pawn.label.Named("RACE")));
                __result = false;
            }

            //No "Consume grass" for you.
            __result = __result && true;

        }
    }
    #endregion
    #region Apparel Equipping
    //Cant patch CanEquip, apparently. This still works though.
    [HarmonyPatch(typeof(EquipmentUtility), "CanEquip_NewTmp")]
    public static class ApparelPatch
    {

        public static bool CanWear(ThingDef def, Pawn pawn)
        {

            return Restrictions.checkRestrictions(Restrictions.equipmentRestrictions, def, pawn.def) || Restrictions.checkRestrictions(Restrictions.equipabblbleWhiteLists, def, pawn.def, false);
        }
        public static bool CanWearHeavyRestricted(ThingDef def, Pawn pawn)
        {

            return Restrictions.checkRestrictions(Restrictions.equipmentRestrictions, def, pawn.def, false) || Restrictions.checkRestrictions(Restrictions.equipabblbleWhiteLists, def, pawn.def, false);
        }
        [HarmonyPostfix]
        public static void equipable(ref bool __result, Thing thing, Pawn pawn, ref string cantReason)
        {
            if (!CanWear(thing.def, pawn))
            {

                __result = false;
                cantReason = pawn.def.label + " " + "CannotWear".Translate(pawn.def.label.Named("RACE"));

            }

            if (pawn.def is RimValiRaceDef valiRaceDef)
            {
                if (valiRaceDef.restrictions.canOnlyUseApprovedApparel)
                {
                    if (thing.def.IsApparel && !thing.def.IsWeapon)
                    {

                        if (CanWearHeavyRestricted(thing.def, pawn))
                        {
                            __result = true;
                        }
                        else
                        {

                            __result = false;
                            cantReason = pawn.def.label + " " + "CannotWear".Translate(pawn.def.label.Named("RACE"));
                        }
                    }
                }
            }
            __result = true && __result;
        }
    }
    #endregion
    #region Construction
    [HarmonyPatch(typeof(GenConstruct), "CanConstruct")]
    //This was confusing at first, but it works.
    public static class ConstructPatch
    {
        [HarmonyPostfix]
        public static void constructable(Thing t, Pawn p, ref bool __result)
        {
            //Log.Message(t.def.ToString());
            if (!Restrictions.checkRestrictions<BuildableDef, string>(Restrictions.buildingRestrictions, t.def.entityDefToBuild, p.def.defName))
            {

                __result = false;
                JobFailReason.Is(p.def.label + " " + "CannotBuild".Translate(p.def.label.Named("RACE")));
                return;
            }

            __result = true && __result;
            return;

        }
    }
    #endregion
    #region ResolveAllGraphics patch
    [HarmonyPatch(typeof(PawnGraphicSet), "ResolveAllGraphics")]
    public static class ResolvePatch
    {
        [HarmonyPrefix]
        public static bool ResolveGraphics(PawnGraphicSet __instance)
        {
            Pawn pawn = __instance.pawn;
            if (pawn.def is RimValiRaceDef rimvaliRaceDef)
            {
                raceColors graphics = rimvaliRaceDef.graphics;
                colorComp colorComp = pawn.TryGetComp<colorComp>();

                if (colorComp.colors == null || colorComp.colors.Count() == 0)
                {
                    rimvaliRaceDef.GenGraphics(pawn);
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
    #endregion
    #region Hat patch
    // //This patch helps with automatic resizing, and apparel graphics
    [HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
    public static class Avali_ApparelGraphicRecordGetter_TryGetGraphicApparel_AvaliSpecificHat_Patch
    {
        [HarmonyPostfix]
        public static void Patch(ref Apparel apparel, ref BodyTypeDef bodyType, ref ApparelGraphicRecord rec)
        {
            Pawn pawn = apparel.Wearer;
            if (apparel.def.apparel.layers.Any(d => d == ApparelLayerDefOf.Overhead))
            {
                if (bodyType != AvaliMod.AvaliDefs.Avali && bodyType != AvaliMod.AvaliDefs.Avali)
                    return;

                string path = $"{apparel.def.apparel.wornGraphicPath}_{bodyType.defName}";
                if (pawn.def is RimValiRaceDef def && (ContentFinder<Texture2D>.Get($"{path}_north", false) != null) && (ContentFinder<Texture2D>.Get($"{path}_east", false) != null) && (ContentFinder<Texture2D>.Get($"{path}_south", false) != null))
                {

                    Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.Cutout, apparel.def.graphicData.drawSize / def.renderableDefs.First(x => x.defName.ToLower() == "head").south.size, apparel.DrawColor);
                    rec = new ApparelGraphicRecord(graphic, apparel);
                }
                else if(!(pawn.def is RimValiRaceDef))
                {

                    if ((ContentFinder<Texture2D>.Get($"{path}_north", false) != null) && (ContentFinder<Texture2D>.Get($"{path}_east", false) != null) && (ContentFinder<Texture2D>.Get($"{path}_south", false) != null))
                    {
                        Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.Cutout, apparel.def.graphicData.drawSize, apparel.DrawColor);
                        rec = new ApparelGraphicRecord(graphic, apparel);
                    }
                }
            }
            else if (!apparel.def.apparel.wornGraphicPath.NullOrEmpty())
            {
                string str = $"{apparel.def.apparel.wornGraphicPath}_{bodyType.defName}";
                if (ContentFinder<Texture2D>.Get($"{str}_north", false) == null || ContentFinder<Texture2D>.Get($"{str}_east", false) == null || ContentFinder<Texture2D>.Get($"{str}_south", false) == null)
                {
                    Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(apparel.def.apparel.wornGraphicPath, ShaderDatabase.Cutout, apparel.def.graphicData.drawSize, apparel.DrawColor);
                    rec = new ApparelGraphicRecord(graphic, apparel);
                }
            }
        }
    }
    #endregion
    #region Portraits patch
    //Render renderables the correct way in the portrait. 
    [HarmonyPatch(typeof(PawnRenderer), "RenderPortrait")]
    static class RenderPatch
    {
        [HarmonyPostfix]
        static void Portrait(PawnRenderer __instance)
        {
            Vector3 zero = Vector3.zero;
            float angle;
            if (__instance.graphics.pawn.Dead || __instance.graphics.pawn.Downed)
            {
                angle = 85f;
                zero.x -= 0.18f;
                zero.z -= 0.18f;
            }
            else
            {
                angle = 0f;
            }
            try
            {

                
                Pawn pawn = __instance.graphics.pawn;
                if (pawn.def is RimValiRaceDef rimValiRaceDef && !pawn.Dead && !pawn.Downed)
                {
                    RenderPatchTwo.RenderBodyParts(true, Vector3.zero, __instance, Rot4.South);

                } else if (pawn.def is RimValiRaceDef rDef)
                {
                    RenderPatchTwo.RenderBodyParts(true, angle, new Vector3(-0.2f, 0, -0.2f), __instance, Rot4.South);
                }
            }
            catch (Exception error)
            {
                //Achivement get! How did we get here?
                Log.Error("Something has gone terribly wrong! Error: \n" + error.Message);
            }
            object[] p = new object[] { zero, angle, true, Rot4.South, Rot4.South, RimValiUtility.GetProp<Rot4>("CurRotDrawMode", obj: __instance), true, !__instance.graphics.pawn.health.hediffSet.HasHead, __instance.graphics.pawn.IsInvisible() };
            RimValiUtility.InvokeMethod("RenderPawnInternal",__instance,p);
            //__instance.RenderPawnInternal(zero, angle, true, Rot4.South, Rot4.South, RimValiUtility.GetProp<Rot4>("CurRotDrawMode",obj: __instance), true, !__instance.graphics.pawn.health.hediffSet.HasHead, __instance.graphics.pawn.IsInvisible());
        }
    }
    #endregion
    public static class ColorInfo
    {
        public static Dictionary<string, PawnGraphicSet> sets = new Dictionary<string, PawnGraphicSet>();
    }
    #region Rendering patch
    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool) })]
    static class RenderPatchTwo
    {
        public static Dictionary<Pawn, PawnRenderer> renders = new Dictionary<Pawn, PawnRenderer>();
        public static void RenderBodyParts(bool portrait, float angle, Vector3 vector, PawnRenderer pawnRenderer, Rot4 rotation)
        {
            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
            Pawn pawn = pawnRenderer.graphics.pawn;
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {

                foreach (RenderableDef renderable in rimValiRaceDef.renderableDefs)
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
                            raceColors graphics = rimValiRaceDef.graphics;
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

                            AvaliGraphic graphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(renderable.texPath(pawn), AvaliShaderDatabase.Tricolor, size, color1, color2, color3);
                            GenDraw.DrawMeshNowOrLater(graphic.MeshAt(rotation), vector + offset.RotatedBy(Mathf.Acos(Quaternion.Dot(Quaternion.identity, quaternion)) * 114.59156f),
                            quaternion, graphic.MatAt(rotation), portrait);
                        }
                        else
                        {
                            AvaliGraphic graphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(renderable.texPath(pawn), AvaliShaderDatabase.Tricolor, size, pawn.story.SkinColor);
                            GenDraw.DrawMeshNowOrLater(graphic.MeshAt(rotation), vector + offset.RotatedBy(Mathf.Acos(Quaternion.Dot(Quaternion.identity, quaternion)) * 114.59156f),
                             quaternion, graphic.MatAt(rotation), portrait);
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

                foreach (RenderableDef renderable in rimValiRaceDef.renderableDefs)
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
                            raceColors graphics = rimValiRaceDef.graphics;
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
                                Log.Error(renderable.defName + " could not fiind color set: " + colorSetToUse);
                            }

                            AvaliGraphic graphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(renderable.texPath(pawn, renderable.GetMyIndex(pawn)), ContentFinder<Texture2D>.Get(renderable.texPath(pawn, renderable.GetMyIndex(pawn)) + "south", false) == null ? AvaliShaderDatabase.Tricolor : AvaliShaderDatabase.Tricolor, size, color1, color2, color3);
                            GenDraw.DrawMeshNowOrLater(graphic.MeshAt(rotation), offset + vector.RotatedBy(Mathf.Acos(Quaternion.Dot(Quaternion.identity, Quaternion.identity)) * 2f * 57.29578f),
                            Quaternion.AngleAxis(0, Vector3.up), graphic.MatAt(rotation), portrait);
                        }
                        else
                        {
                            AvaliGraphic graphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(renderable.texPath(pawn, renderable.GetMyIndex(pawn)), ContentFinder<Texture2D>.Get(renderable.texPath(pawn, renderable.GetMyIndex(pawn)) + "south", false) == null ? AvaliShaderDatabase.Tricolor : AvaliShaderDatabase.Tricolor, size, pawn.story.SkinColor);
                            GenDraw.DrawMeshNowOrLater(graphic.MeshAt(rotation), offset + vector.RotatedBy(Mathf.Acos(Quaternion.Dot(Quaternion.identity, Quaternion.identity)) * 2f * 57.29578f),
                            Quaternion.AngleAxis(0, Vector3.up), graphic.MatAt(rotation), portrait);
                        }
                    }
                }
            }
        }
        static Vector3 southHeadOffset(PawnRenderer __instance)
        {
            return __instance.BaseHeadOffsetAt(Rot4.South);
        }
        [HarmonyPostfix]
        static void RenderPawnInternal(Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump, bool invisible, PawnRenderer __instance)
        {
            
            void Render()
            {
                Pawn pawn = __instance.graphics.pawn;
                PawnGraphicSet graphics = __instance.graphics;
                if (!renders.ContainsKey(pawn))
                {
                    renders.Add(pawn, __instance);
                }
                if (__instance.graphics.pawn.def is RimValiRaceDef && !portrait)
                {
                    Rot4 rot = __instance.graphics.pawn.Rotation;
                    // angle = pawn.Graphic.DrawRotatedExtraAngleOffset;
                    //angle = pawn.Position.AngleFlat;
                    angle = __instance.BodyAngle();
                    Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
                    if (__instance.graphics.pawn.GetPosture() != PawnPosture.Standing)
                    {

                        rot = __instance.LayingFacing();
                        Building_Bed building_Bed = __instance.graphics.pawn.CurrentBed();
                        if (building_Bed != null && __instance.graphics.pawn.RaceProps.Humanlike)
                        {
                            renderBody = building_Bed.def.building.bed_showSleeperBody;
                            AltitudeLayer altLayer = (AltitudeLayer)Mathf.Max((int)building_Bed.def.altitudeLayer, 17);
                            Vector3 vector2;
                            Vector3 a3 = vector2 = __instance.graphics.pawn.Position.ToVector3ShiftedWithAltitude(altLayer);
                            vector2.y += 0.024489796f;
                            Rot4 rotation = building_Bed.Rotation;
                            rotation.AsInt += 2;
                            float d = -__instance.BaseHeadOffsetAt(Rot4.South).z;
                            Vector3 a2 = rotation.FacingCell.ToVector3();
                            rootLoc = a3 + a2 * d;
                            rootLoc.y += 0.009183673f;
                        }
                        else if (!__instance.graphics.pawn.Dead && __instance.graphics.pawn.CarriedBy == null)
                        {
                            rootLoc.y = AltitudeLayer.LayingPawn.AltitudeFor() + 0.009183673f;
                        }

                    }
                    RenderBodyParts(portrait, angle, rootLoc, __instance, rot);



                    if (__instance.graphics.pawn.Spawned && !__instance.graphics.pawn.Dead)
                    {
                        __instance.graphics.pawn.stances.StanceTrackerDraw();
                        __instance.graphics.pawn.pather.PatherDraw();
                    }
                    Vector3 vector = rootLoc;
                    Vector3 a = rootLoc;
                    if (bodyFacing != Rot4.North)
                    {
                        a.y += 0.024489796f;
                        vector.y += 0.021428572f;
                    }
                    else
                    {
                        a.y += 0.021428572f;
                        vector.y += 0.024489796f;
                    }
                    List<ApparelGraphicRecord> apparelGraphics = __instance.graphics.apparelGraphics;
                    if (__instance.graphics.headGraphic != null && !portrait)
                    {
                        Vector3 b = quaternion * __instance.BaseHeadOffsetAt(headFacing);
                        Material material = __instance.graphics.HeadMatAt_NewTemp(headFacing, bodyDrawType, headStump, portrait);
                        if (material != null)
                        {
                            GenDraw.DrawMeshNowOrLater(MeshPool.humanlikeHeadSet.MeshAt(headFacing), a + b, quaternion, material, portrait);
                        }
                        Vector3 loc2 = rootLoc + b;
                        loc2.y += 0.030612245f;
                        bool flag = false;
                        if (!portrait || !Prefs.HatsOnlyOnMap)
                        {
                            Mesh mesh2 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                            for (int j = 0; j < apparelGraphics.Count; j++)
                            {
                                if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead)
                                {
                                    if (!apparelGraphics[j].sourceApparel.def.apparel.hatRenderedFrontOfFace)
                                    {
                                        flag = true;
                                        Material material2 = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
                                        material2 = OverrideMaterialIfNeeded_NewTemp(material2, __instance.graphics.pawn, __instance, portrait);
                                        GenDraw.DrawMeshNowOrLater(mesh2, loc2, quaternion, material2, portrait);
                                    }
                                    else
                                    {
                                        Material material3 = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
                                        material3 = OverrideMaterialIfNeeded_NewTemp(material3, __instance.graphics.pawn, __instance, portrait);
                                        Vector3 loc3 = rootLoc + b;
                                        loc3.y += ((bodyFacing == Rot4.North) ? 0.0030612245f : 0.03367347f);
                                        GenDraw.DrawMeshNowOrLater(mesh2, loc3, quaternion, material3, portrait);
                                    }
                                }
                            }
                        }
                        if (!flag && bodyDrawType != RotDrawMode.Dessicated && !headStump)
                        {
                            Mesh mesh3 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                            Material mat2 = __instance.graphics.HairMatAt_NewTemp(headFacing, portrait);
                            GenDraw.DrawMeshNowOrLater(mesh3, loc2, quaternion, mat2, portrait);
                        }
                    }
                    else if (__instance.graphics.headGraphic != null && portrait)
                    {
                        Vector3 b = quaternion * southHeadOffset(__instance);
                        Material material = __instance.graphics.HeadMatAt_NewTemp(headFacing, bodyDrawType, headStump, portrait);
                        if (material != null)
                        {
                            GenDraw.DrawMeshNowOrLater(MeshPool.humanlikeHeadSet.MeshAt(headFacing), a + b, quaternion, material, portrait);
                        }
                        Vector3 loc2 = rootLoc + b;
                        loc2.y += 0.030612245f;
                        bool flag = false;
                        if (!Prefs.HatsOnlyOnMap)
                        {
                            Mesh mesh2 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                            for (int j = 0; j < apparelGraphics.Count; j++)
                            {
                                if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead)
                                {
                                    if (!apparelGraphics[j].sourceApparel.def.apparel.hatRenderedFrontOfFace)
                                    {
                                        flag = true;
                                        Material material2 = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
                                        material2 = OverrideMaterialIfNeeded_NewTemp(material2, __instance.graphics.pawn, __instance, portrait);
                                        GenDraw.DrawMeshNowOrLater(mesh2, loc2, quaternion, material2, portrait);
                                    }
                                    else
                                    {
                                        Material material3 = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
                                        material3 = OverrideMaterialIfNeeded_NewTemp(material3, __instance.graphics.pawn, __instance, portrait);
                                        Vector3 loc3 = rootLoc + b;
                                        loc3.y += ((bodyFacing == Rot4.North) ? 0.0030612245f : 0.03367347f);
                                        GenDraw.DrawMeshNowOrLater(mesh2, loc3, quaternion, material3, portrait);
                                    }
                                }
                            }
                        }
                        if (!flag && bodyDrawType != RotDrawMode.Dessicated && !headStump)
                        {
                            Mesh mesh3 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                            Material mat2 = __instance.graphics.HairMatAt_NewTemp(headFacing, portrait);
                            GenDraw.DrawMeshNowOrLater(mesh3, loc2, quaternion, mat2, portrait);
                        }
                    }
                }
                
            }
            Render();
        }
        static Material OverrideMaterialIfNeeded_NewTemp(Material original, Pawn pawn, PawnRenderer instance, bool portrait = false)
        {
            Material baseMat = (!portrait && pawn.IsInvisible()) ? InvisibilityMatPool.GetInvisibleMat(original) : original;
            return instance.graphics.flasher.GetDamagedMat(baseMat);
        }

    }
    #endregion
    
  
}