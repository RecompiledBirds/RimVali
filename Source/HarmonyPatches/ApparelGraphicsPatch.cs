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
            string bodyTypePath = $"{apparel.def.apparel.wornGraphicPath}_{bodyType.defName}";
            string typeLessPath = $"{apparel.def.apparel.wornGraphicPath}";
            if (apparel.def.apparel.layers.Any(d => d == ApparelLayerDefOf.Overhead) &&
                apparel.def.apparel.wornGraphicPath != null)
            {
                if (bodyType != AvaliDefs.Avali && bodyType != AvaliDefs.Avali)
                {
                    return;
                }
                
                if (pawn.def is RimValiRaceDef def)
                {
                    if (ContentFinder<Texture2D>.Get($"{bodyTypePath}_north", false) != null)
                    {
                        Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(bodyTypePath, ShaderDatabase.Cutout,
                        (apparel.def.graphicData.drawSize /
                        def.renderableDefs.First(x => x.defName.ToLower() == "head").south.size) * 0.02f, apparel.DrawColor);
                        rec = new ApparelGraphicRecord(graphic, apparel);
                    }
                    else if (ContentFinder<Texture2D>.Get($"{typeLessPath}_north", false) != null)
                    {
                        Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(typeLessPath, ShaderDatabase.Cutout,
                        (apparel.def.graphicData.drawSize / def.renderableDefs.First(x => x.defName.ToLower() == "head").south.size) * 0.02f, apparel.DrawColor);
                        rec = new ApparelGraphicRecord(graphic, apparel);
                    }
                }
                else if (!(pawn.def is RimValiRaceDef))
                {
                    if (ContentFinder<Texture2D>.Get($"{bodyTypePath}_north", false) != null &&
                        ContentFinder<Texture2D>.Get($"{bodyTypePath}_east", false) != null &&
                        ContentFinder<Texture2D>.Get($"{bodyTypePath}_south", false) != null)
                    {
                        Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(bodyTypePath, ShaderDatabase.Cutout,
                            apparel.def.graphicData.drawSize, apparel.DrawColor);
                        rec = new ApparelGraphicRecord(graphic, apparel);
                    }
                }
            }
            else if (!apparel.def.apparel.wornGraphicPath.NullOrEmpty())
            {
                if (pawn.def is RimValiRaceDef def)
                {
                    if (ContentFinder<Texture2D>.Get($"{bodyTypePath}_north", false) != null)
                    {
                        Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(bodyTypePath, ShaderDatabase.Cutout,
                        (apparel.def.graphicData.drawSize /
                        def.renderableDefs.First(x => x.defName.ToLower() == "head").south.size) * 0.02f, apparel.DrawColor);
                        rec = new ApparelGraphicRecord(graphic, apparel);
                    }
                    else if (ContentFinder<Texture2D>.Get($"{typeLessPath}_north", false) != null)
                    {
                        Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(typeLessPath, ShaderDatabase.Cutout,
                        (apparel.def.graphicData.drawSize / def.renderableDefs.First(x => x.defName.ToLower() == "head").south.size) * 0.02f, apparel.DrawColor);
                        rec = new ApparelGraphicRecord(graphic, apparel);
                    }
                }
                else
                {
                    if (ContentFinder<Texture2D>.Get($"{bodyTypePath}_north", false) != null &&
                        ContentFinder<Texture2D>.Get($"{bodyTypePath}_east", false) != null &&
                        ContentFinder<Texture2D>.Get($"{bodyTypePath}_south", false) != null)
                    {
                        Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(bodyTypePath, ShaderDatabase.Cutout,
                            apparel.def.graphicData.drawSize, apparel.DrawColor);
                        rec = new ApparelGraphicRecord(graphic, apparel);
                    }
                }
            }
        }
    }
}
