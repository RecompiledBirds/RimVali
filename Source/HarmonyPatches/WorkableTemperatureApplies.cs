using HarmonyLib;
using RimWorld;
using Verse;

//Based off code by Shearion. Thank you for the help!
//-Nesi
namespace AvaliMod
{
    [HarmonyPatch(typeof(StatPart_WorkTableTemperature), nameof(StatPart_WorkTableTemperature.Applies),
        typeof(ThingDef), typeof(Map), typeof(IntVec3))]
    public static class WorkableTemperatureApplies
    {
        [HarmonyPostfix]
        public static void TemperaturePatch(ThingDef thingDef, ref bool __result)
        {
            // Result is already true. We only ever set it to false.
            if (!__result)
            {
                return;
            }

            //The original method is gonna apply the patch, so decide whether to change that result to "no, don't apply the stat".
            if (thingDef.defName == AvaliDefs.AvaliNanoForge.defName ||
                thingDef.defName == AvaliDefs.AvaliNanoLoom.defName ||
                thingDef.defName == AvaliDefs.AvaliResearchBench.defName)
            {
                //Set your result here. true = stat will apply, false = stat will not apply.
                __result = false;
            }
        }
    }
}
