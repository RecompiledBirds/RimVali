using HarmonyLib;
using RimWorld;
namespace AvaliMod
{
    [HarmonyPatch(typeof(CompHatcher), "Hatch")]
    internal class EggPatch
    {
        [HarmonyPostfix]
        private static void HatchPatch(CompHatcher __instance)
        {
            if (__instance.hatcheeParent == null & __instance.Props.hatcherPawn.RaceProps.body.defName == "RimValiBody")
            {
                __instance.hatcheeFaction = Faction.OfPlayer;
            }
        }
    }
}
