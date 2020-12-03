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
            
            Harmony harmony = new Harmony("RimVali.patches");
            Log.Message("Started patches. [RimVali]", false);
            try { harmony.PatchAll();
                Log.Message("Patches completed. " + harmony.GetPatchedMethods().EnumerableCount().ToString() + " methods patched. [RimVali]");
            }
            catch (Exception error)
            {
                Log.Message("A patch has failed! Patches completed: " + harmony.GetPatchedMethods().EnumerableCount().ToString());
                Log.Message(error.ToString());
            }
            

        }

    }
}