using HarmonyLib;
using RimWorld;
using Verse;
namespace AvaliMod
{
    [HarmonyPatch(typeof(CompHatcher), "Hatch")]
    class EggPatch
    {
        static void Postfix(CompHatcher __instance)
        {
            if (__instance.hatcheeParent == null & __instance.Props.hatcherPawn.RaceProps.body.defName == "RimValiBody")
                __instance.hatcheeFaction = Faction.OfPlayer;
        }
    }
}
