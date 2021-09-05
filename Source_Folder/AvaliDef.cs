using HarmonyLib;
using RimValiCore.RVR;
using Verse;

namespace AvaliMod
{
    [HarmonyPatch(typeof(RimValiRaceDef), "PostLoad")]
    public static class LoadPatch
    {
        [HarmonyPostfix]
        public static void Patch()
        {
            Log.Message("test load message");
        }
    }
}
