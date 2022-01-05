using RimWorld;
using HarmonyLib;
using Verse;
using UnityEngine;

namespace AvaliMod.HarmonyPatches
{
    [HarmonyPatch(typeof(Pawn), "TicksPerMove")]
    public static class TicksPerMove_Patch
    {
        public static void Prefix(bool diagonal, Pawn __instance, ref int __result)
        {
            float baseSpeed = __instance.GetStatValue(StatDefOf.MoveSpeed);
            if (RestraintsUtility.InRestraints(__instance))
            {
                baseSpeed *= 0.35f;
            }
			if (IsCarryingPawn(__instance))
			{
				baseSpeed *= 0.6f;
			}
			float speed = baseSpeed / 60f;
			float final;
			if (speed == 0f)
			{
				final = 450f;
			}
			else
			{
				final = 1f / speed;
				if (IsRoofed(__instance))
				{
					final /=GetWeatherMoveSpeed(__instance);
				}
				if (diagonal)
				{
					final *= 1.41421f;
				}
			}
			__result = Mathf.Clamp(Mathf.RoundToInt(final), 1, 450);
		}

		public static bool IsCarryingPawn(Pawn pawn)
        {
			return pawn.carryTracker != null && pawn.carryTracker.CarriedThing != null && pawn.carryTracker.CarriedThing.def.category == ThingCategory.Pawn;
		}

		public static bool IsRoofed(Pawn pawn)
        {
			return pawn.Spawned && !pawn.Map.roofGrid.Roofed(pawn.Position);

		}

		public static float GetWeatherMoveSpeed(Pawn pawn)
        {
			WeatherManager weatherManager = pawn.Map.weatherManager;
            if (pawn.def != AvaliDefs.RimVali)
            {
				return Mathf.Lerp(weatherManager.lastWeather.moveSpeedMultiplier, weatherManager.curWeather.moveSpeedMultiplier, weatherManager.TransitionLerpFactor);
            }
			return Mathf.Lerp(weatherManager.lastWeather.snowRate>0 ?1 : weatherManager.lastWeather.moveSpeedMultiplier, weatherManager.curWeather.snowRate > 0 ? 1 : weatherManager.curWeather.moveSpeedMultiplier, weatherManager.TransitionLerpFactor);        }
    }
}
