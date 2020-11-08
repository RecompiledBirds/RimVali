using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace AvaliMod
{
	// Token: 0x0200000D RID: 13
	public class ITab_Avali_Pack : ITab
	{
		// Token: 0x06000033 RID: 51 RVA: 0x00004039 File Offset: 0x00003039
		public ITab_Avali_Pack()
		{
			this.size = this.WinSize;
			this.labelKey = "TabAvaliPack";
			this.oldColor = GUI.color;
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000034 RID: 52 RVA: 0x00004078 File Offset: 0x00003078
		public override bool IsVisible
		{
			get
			{
				return base.SelPawn.def == AvaliDefs.RimVali;
			}
		}

		// Token: 0x06000035 RID: 53 RVA: 0x0000408C File Offset: 0x0000308C
		protected override void FillTab()
		{
			Rect rect = new Rect(0f, 0f, this.WinSize.x, this.WinSize.y).ContractedBy(10f);
			Text.Font = GameFont.Small;
			this.oldColor = GUI.color;
			AvaliPack pawnPack = null;
			List<AvaliPack> packs = AvaliPackDriver.packs;
			Pawn pawn = this.SelPawn;
		
			foreach(AvaliPack pack in packs)
            {
                if (pack.pawns.Contains(pawn))
                {
					pawnPack = pack;
					break;
                }
            }
			if(pawnPack == null)
            {
				this.NotInPack(rect);
				return;
            }
			List<Pawn> list = pawnPack.pawns;
			int num = pawnPack.size;
			if ( num < 2 || list == null)
			{
				this.NotInPack(rect);
				return;
			}
			if (pawn == null)
			{
				pawn = base.SelPawn;
			}
			Rect rect2 = new Rect(0f, 20f, this.WinSize.x, 42f).ContractedBy(10f);
			this.DrawLineHorizontalWithColor(10f, 52f, this.WinSize.x - 20f, Color.gray);
			Rect rect3 = new Rect(0f, 44f, this.WinSize.x, 42f).ContractedBy(10f);
			ITab_Avali_Pack.NewRect(rect3, "PackLeader".Translate(), pawn.Name.ToString(), rect3, "PackLeaderDesc".Translate());
			Rect rect4 = new Rect(0f, 68f, this.WinSize.x, 42f).ContractedBy(10f);
			string text = base.SelPawn.Name.ToString();
			int num2 = 1;
			for (int j = 0; j < list.Count; j++)
			{
				Pawn pawn3 = list[j];
				if (pawn3 != base.SelPawn)
				{
					text = text + "\n" + pawn3.Name;
					num2++;
				}
			}
			ITab_Avali_Pack.NewRect(rect4, "PackMembers".Translate(), num2 + "/" + num, rect4, text);
			if (base.SelPawn.IsColonist || Prefs.DevMode)
			{
				this.DrawLineHorizontalWithColor(10f, 148f, this.WinSize.x - 20f, Color.gray);
				Rect rect5 = new Rect(0f, 116f, this.WinSize.x, 42f).ContractedBy(10f);
				float num3 = 30f * base.SelPawn.health.capacities.GetLevel(PawnCapacityDefOf.Hearing);
				ITab_Avali_Pack.NewRect(rect5, "PackEffects".Translate(), "", rect5, string.Format("PackEffectsDesc".Translate(), (int)num3));
				float num4 = 140f;
				num4 = 140f;
			}
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00004651 File Offset: 0x00003651
		public void DrawLineHorizontalWithColor(float x, float y, float length, Color color)
		{
			GUI.color = color;
			Widgets.DrawLineHorizontal(x, y, length);
			GUI.color = this.oldColor;
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00004670 File Offset: 0x00003670
		public static void NewRect(Rect rect, string label, string rectLabel, Rect rectLabelRect, string tooltip = null)
		{
			Text.Font = GameFont.Small;
			Widgets.Label(rect, label);
			if (rectLabel != null)
			{
				if (rect == rectLabelRect)
				{
					rectLabelRect = new Rect(rect.center.x, rect.y, rect.width, rect.height);
				}
				Widgets.Label(rectLabelRect, rectLabel);
			}
			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);
			}
			if (tooltip != null)
			{
				TooltipHandler.TipRegion(rect, new TipSignal(tooltip, rect.GetHashCode()));
			}
		}

		// Token: 0x06000038 RID: 56 RVA: 0x000046F4 File Offset: 0x000036F4

		// Token: 0x06000039 RID: 57 RVA: 0x00004740 File Offset: 0x00003740
		public void NotInPack(Rect rect)
		{
			string text = string.Format("NotInPack".Translate(), base.SelPawn);
			Text.Font = GameFont.Medium;
			GUI.color = Color.gray;
			rect = new Rect(rect.x + this.WinSize.x / 4f - (float)text.Length, rect.y + this.WinSize.y / 2f - (float)text.Length, rect.width, rect.height);
			Widgets.Label(rect, text);
		}

		// Token: 0x04000017 RID: 23
		public const float startPosY = 20f;

		// Token: 0x04000018 RID: 24
		public const float labelSizeY = 24f;

		// Token: 0x04000019 RID: 25
		public Vector2 WinSize = new Vector2(630f, 510f);


		// Token: 0x0400001B RID: 27
		private Color oldColor;
	}
}
