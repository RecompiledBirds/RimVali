using System;
using HarmonyLib;
using Verse;

namespace AvaliMod
{
    [StaticConstructorOnStartup]
    public static class RimValiPatches
    {
        public static void Initalize()
        {
            var rimValiHarmony = new Harmony("RimVali.FarFromAvalon");
            Log.Message("[RimVali: Far From Avalon] Started patches.");
            try
            {
                rimValiHarmony.PatchAll();

                int methodsPatched = rimValiHarmony.GetPatchedMethods().EnumerableCount();
                Log.Message("[RimVali: Far From Avalon] Patches completed. " + methodsPatched + " methods patched.");
            }
            catch (Exception error)
            {
                Log.Warning("[RimVali: Far From Avalon] A patch has failed! Patches completed: " +
                            rimValiHarmony.GetPatchedMethods().EnumerableCount());
                Log.Error(error.ToString());
            }
        }
    }
}
