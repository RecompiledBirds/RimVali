using System;
using RimWorld;
using Verse;
using AlienRace;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace AvaliMod
{
    
    [StaticConstructorOnStartup]
    public static class HARTweak
    {
        static HARTweak()
        {
            var harmony = new Harmony("Rimvali.HarPatches");
            Log.Message("Starting HAR patches. [RimVali Compatiblity]");
            try
            {
                harmony.Patch(AccessTools.Method(typeof(RimValiDefChecks), name: "potentialRaces"), null, new HarmonyMethod(typeof(ReturnDataPatch), "ReturnDataRaces"));
                Log.Message("Patches completed!");
                RestrictionsPatch.AddRestrictions();
            }catch (Exception error)
            {
                Log.Message("Patches failed! [RimVali Compatiblity]");
                Log.Message(error.Message);
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
    public class ReturnDataPatch
    {
        public static List<AlienRace.ThingDef_AlienRace> potentialPackRaces = DefDatabase<AlienRace.ThingDef_AlienRace>.AllDefsListForReading;
        public void ReturnDataRaces(ref List<ThingDef> __result)
        {
            __result = DefDatabase<ThingDef>.AllDefs.Where(x => x.race != null).ToList();
            __result.AddRange(potentialPackRaces);

        }
    }
}
