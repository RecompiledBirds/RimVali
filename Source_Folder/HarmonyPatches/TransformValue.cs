using HarmonyLib;
using Verse;
using RimWorld;
using System;

namespace AvaliMod
{
    [HarmonyPatch(typeof(StatPart_WorkTableTemperature), "TransformValue", new Type[] { typeof(StatRequest), typeof(float) })]
    public static class StatPart_WorkTableTemperature_TransformValue
    {
        public static bool Prefix(StatRequest req, ref float value)
        {

            if (!(req.Pawn.GetComp<TempComp>() == null))
            {
                value *= 1f; // no penalty 
                return false;
            }
            else
            {
                return true; // use original penalty
            }
        }
    }
}