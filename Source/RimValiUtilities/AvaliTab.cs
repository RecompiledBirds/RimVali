using System;
using System.Text;
using Rimvali.Rewrite.Packs;
using RimWorld;
using UnityEngine;
using Verse;

namespace AvaliMod
{
    [StaticConstructorOnStartup]
    public class UIResources
    {
        public static readonly Texture2D
            NegativePackOP = SolidColorMaterials.NewSolidColorTexture(new Color(247, 0, 0));

        public static readonly Texture2D Rename = ContentFinder<Texture2D>.Get("UI/Buttons/Rename");
    }

    public class AvaliTab : ITab
    {
        private const float packMembersRectSize = 300f;

        private const float scrollViewHeight = 0.0f;

        private readonly int maxSize =
            LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().maxPackSize;

        public Vector2 bonusScrollPos = new Vector2();
        private Vector2 membersScrollPos;
        private Vector2 scrollPosition = Vector2.zero;
        public Vector2 WinSize = new Vector2(700, 400);

        public AvaliTab()
        {
            size = WinSize;
            labelKey = "PackTab";
        }

        public override bool IsVisible { get; } =
            LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packsEnabled;

        public virtual string GetPackName(Rect rect, Pack pack)
        {
            return pack.Name;
        }

       

        protected override void FillTab()
        {
            bool debugSquares = RimValiMod.settings.enableDebugMode;


            Text.Font = GameFont.Small;
            Rect rect = new Rect(0f, 20f, size.x, size.y - 20f).ContractedBy(10f);

            var position = new Rect(rect.x, rect.y, rect.width, rect.height);
            GUI.BeginGroup(position);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            var outRect = new Rect(0f, 0f, position.width, position.height);
            var viewRect = new Rect(0f, 0f, position.width - 16f, scrollViewHeight);


            //COMPILED BY NESGUI
            //Rect pass

            var packNameRect = new Rect(new Vector2(5f, 5f), new Vector2(600f, 30f));
            var changeNameRect = new Rect(new Vector2(605f, 5f), new Vector2(30f, 30f));
            var countRect = new Rect(new Vector2(635f, 5f), new Vector2(55f, 30f));
            var packLeaderLabelRect = new Rect(new Vector2(5f, 45f), new Vector2(90f, 25f));
            var leaderNameRect = new Rect(new Vector2(95f, 45f), new Vector2(110f, 25f));
            var pawnListRect = new Rect(new Vector2(5f, 75f), new Vector2(200f, 310f));
            var pawnLikedLabelRect = new Rect(new Vector2(205f, 45f), new Vector2(125f, 25f));
            var pawnLikedFillRect = new Rect(new Vector2(340f, 45f), new Vector2(350f, 25f));
            var packTypeRect = new Rect(new Vector2(205f, 75f), new Vector2(240f, 310f));
            var packRelationsRect = new Rect(new Vector2(445f, 75f), new Vector2(245f, 310f));

            //END NESGUI CODE


            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            try
            {
                PacksV2WorldComponent packsComp = Find.World.GetComponent<PacksV2WorldComponent>();
                Pawn pawn = SelPawn;
                Pack pack = packsComp.GetPack(pawn);

                var rectPosY = 0f;

                if (pack != null && pack.GetPawns.Count > 0)
                {
                    var effects = "";
                    var packSpecialityName = "NONE";
                    var listCount = 0;
                    /*
                    if (pack.GetPackSkillDef() != null)
                    {
                        AvaliPackSkillDef skillDef = pack.GetPackSkillDef();

                        if (skillDef != null)
                        {
                            listCount = skillDef.effectList.Count;
                            packSpecialityName = skillDef.specialityLabel;
                            foreach (string str in skillDef.effectList)
                            {
                                effects = $"{effects} {str}\n";
                            }
                        }
                    }
                    */
                    Text.Font = GameFont.Medium;
                    var PackNameRect = new Rect(outRect.xMin, rectPosY, packMembersRectSize, 30f);
                    new Rect(outRect.xMax - 30f, rectPosY, 30f, 30f);
                    Widgets.DrawLineHorizontal(0f, PackNameRect.yMax + 10f, rect.width);
                    Text.Font = GameFont.Small;
                    rectPosY = PackNameRect.yMax + 10f;
                    rectPosY += 20f;


                    var packMemberListViewRect = new Rect(pawnListRect.x - 5, pawnListRect.y - 10,
                        pawnListRect.width - 5, pack.GetPawns.Count * 30f);

                    string packcount = pack.GetAllPawns.Count + "/" + maxSize;


                    float num = rectPosY;
                    float y = membersScrollPos.y;
                    float num2 = membersScrollPos.y + pawnListRect.height;

                    void drawLabels()
                    {
                        //Count
                        Widgets.Label(countRect, packcount);
                        num = rectPosY;
                        y = membersScrollPos.y;
                        num2 = membersScrollPos.y + pawnListRect.height;
                        Widgets.BeginScrollView(pawnListRect, ref membersScrollPos, packMemberListViewRect);
                        {
                            foreach (Pawn p in pack.GetPawns)
                            {

                                var rowHeight = 30f;
                                if (num > y - rowHeight && num < num2)
                                {
                                    DrawMemberRow(num, pawnListRect.width, p);
                                }

                                num += rowHeight;
                            }
                        }
                        Widgets.EndScrollView();

                        Widgets.Label(packNameRect, GetPackName(rect, pack));
                        if (Widgets.ButtonImage(changeNameRect, UIResources.Rename))
                        {
                            Find.WindowStack.Add(new Dialog_NamePack(pawn));
                        }

                        Widgets.Label(packTypeRect,
                            new GUIContent
                            {
                                text =
                                    $"{"PackSpeciality".Translate(packSpecialityName.Named("SPECIALITY"))} \n\n{"PackEffects".Translate()} \n{effects}",
                            });


                        float opinion = pack.GetAvgOpinionOf(pawn);
                        if (opinion > 0)
                        {
                            Widgets.FillableBar(pawnLikedFillRect, opinion / 100);
                        }
                        else
                        {
                            Widgets.FillableBar(pawnLikedFillRect, -opinion / 100, UIResources.NegativePackOP);
                        }

                        string name = pawn.Name.ToStringShort;
                        Widgets.DrawHighlightIfMouseover(pawnLikedFillRect);
                        Widgets.Label(pawnLikedLabelRect, "PackOpinionLabel".Translate());
                        var builder = new StringBuilder();
                        builder.AppendLine("PackOpinionTooltip".Translate(opinion.Named("OPINION"),
                            name.Named("PAWN")));
                        builder.AppendLine();
                        foreach (Pawn p in pack.GetAllPawns)
                        {
                            if (p != pawn)
                            {
                                builder.AppendLine($"{p.Name.ToStringShort}: {p.relations.OpinionOf(pawn)}");
                            }
                        }

                        TooltipHandler.TipRegion(pawnLikedFillRect, builder.ToString());
                        builder.Clear();


                        Widgets.Label(packLeaderLabelRect, "PackLeaderLabel".Translate());
                        Widgets.Label(leaderNameRect, pack.Leader.Name.ToStringShort);
                        if (Widgets.ButtonInvisible(leaderNameRect) && Current.ProgramState == ProgramState.Playing)
                        {
                            if (pack.Leader.Dead)
                            {
                                Messages.Message(
                                    "MessageCantSelectDeadPawn"
                                        .Translate(pack.Leader.LabelShort, pack.Leader).CapitalizeFirst(),
                                    MessageTypeDefOf.RejectInput, false);
                            }
                            else if (pack.Leader.Spawned)
                            {
                                CameraJumper.TryJumpAndSelect(pack.Leader);
                            }
                            else
                            {
                                Messages.Message(
                                    "MessageCantSelectOffMapPawn"
                                        .Translate(pack.Leader.LabelShort, pack.Leader).CapitalizeFirst(),
                                    MessageTypeDefOf.RejectInput, false);
                            }
                        }
                    }


                    //Makes it easier to see the GUI layout
                    if (debugSquares)
                    {
                        Widgets.DrawBoxSolid(pawnListRect, Color.red);
                        Widgets.DrawBoxSolid(pawnLikedLabelRect, Color.blue);
                        Widgets.DrawBoxSolid(packNameRect, Color.magenta);
                        Widgets.DrawBoxSolid(packTypeRect, Color.green);
                        Widgets.DrawBoxSolid(countRect, Color.cyan);
                        Widgets.DrawBoxSolid(pawnLikedFillRect, Color.black);
                        Widgets.DrawBoxSolid(packRelationsRect, Color.grey);
                        Widgets.DrawBoxSolid(leaderNameRect, Color.yellow);
                        Widgets.DrawBoxSolid(changeNameRect, Color.red);
                        Widgets.DrawBoxSolid(packLeaderLabelRect, Color.blue);
                    }

                    drawLabels();
                }
                else
                {
                    Widgets.Label(rect, pawn.IsPackBroken() ? "PackBroken".Translate(pawn.Name.ToStringShort.Named("PAWN")) : "NoPack".Translate());
                }

            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                Widgets.Label(rect,
                    $"Hey! If you are seeing this, something probably went wrong somewhere... sorry about that :( . I've logged the error: {e.Message}");
            }

            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private static void DrawMemberRow(float y, float width, Pawn packmember)
        {
            var rowHeight = 30f;
            var rect = new Rect(0f, y, width, rowHeight);
            Pawn otherPawn = packmember;
            if (Widgets.ButtonInvisible(rect))
            {
                if (Current.ProgramState == ProgramState.Playing)
                {
                    if (otherPawn.Dead)
                    {
                        Messages.Message(
                            "MessageCantSelectDeadPawn".Translate(otherPawn.LabelShort, otherPawn).CapitalizeFirst(),
                            MessageTypeDefOf.RejectInput, false);
                    }
                    else if (otherPawn.Spawned)
                    {
                        CameraJumper.TryJumpAndSelect(otherPawn);
                    }
                    else
                    {
                        Messages.Message(
                            "MessageCantSelectOffMapPawn".Translate(otherPawn.LabelShort, otherPawn).CapitalizeFirst(),
                            MessageTypeDefOf.RejectInput, false);
                    }
                }
            }

            var rect4 = new Rect(0f, y + 3f, 500f, rowHeight - 3f);
            Widgets.Label(rect4, otherPawn.Name.ToStringFull);
        }
    }
}
