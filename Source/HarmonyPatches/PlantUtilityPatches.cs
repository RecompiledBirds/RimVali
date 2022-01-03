using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace AvaliMod
{
    [HarmonyPatch(typeof(PlantUtility), "CanSowOnGrower")]
    public static class CanGrowOn_Patch
    {
        public static void Postfix(ThingDef plantDef, object obj, ref bool __result)
        {
            if (obj is Zone z && z.Map.terrainGrid.TerrainAt(z.Position).affordances.Contains(AvaliDefs.IcySoil))
            {
                __result = plantDef.plant.sowTags.Contains("IcySoil");
            }
        }
    }


    [HarmonyPatch(typeof(Zone_Growing), "GetInspectString")]
    //A minor tweak that makes the inspector appear correctly.
    public static class Zone_Growing_GetInspectString_Patch
    {
        public static void Prefix(ref string __result, Zone_Growing __instance)
        {
            string text = "";
            if (!__instance.Cells.NullOrEmpty())
            {
                IntVec3 c = __instance.Cells.First();
                if (c.UsesOutdoorTemperature(__instance.Map))
                {
                    text += "OutdoorGrowingPeriod".Translate() + ": " + Zone_Growing.GrowingQuadrumsDescription(__instance.Map.Tile) + "\n";
                }
                if (PlantUtility.GrowthSeasonNow(c, __instance.Map, true) || __instance.GetPlantDefToGrow().plant.sowTags.Contains("IcySoil")&&c.GetTerrain(__instance.Map).affordances.Contains(AvaliDefs.IcySoil))
                {
                    text += "GrowSeasonHereNow".Translate();
                }
                else
                {
                    text += "CannotGrowBadSeasonTemperature".Translate();
                }
            }
            __result= text;
        }
    }
}
