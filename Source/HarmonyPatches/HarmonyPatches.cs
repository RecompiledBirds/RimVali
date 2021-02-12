using HarmonyLib;
using System;
using Verse;
using RimWorld;
using UnityEngine;
using AlienRace;
using System.Collections.Generic;
using System.Linq;

namespace AvaliMod
{
    [StaticConstructorOnStartup]
    public static class RimValiPatches
    {
        
        static RimValiPatches()
        {
            
            Harmony rimValiHarmony = new Harmony("RimVali.patches");
            Log.Message("Started patches. [RimVali]", false);
            try {

                rimValiHarmony.PatchAll();
              

           

                //Graphics
                /*rimValiHarmony.Patch(AccessTools.Method(typeof(PawnGraphicSet), "ResolveAllGraphics"), new HarmonyMethod(typeof(ResolvePatch), "ResolveGraphics"), null);
                rimValiHarmony.Patch(AccessTools.Method(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel"), null, new HarmonyMethod(typeof(Avali_ApparelGraphicRecordGetter_TryGetGraphicApparel_AvaliSpecificHat_Patch), "Avali_SpecificHatPatch"));
                rimValiHarmony.Patch(AccessTools.Method(typeof(PawnRenderer), "RenderPortrait"), null, new HarmonyMethod(typeof(RenderPatch), "Portrait"));*/
                
                int methodsPatched = rimValiHarmony.GetPatchedMethods().EnumerableCount();
                Log.Message("Patches completed. " + methodsPatched.ToString() + " methods patched. [RimVali]");
                
            }
            catch (Exception error)
            {
                Log.Message("A patch has failed! Patches completed: " + rimValiHarmony.GetPatchedMethods().EnumerableCount().ToString());
                Log.Message(error.ToString());

            }
            

        }


    }
}