using HarmonyLib;
using RimWorld;
using Verse;
namespace AvaliMod
{
    [HarmonyPatch(typeof(CompHatcher),"Hatch")]
    class EggPatch
    {
        [HarmonyPostfix]
        static void HatchPatch(CompHatcher __instance)
        {
            if (__instance.hatcheeParent == null & __instance.Props.hatcherPawn.RaceProps.body.defName == "RimValiBody")
                __instance.hatcheeFaction = Faction.OfPlayer;
        }
    }
}
