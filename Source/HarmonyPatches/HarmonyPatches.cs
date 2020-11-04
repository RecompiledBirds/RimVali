using HarmonyLib;
using Verse;
namespace AvaliMod
{
    [StaticConstructorOnStartup]
    public static class RimValiPatches
    {
        public static void RunPatch()
        {
            new Harmony("RimVali.patches").PatchAll();
        }
        static RimValiPatches()
        {
            Log.Message("Started patches. [RimVali]", false);
            RimValiPatches.RunPatch();
            Log.Message("Patches completed. [RimVali]");

        }

    }
}