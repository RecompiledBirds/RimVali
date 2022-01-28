using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using RimValiCore.RVR;
using HarmonyLib;
namespace AvaliMod
{

    [HarmonyPatch(typeof(Thing),"TakeDamage")]
    public static class HoloVali_TakeDamagePatch
    {
        public static bool Prefix(Thing __instance, DamageInfo dinfo, ref DamageWorker.DamageResult __result)
        {
            if(__instance is Pawn p && p.story.AllBackstories.Any(x =>x.GetTags().Contains("Holovali")))
            {
                if (dinfo.Def == DamageDefOf.EMP)
                {
                    Thing drop = ThingMaker.MakeThing(ThingDefOf.ComponentIndustrial);
                    drop.stackCount = UnityEngine.Random.Range(1, 2);
                    IntVec3 loc = p.Position;
                    Map map = p.Map;
                    p.DeSpawn();
                    GenSpawn.Spawn(drop, loc,map);
                    p.Kill(dinfo);
                }
                __result = new DamageWorker.DamageResult();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Need_Rest),"NeedInterval")]
    public static class HoloVali_RestPatch
    {
        public static bool Prefix(Need_Rest __instance)
        {
            object pawn = __instance.GetType().GetField("pawn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(__instance);
            if (pawn is Pawn p && p.RaceProps.Humanlike && p.story.AllBackstories.Any(x => x.GetTags().Contains("Holovali")))
            {
                __instance.CurLevel = __instance.MaxLevel;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Need_Food), "NeedInterval")]
    public static class HoloVali_FoodPatch
    {
        public static bool Prefix(Need_Food __instance)
        {
            object pawn = __instance.GetType().GetField("pawn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(__instance);
            if (pawn is Pawn p && p.RaceProps.Humanlike && p.story.AllBackstories.Any(x => x.GetTags().Contains("Holovali")))
            {
                __instance.CurLevel = __instance.MaxLevel;
                return false;
            }
            return true;
        }
    }
}
