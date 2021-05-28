using HarmonyLib;
using RimWorld;
using System;
using Verse;

//Based off code by Shearion. Thank  you for the help!
//-Nesi
namespace AvaliMod
{
    [HarmonyPatch(typeof(StatPart_WorkTableTemperature), "Applies", new Type[] { typeof(ThingDef), typeof(Map), typeof(IntVec3) })]
    public static class StatPart_WorkTableTemperature_Applies
    {
        [HarmonyPatch]
        public static void tempPatch(ThingDef tDef, Map map, IntVec3 c, ref bool __result)
        {
            if (__result) //The original method is gonna apply the patch, so decide whether to change that result to "no, don't apply the stat".
            {
                if (tDef.defName == AvaliDefs.AvaliNanoForge.defName | tDef.defName == AvaliDefs.AvaliNanoLoom.defName | tDef.defName == AvaliDefs.AvaliResearchBench.defName)
                {
                    __result = false;
                }
                else
                {

                    __result = true; //Set your result here. true = stat will apply, false = stat will not apply.
                }
            }
        }
    }
}