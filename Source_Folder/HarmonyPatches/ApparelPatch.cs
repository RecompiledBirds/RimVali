using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AvaliMod
{
    [HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
    public static class Avali_ApparelGraphicRecordGetter_TryGetGraphicApparel_AvaliSpecificHat_Patch
    {
        [HarmonyPostfix]
        public static void Avali_SpecificHatPatch(
          ref Apparel apparel,
          ref BodyTypeDef bodyType,
          ref ApparelGraphicRecord rec)
        {
            if (bodyType != AvaliMod.AvaliDefs.Avali && bodyType != AvaliMod.AvaliDefs.Avali)
                return;
            if (apparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead)
            {
                string path = apparel.def.apparel.wornGraphicPath + "_" + bodyType.defName;
                if (!((Object)ContentFinder<Texture2D>.Get(path + "_north", false) == (Object)null) && !((Object)ContentFinder<Texture2D>.Get(path + "_east", false) == (Object)null) && !((Object)ContentFinder<Texture2D>.Get(path + "_south", false) == (Object)null))
                {
                    AvaliGraphic graphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(path, AvaliShaderDatabase.Tricolor, apparel.def.graphicData.drawSize, apparel.DrawColor);
                    rec = new ApparelGraphicRecord(graphic, apparel);
                }
            }
            else if (!apparel.def.apparel.wornGraphicPath.NullOrEmpty())
            {
                string str = apparel.def.apparel.wornGraphicPath + "_" + bodyType.defName;
                if ((Object)ContentFinder<Texture2D>.Get(str + "_north", false) == (Object)null || (Object)ContentFinder<Texture2D>.Get(str + "_east", false) == (Object)null || (Object)ContentFinder<Texture2D>.Get(str + "_south", false) == (Object)null)
                {
                    AvaliGraphic graphic = AvaliGraphicDatabase.Get<AvaliGraphic_Multi>(apparel.def.apparel.wornGraphicPath, AvaliShaderDatabase.Tricolor, apparel.def.graphicData.drawSize, apparel.DrawColor);
                    rec = new ApparelGraphicRecord(graphic, apparel);
                }
            }
        }
    }
}
