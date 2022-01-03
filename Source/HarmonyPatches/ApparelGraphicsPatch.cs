using System.Linq;
using HarmonyLib;
using RimValiCore.RVR;
using RimWorld;
using UnityEngine;
using Verse;

namespace AvaliMod
{
    // //This patch helps with automatic resizing, and apparel graphics
    [HarmonyPatch(typeof(ApparelGraphicRecordGetter), nameof(ApparelGraphicRecordGetter.TryGetGraphicApparel))]
    public static class ApparelGraphicsPatch
    {
        [HarmonyPostfix]
        public static void Patch(ref Apparel apparel, ref BodyTypeDef bodyType, ref ApparelGraphicRecord rec)
        {
            Pawn pawn = apparel.Wearer;
            if (apparel.def.apparel.layers.Any(d => d == ApparelLayerDefOf.Overhead) &&
                apparel.def.apparel.wornGraphicPath != null)
            {
                if (bodyType != AvaliDefs.Avali && bodyType != AvaliDefs.Avali)
                {
                    return;
                }

                var path = $"{apparel.def.apparel.wornGraphicPath}_{bodyType.defName}";
                if (pawn.def is RimValiRaceDef def && ContentFinder<Texture2D>.Get($"{path}_north", false) != null &&
                    ContentFinder<Texture2D>.Get($"{path}_east", false) != null &&
                    ContentFinder<Texture2D>.Get($"{path}_south", false) != null)
                {
                    Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.Cutout,
                        apparel.def.graphicData.drawSize /
                        def.renderableDefs.First(x => x.defName.ToLower() == "head").south.size, apparel.DrawColor);
                    rec = new ApparelGraphicRecord(graphic, apparel);
                }
                else if (!(pawn.def is RimValiRaceDef))
                {
                    if (ContentFinder<Texture2D>.Get($"{path}_north", false) != null &&
                        ContentFinder<Texture2D>.Get($"{path}_east", false) != null &&
                        ContentFinder<Texture2D>.Get($"{path}_south", false) != null)
                    {
                        Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.Cutout,
                            apparel.def.graphicData.drawSize, apparel.DrawColor);
                        rec = new ApparelGraphicRecord(graphic, apparel);
                    }
                }
            }
            else if (!apparel.def.apparel.wornGraphicPath.NullOrEmpty())
            {
                var str = $"{apparel.def.apparel.wornGraphicPath}_{bodyType.defName}";
                if (ContentFinder<Texture2D>.Get($"{str}_north", false) == null ||
                    ContentFinder<Texture2D>.Get($"{str}_east", false) == null ||
                    ContentFinder<Texture2D>.Get($"{str}_south", false) == null)
                {
                    Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(apparel.def.apparel.wornGraphicPath,
                        ShaderDatabase.Cutout, apparel.def.graphicData.drawSize, apparel.DrawColor);
                    rec = new ApparelGraphicRecord(graphic, apparel);
                }
            }
        }
    }
}
