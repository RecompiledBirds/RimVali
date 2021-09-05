using HarmonyLib;
using System;
using Verse;

namespace AvaliMod
{
    [StaticConstructorOnStartup]
    public static class RimValiPatches
    {

        static RimValiPatches()
        {

            Harmony rimValiHarmony = new Harmony("RimVali.FarFromAvalon");
            Log.Message("[RimVali: Far From Avalon] Started patches.", false);
            try
            {

                rimValiHarmony.PatchAll();




                //Graphics
                /*rimValiHarmony.Patch(AccessTools.Method(typeof(PawnGraphicSet), "ResolveAllGraphics"), new HarmonyMethod(typeof(ResolvePatch), "ResolveGraphics"), null);
                rimValiHarmony.Patch(AccessTools.Method(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel"), null, new HarmonyMethod(typeof(Avali_ApparelGraphicRecordGetter_TryGetGraphicApparel_AvaliSpecificHat_Patch), "Avali_SpecificHatPatch"));
                rimValiHarmony.Patch(AccessTools.Method(typeof(PawnRenderer), "RenderPortrait"), null, new HarmonyMethod(typeof(RenderPatch), "Portrait"));*/

                int methodsPatched = rimValiHarmony.GetPatchedMethods().EnumerableCount();
                Log.Message("[RimVali: Far From Avalon] Patches completed. " + methodsPatched.ToString() + " methods patched.");

            }
            catch (Exception error)
            {
                Log.Warning("[RimVali: Far From Avalon] A patch has failed! Patches completed: " + rimValiHarmony.GetPatchedMethods().EnumerableCount().ToString());
                Log.Error(error.ToString());

            }


        }


    }
}