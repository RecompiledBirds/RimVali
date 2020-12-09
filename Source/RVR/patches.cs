
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
                        if (!buildingWhitelists.ContainsKey(thing))
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

                if (raceDef.restrictions.modContentRestrictionsApparelWhiteList.Count > 0)
                {
                    foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading.Where(x => raceDef.restrictions.modContentRestrictionsApparelWhiteList.Contains(x.PackageId)))
                    {
                        foreach (ThingDef def in mod.AllDefs.Where(x => x is ThingDef thingDef && thingDef.IsApparel))
                        {
                            if (!Restrictions.equipabblbleWhiteLists.ContainsKey(def))
                            {
                                Restrictions.equipabblbleWhiteLists.Add(def, new List<ThingDef>());
                                Restrictions.equipabblbleWhiteLists[def].Add(raceDef);
                            }
                            else
                            {
                                Restrictions.equipabblbleWhiteLists[def].Add(raceDef);
                            }
                        }
                    }
                }
                if (raceDef.useHumanRecipes)
                {
                    Log.Message("Adding recipes to race: " + raceDef.label);
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
                            Log.Message("Adding recipe: " + recipe.defName);
                            raceDef.recipes.Add(recipe);
                        }
                        foreach (BodyPartDef bodyPart in recipe.appliedOnFixedBodyParts)
                        {
                            foreach (BodyPartRecord bodyPartRecord in raceDef.race.body.AllParts)
                            {
                                BodyPartDef def = bodyPartRecord.def;
                                if (def == bodyPart)
                                {
                                    Log.Message("Adding recipe: " + recipe.defName);
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
                        if (story.CanSpawn(pawn))
                        {
                            Log.Message("Story can spawn");
                        }
                        else
                        {
                            FillBackstorySlotShuffled(pawn, slot, ref backstory, backstoryOtherSlot, backstoryCategories, factionType);
                        }
                    }
                }
            }
        }
        [HarmonyPostfix]
        public static void checkStory(Pawn pawn, BackstorySlot slot, ref Backstory backstory, Backstory backstoryOtherSlot, List<BackstoryCategoryFilter> backstoryCategories, FactionDef factionType)
        {
            backstory = backstory = (from kvp in BackstoryDatabase.allBackstories
                                     where kvp.Value.slot == slot
                                     select kvp).RandomElement<KeyValuePair<string, Backstory>>().Value;
            foreach (RVRBackstory story in DefDatabase<RVRBackstory>.AllDefsListForReading)
            {
                if (story.defName == backstory.identifier)
                {
                    if (story.CanSpawn(pawn))
                    {
                        Log.Message("Story can spawn");
                    }
                    /*else
                    {
                        FillBackstorySlotShuffled(pawn, slot, ref backstory, backstoryOtherSlot, backstoryCategories, factionType);
                    }*/
                }
            }
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
                if (!(rimValiRaceDef.bodyPartGraphics.Where<RenderableDef>(x => x.defName.ToLower() == "head").Count() > 0))
                {
                    Vector2 offset = new Vector2(0, 0);
                    if (rimValiRaceDef.graphics.headOffset != null)
                    {
                        offset = rimValiRaceDef.graphics.headOffset;
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
                    Log.Message(pos.ToString());
                    __result = pos;
                }
                else
                {
                    __result = __result;
                }
            }
            else
            {
                __result = __result;
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
                    System.Random randChoice = new System.Random();

                    //You would think this works, but 1/2 the time it throws an error. Huh.
                    //pawn.story.bodyType =rimValiRace.mainSettings.bodyTypeDefs[randChoice.Next(rimValiRace.mainSettings.bodyTypeDefs.Count-1)];

                    //This is what HAR does, but wouldn't the above effectively be the same thing?
                    pawn.story.bodyType = rimValiRace.mainSettings.bodyTypeDefs.RandomElement<BodyTypeDef>();
                    //Apparently that throws the error too occasionally. 
                }
                catch (Exception e)
                {
                    Log.Message(e.Message);
                    bodyPatch(ref pawn);
                }
            }
        }
    }

    //I dont think these patches interefere with HAR, nor should HAR patches interefere with these?

    //Was going to patch WillEat, but this seems better? I'd imagine they still *could* eat it by force if i patched WillEat.
    public static class FoodPatch
    {
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
                    cantReason = pawn.Label + " CannotWear".Translate();
                    return;
                }

            }
            __result = true;
            return;
        }
    }

    //This was confusing at first, but it works.
    public static class ConstructPatch
    {
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
                    __result = true;
                    return;
                }
            }
            __result = true;
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


    //This patch helps with automatic resizing.
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
            Pawn pawn = apparel.Wearer;

            if (pawn.def is RimValiRaceDef valiRaceDef)
            {
                AvaliGraphic graphic = new AvaliGraphic();
                string path = apparel.def.apparel.wornGraphicPath + "_" + bodyType.defName;
                if (!((UnityEngine.Object)ContentFinder<Texture2D>.Get(path + "_north", false) == (UnityEngine.Object)null) && !((UnityEngine.Object)ContentFinder<Texture2D>.Get(path + "_east", false) == (UnityEngine.Object)null) && !((UnityEngine.Object)ContentFinder<Texture2D>.Get(path + "_south", false) == (UnityEngine.Object)null))
                {

                    if (valiRaceDef.graphics.headSize != null)
                    {
                        graphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(path, AvaliShaderDatabase.Tricolor, valiRaceDef.graphics.headSize, apparel.DrawColor);
                    }
                    else
                    {
                        graphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(path, AvaliShaderDatabase.Tricolor, apparel.def.graphicData.drawSize, apparel.DrawColor);
                    }
                }
                else
                {
                    graphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(path, AvaliShaderDatabase.Tricolor, apparel.def.graphicData.drawSize, apparel.DrawColor);
                }

                rec = new ApparelGraphicRecord(graphic, apparel);
            }
            else if (!apparel.def.apparel.wornGraphicPath.NullOrEmpty())
            {
                string str = apparel.def.apparel.wornGraphicPath + "_" + bodyType.defName;
                if ((UnityEngine.Object)ContentFinder<Texture2D>.Get(str + "_north", false) == (UnityEngine.Object)null || (UnityEngine.Object)ContentFinder<Texture2D>.Get(str + "_east", false) == (UnityEngine.Object)null || (UnityEngine.Object)ContentFinder<Texture2D>.Get(str + "_south", false) == (UnityEngine.Object)null)
                {
                    AvaliGraphic graphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(apparel.def.apparel.wornGraphicPath, AvaliShaderDatabase.Tricolor, apparel.def.graphicData.drawSize, apparel.DrawColor);
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
            Pawn pawn = __instance.graphics.pawn;
            Vector3 zero = Vector3.zero;
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                RenderPatchTwo.RenderBodyParts(true, zero, __instance, Rot4.South);
            }
        }
    }



    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool) })]
    static class RenderPatchTwo
    {

        public static void RenderBodyParts(bool portrait, Quaternion quaternion, Vector3 vector, PawnRenderer pawnRenderer, Rot4 rotation)
        {

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
                    if (renderable.CanShow(pawn))
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
            Mesh mesh = null;
            PawnGraphicSet graphics = __instance.graphics;
            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
            if (__instance.graphics.pawn.def is RimValiRaceDef)
            {
                if (!portrait)
                {
                    RenderBodyParts(portrait, quaternion, rootLoc, __instance, __instance.graphics.pawn.Rotation);


                }

            }
        }
    }
    [HarmonyPatch(typeof(WorkGiver_Researcher), "ShouldSkip")]
    public class researchPatch
    {
        [HarmonyPostfix]
        static void research(Pawn pawn, ref bool __result)
        {
            if (Find.ResearchManager.currentProj != null)
            {
                if (Restrictions.researchRestrictions.Count() > 0 && Restrictions.researchRestrictions.ContainsKey(Find.ResearchManager.currentProj))
                {
                    if (Restrictions.researchRestrictions[Find.ResearchManager.currentProj].Contains(pawn.def))
                    {
                        __result = true && __result;
                    }
                    else
                    {
                        __result = false;
                    }
                }
                else
                {
                    __result = __result;
                }
            }
        }
    }
}