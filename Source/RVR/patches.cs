
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;

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
            foreach (RimValiRaceDef raceDef in RimValiDefChecks.races)
            {
                Log.Message(raceDef.defName);
                if (raceDef.restrictions.buildables.Count>0)
                {
                    foreach (ThingDef thing in raceDef.restrictions.buildables)
                    {
                        if (!(DefDatabase<ThingDef>.AllDefs.ToList().Contains(thing))){
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
                    foreach(ThingDef thing in raceDef.restrictions.equippablesWhitelist)
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
            if(pawn.def is RimValiRaceDef rimValiRace)
            {
                int randChoice = Random.RandomRange(0, rimValiRace.mainSettings.bodyTypeDefs.Count - 1);
                pawn.story.bodyType = rimValiRace.mainSettings.bodyTypeDefs[randChoice];
            }
        }
    }

    //I dont think these patches interefere with HAR, nor should HAR patches interefere with these?

    //Was going to patch WillEat, but this seems better? I'd imagine they still *could* eat it by force if i patched WillEat.
    [HarmonyPatch(typeof(RaceProperties), "CanEverEat", new[] { typeof(ThingDef)
})]
    public static class FoodPatch
    {
        [HarmonyPostfix]
        public static void edible(ref bool __result, RaceProperties __instance, ThingDef t)
        {
            if (Restrictions.consumableRestrictions.ContainsKey(t))
            {
                List<ThingDef> races = Restrictions.consumableRestrictions[t];
                ThingDef pawn = DefDatabase<ThingDef>.AllDefs.Where<ThingDef>(x=> x.race!=null && x.race==__instance).First();
                if (!races.Contains(pawn)){
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
                if (!races.Contains(pawn.def) && !(Restrictions.equipabblbleWhiteLists.ContainsKey(thing.def) ? Restrictions.equipabblbleWhiteLists[thing.def].Contains(pawn.def) : false))
                {
                    __result = false;
                    cantReason = "Test";
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

    //This was confusing at first, but it works.
    [HarmonyPatch(typeof(GenConstruct), "CanConstruct")]
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
        public static bool ResolveAllGraphicsPrefix(PawnGraphicSet __instance)
        {
            Pawn pawn = __instance.pawn;
            if (pawn.def is RimValiRaceDef rimvaliRaceDef)
            {
                graphics graphics = rimvaliRaceDef.graphics;
                



                //This is the "body" texture of the pawn.
                AvaliGraphic nakedGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(graphics.bodyTex, ContentFinder<Texture2D>.Get(graphics.bodyTex + "_northm", reportFailure: false) == null ? AvaliShaderDatabase.Tricolor :
                                                             AvaliShaderDatabase.Tricolor, Vector2.one, pawn.story.SkinColor);
                __instance.nakedGraphic = nakedGraphic;

                //Find the pawns head graphic and set it..
                AvaliGraphic headGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(graphics.headTex, ContentFinder<Texture2D>.Get(graphics.headTex + "_northm", reportFailure: false) == null ? AvaliShaderDatabase.Tricolor :
                                                             AvaliShaderDatabase.Tricolor, Vector2.one, pawn.story.SkinColor);
                __instance.headGraphic = headGraphic;

                //First, let's get the pawns hair texture.
                AvaliGraphic hairGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(__instance.pawn.story.hairDef.texPath, ContentFinder<Texture2D>.Get(graphics.headTex + "_northm", reportFailure: false) == null ? AvaliShaderDatabase.Tricolor :
                                                             AvaliShaderDatabase.Tricolor, Vector2.one, pawn.story.SkinColor);

                //Should the race have hair?
                if (!rimvaliRaceDef.hasHair)
                {
                    //This leads to a blank texture. So the pawn doesnt have hair, visually. I might (and probably should) change this later.
                    hairGraphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>("avali/Heads/AvaliHead");

                }
                __instance.hairGraphic = hairGraphic;
                foreach (RenderableDef renderable in rimvaliRaceDef.bodyPartGraphics)
                {
                    Log.Message(renderable.linkedBodyPart);

                }
                return false;
            }
            return true;
        }
    }

}