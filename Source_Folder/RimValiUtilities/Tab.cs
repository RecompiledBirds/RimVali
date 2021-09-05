using RimWorld;
using System;
using System.Text;
using UnityEngine;
using Verse;


namespace AvaliMod
{
    [StaticConstructorOnStartup]
    public class UIResources
    {
        public static readonly Texture2D NegativePackOP = SolidColorMaterials.NewSolidColorTexture(new Color(247, 0, 0));
        public static readonly Texture2D Rename = ContentFinder<Texture2D>.Get("UI/Buttons/Rename", true);
    }

    public class AvaliTab : ITab
    {
        private readonly int maxSize = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().maxPackSize;
        private readonly bool packsEnabled = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packsEnabled;
        private Vector2 membersScrollPos = new Vector2();
        public Vector2 bonusScrollPos = new Vector2();
        public virtual AvaliPack GetPack(Pawn pawn)
        {
            AvaliPack pack = null;
            pack = pawn.GetPack();
            return pack;
        }
        public virtual string GetPackName(Rect rect, AvaliPack pack)
        {
            return pack.name;
        }

        private readonly float heightOffset = 12f;
        private readonly float packMembersRectSize = 300f;
        public Vector2 WinSize = new Vector2(700, 400);
        protected override void FillTab()
        {
            bool debugSquares = RimValiMod.settings.enableDebugMode;



            Text.Font = GameFont.Small;
            Rect rect = new Rect(0f, 20f, size.x, size.y - 20f).ContractedBy(10f);

            Rect position = new Rect(rect.x, rect.y, rect.width, rect.height);
            GUI.BeginGroup(position);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, 0f, position.width, position.height);
            Rect viewRect = new Rect(0f, 0f, position.width - 16f, scrollViewHeight);



            //COMPILED BY NESGUI
            //Prepare varibles

            GameFont prevFont = Text.Font;
            TextAnchor textAnchor = Text.Anchor;

            //Rect pass

            Rect packNameRect = new Rect(new Vector2(5f, 5f), new Vector2(600f, 30f));
            Rect changeNameRect = new Rect(new Vector2(605f, 5f), new Vector2(30f, 30f));
            Rect countRect = new Rect(new Vector2(635f, 5f), new Vector2(55f, 30f));
            Rect packLeaderLabelRect = new Rect(new Vector2(5f, 45f), new Vector2(90f, 25f));
            Rect leaderNameRect = new Rect(new Vector2(95f, 45f), new Vector2(110f, 25f));
            Rect pawnListRect = new Rect(new Vector2(5f, 75f), new Vector2(200f, 310f));
            Rect pawnLikedLabelRect = new Rect(new Vector2(205f, 45f), new Vector2(125f, 25f));
            Rect pawnLikedFillRect = new Rect(new Vector2(340f, 45f), new Vector2(350f, 25f));
            Rect packTypeRect = new Rect(new Vector2(205f, 75f), new Vector2(240f, 310f));
            Rect packRelationsRect = new Rect(new Vector2(445f, 75f), new Vector2(245f, 310f));

            //END NESGUI CODE


            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect, true);
            try
            {
                {
                    Pawn pawn = SelPawn;
                    AvaliPack pack = pawn.GetPack();
                    float rectPosY = 0f;

                    if (pack != null && pack.GetAllNonNullPawns.Count > 1)
                    {
                        string effects = "";
                        string packSpecialityName = "NONE";
                        int listCount = 0;
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
                        Text.Font = GameFont.Medium;
                        Rect PackNameRect = new Rect(outRect.xMin, rectPosY, packMembersRectSize, 30f);
                        Rect RenameButtonRect = new Rect(outRect.xMax - 30f, rectPosY, 30f, 30f);
                        Widgets.DrawLineHorizontal(0f, (PackNameRect.yMax + 10f), rect.width);
                        Text.Font = GameFont.Small;
                        rectPosY = PackNameRect.yMax + 10f;
                        rectPosY += 20f;


                        Rect packMemberListViewRect = new Rect(pawnListRect.x - 5, pawnListRect.y - 10, pawnListRect.width - 5, pack.GetAllNonNullPawns.Count * 30f);

                        string packcount = pack.GetAllNonNullPawns.Count.ToString() + "/" + maxSize.ToString();


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
                            Widgets.BeginScrollView(pawnListRect, ref membersScrollPos, packMemberListViewRect, true);
                            {
                                foreach (Pawn p in pack.GetAllNonNullPawns)
                                {
                                    if (p != pack.leaderPawn)
                                    {
                                        float rowHeight = 30f;
                                        if (num > y - rowHeight && num < num2) { DrawMemberRow(num, pawnListRect.width, p); }
                                        num += rowHeight;
                                    }
                                }
                            }
                            Widgets.EndScrollView();

                            Widgets.Label(packNameRect, GetPackName(rect, pack));
                            if (Widgets.ButtonImage(changeNameRect, UIResources.Rename, true)) { Find.WindowStack.Add(new Dialog_NamePack(pawn)); }

                            Widgets.Label(packTypeRect, new GUIContent { text = $"{"PackSpeciality".Translate(packSpecialityName.Named("SPECIALITY"))} \n\n{"PackEffects".Translate()} \n{effects}" });


                            float opinion = RimValiUtility.GetPackAvgOP(pack, pawn);
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
                            StringBuilder builder = new StringBuilder();
                            builder.AppendLine("PackOpinionTooltip".Translate(opinion.Named("OPINION"), name.Named("PAWN")));
                            builder.AppendLine();
                            foreach (Pawn p in pack.GetAllNonNullPawns)
                            {
                                if (p != pawn)
                                {
                                    builder.AppendLine($"{p.Name.ToStringShort}: {p.relations.OpinionOf(pawn)}");
                                }
                            }
                            TooltipHandler.TipRegion(pawnLikedFillRect, builder.ToString());
                            builder.Clear();

                            Widgets.Label(packLeaderLabelRect, "PackLeaderLabel".Translate());
                            Widgets.Label(leaderNameRect, pack.leaderPawn.Name.ToStringShort);
                            if (Widgets.ButtonInvisible(leaderNameRect, true) && Current.ProgramState == ProgramState.Playing)
                            {
                                if (pack.leaderPawn.Dead) { Messages.Message("MessageCantSelectDeadPawn".Translate(pack.leaderPawn.LabelShort, pack.leaderPawn).CapitalizeFirst(), MessageTypeDefOf.RejectInput, false); }
                                else if (pack.leaderPawn.Spawned) { CameraJumper.TryJumpAndSelect(pack.leaderPawn); }
                                else { Messages.Message("MessageCantSelectOffMapPawn".Translate(pack.leaderPawn.LabelShort, pack.leaderPawn).CapitalizeFirst(), MessageTypeDefOf.RejectInput, false); }
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
                        Widgets.Label(rect, "NoPack".Translate());
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                Widgets.Label(rect, $"Hey! If you are seeing this, something probably went wrong somewhere... sorry about that :( . I've logged the error: {e.Message}");
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private static void DrawMemberRow(float y, float width, Pawn packmember)
        {
            float rowHeight = 30f;
            Rect rect = new Rect(0f, y, width, rowHeight);
            Pawn otherPawn = packmember;
            if (Widgets.ButtonInvisible(rect, true))
            {
                if (Current.ProgramState == ProgramState.Playing)
                {
                    if (otherPawn.Dead) { Messages.Message("MessageCantSelectDeadPawn".Translate(otherPawn.LabelShort, otherPawn).CapitalizeFirst(), MessageTypeDefOf.RejectInput, false); }
                    else if (otherPawn.Spawned) { CameraJumper.TryJumpAndSelect(otherPawn); }
                    else { Messages.Message("MessageCantSelectOffMapPawn".Translate(otherPawn.LabelShort, otherPawn).CapitalizeFirst(), MessageTypeDefOf.RejectInput, false); }
                }
            }
            Rect rect4 = new Rect(0f, y + 3f, 500f, rowHeight - 3f);
            Widgets.Label(rect4, otherPawn.Name.ToStringFull);
        }

        public AvaliTab()
        {
            size = WinSize;
            labelKey = "PackTab";
        }

        // Token: 0x17000007 RID: 7
        // (get) Token: 0x06000034 RID: 52 RVA: 0x00004078 File Offset: 0x00003078
        public override bool IsVisible => packsEnabled;
        private Vector2 scrollPosition = Vector2.zero;

        // Token: 0x04006154 RID: 24916
        private readonly float scrollViewHeight;

    }

}