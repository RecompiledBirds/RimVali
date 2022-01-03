using HarmonyLib;
using RimWorld;

namespace AvaliMod
{
    [HarmonyPatch(typeof(CompHatcher), nameof(CompHatcher.Hatch))]
    public static class EggPatch
    {
        [HarmonyPostfix]
        public static void HatchPatch(CompHatcher __instance)
        {
            if (__instance.hatcheeParent == null &&
                __instance.Props.hatcherPawn.RaceProps.body.defName == "RimValiBody")
            {
                __instance.hatcheeFaction = Faction.OfPlayer;
            }
        }
    }
}
