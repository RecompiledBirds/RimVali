using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using HarmonyLib;
using Verse;

namespace AvaliMod
{
    [HarmonyPatch(typeof(ThingFilter), "SetFromPreset")]
    public static class StockpilePatches
    {
        [HarmonyPostfix]
        public static void Postfix(StorageSettingsPreset preset, ThingFilter __instance)
        {
            if (preset == StorageSettingsPreset.DefaultStockpile)
                __instance.SetAllow(AvaliMod.AvaliDefs.AvaliResources,true);
        }
    }
}
