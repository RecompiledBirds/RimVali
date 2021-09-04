using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimValiCore.RVR;
using Verse;
using HarmonyLib;

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
