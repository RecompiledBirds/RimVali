using HarmonyLib;
using RimWorld;
using Verse;
namespace AvaliMod
{
    [HarmonyPatch(typeof(CompHatcher))]
    [HarmonyPatch("Hatch")]
    internal class SetEggFaction
    {
        public static void Prefix(CompHatcher __instance)
        {
            if (__instance.hatcheeParent == null & __instance.Props.hatcherPawn.RaceProps.body.defName.ToString() == "NeziAvaliBody")
                __instance.hatcheeFaction = Faction.OfPlayer;
            Log.Message("Changed hatched avali's faction to player faction.", false);
        }
    }
}
