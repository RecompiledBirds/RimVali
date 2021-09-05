using HarmonyLib;
using RimValiCore.RVR;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AvaliMod
{
    #region Hat patch
    // //This patch helps with automatic resizing, and apparel graphics
    [HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
    public static class Avali_ApparelGraphicRecordGetter_TryGetGraphicApparel_AvaliSpecificHat_Patch
    {
        [HarmonyPostfix]
        public static void Patch(ref Apparel apparel, ref BodyTypeDef bodyType, ref ApparelGraphicRecord rec)
        {
            Pawn pawn = apparel.Wearer;
            if (apparel.def.apparel.layers.Any(d => d == ApparelLayerDefOf.Overhead) && apparel.def.apparel.wornGraphicPath != null)
            {
                if (bodyType != AvaliDefs.Avali && bodyType != AvaliDefs.Avali)
                {
                    return;
                }

                string path = $"{apparel.def.apparel.wornGraphicPath}_{bodyType.defName}";
                if (pawn.def is RimValiRaceDef def && (ContentFinder<Texture2D>.Get($"{path}_north", false) != null) && (ContentFinder<Texture2D>.Get($"{path}_east", false) != null) && (ContentFinder<Texture2D>.Get($"{path}_south", false) != null))
                {

                    Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.Cutout, apparel.def.graphicData.drawSize / def.renderableDefs.First(x => x.defName.ToLower() == "head").south.size, apparel.DrawColor);
                    rec = new ApparelGraphicRecord(graphic, apparel);
                }
                else if (!(pawn.def is RimValiRaceDef))
                {

                    if ((ContentFinder<Texture2D>.Get($"{path}_north", false) != null) && (ContentFinder<Texture2D>.Get($"{path}_east", false) != null) && (ContentFinder<Texture2D>.Get($"{path}_south", false) != null))
                    {
                        Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.Cutout, apparel.def.graphicData.drawSize, apparel.DrawColor);
                        rec = new ApparelGraphicRecord(graphic, apparel);
                    }
                }
            }
            else if (!apparel.def.apparel.wornGraphicPath.NullOrEmpty())
            {
                string str = $"{apparel.def.apparel.wornGraphicPath}_{bodyType.defName}";
                if (ContentFinder<Texture2D>.Get($"{str}_north", false) == null || ContentFinder<Texture2D>.Get($"{str}_east", false) == null || ContentFinder<Texture2D>.Get($"{str}_south", false) == null)
                {
                    Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(apparel.def.apparel.wornGraphicPath, ShaderDatabase.Cutout, apparel.def.graphicData.drawSize, apparel.DrawColor);
                    rec = new ApparelGraphicRecord(graphic, apparel);
                }
            }
        }
    }
    #endregion
    public class CreditDef : Def
    {
        public string title;
        public List<string> names = new List<string>();
    }
    [HarmonyPatch(typeof(CreditsAssembler), "AllCredits")]
    public class Credits
    {
        [HarmonyPostfix]
        public static IEnumerable<CreditsEntry> CreditsPatch(IEnumerable<CreditsEntry> __result)
        {
            foreach (CreditsEntry creditsEntry in __result)
            {
                yield return creditsEntry;
                CreditRecord_Role creditRecord_Role = creditsEntry as CreditRecord_Role;
                if (creditRecord_Role != null && creditRecord_Role.creditee == "Many other gracious volunteers!" /*Thanks for playing!*/)
                {

                    yield return new CreditRecord_Space(150f);
                    yield return new CreditRecord_Title("RimVali would like to thank several people:");
                    foreach (CreditDef def in DefDatabase<CreditDef>.AllDefs)
                    {
                        yield return new CreditRecord_Space(150f);
                        yield return new CreditRecord_Title(def.title);
                        foreach (string name in def.names)
                        {
                            yield return new CreditRecord_Role(null, name);
                        }
                    }

                }

            }
        }
    }
}
