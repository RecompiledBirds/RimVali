using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using RimWorld;
using UnityEngine;
using Verse;


namespace AvaliMod
{
	[StaticConstructorOnStartup]
	public class UIResources
	{
		public static readonly Texture2D Rename = ContentFinder<Texture2D>.Get("UI/Buttons/Rename", true);
	}

	public class AvaliTab : ITab
	{
		private readonly int maxSize = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().maxPackSize;
		private readonly bool packsEnabled = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packsEnabled;
		private Vector2 membersScrollPos = new Vector2();
		public virtual AvaliPack GetPack(Pawn pawn)
		{
			AvaliPack pack = null;
			pack = RimValiUtility.GetPack(pawn);
			return pack;
		}
		public virtual string GetPackName(Rect rect, AvaliPack pack)
		{
			return pack.name;
		}
		public Vector2 WinSize = new Vector2(630f, 510f);
		protected override void FillTab()
		{
			Text.Font = GameFont.Small;
			Rect rect = new Rect(0f, 20f, this.size.x, this.size.y - 20f).ContractedBy(10f);

			Rect position = new Rect(rect.x, rect.y, rect.width, rect.height);
			GUI.BeginGroup(position);
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			Rect outRect = new Rect(0f, 0f, position.width, position.height);
			Rect viewRect = new Rect(0f, 0f, position.width - 16f, this.scrollViewHeight);
			Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);
			Pawn pawn = SelPawn;
			AvaliPack pack = null;
			float rectPosY = 0f;

			pack = this.GetPack(pawn);

			if (!(pack == null) && pack.pawns.Count > 1)
			{
				string members = "Packmates".Translate();
				int onItem = 0;
				foreach (Pawn packmember in pack.pawns)
				{
					if ((onItem + 1) >= pack.pawns.Count)
						members = members + packmember.Name.ToStringShort + ". ";
					else
						members = members + packmember.Name.ToStringShort + ", ";
					onItem++;
				}

				Text.Font = GameFont.Medium;
				Rect PackNameRect = new Rect(outRect.xMin, rectPosY, 500f, 30f);

				Widgets.Label(PackNameRect, GetPackName(rect, pack));
				Rect RenameButtonRect = new Rect(outRect.xMax - 30f, rectPosY, 30f, 30f);
				if (Widgets.ButtonImage(RenameButtonRect, UIResources.Rename, true))
				{
					Find.WindowStack.Add(new Dialog_NamePack(pawn));
				}
				Widgets.DrawLineHorizontal(0f, (PackNameRect.yMax + 10f), rect.width);
				Text.Font = GameFont.Small;
				rectPosY = PackNameRect.yMax + 10f;
				rectPosY += 20f;

				Rect PackMemberCountRect = new Rect(outRect.xMax - 40f, rectPosY, 40f, 30f);
				string packcount = pack.pawns.Count.ToString() + "/" + maxSize.ToString();
				Widgets.Label(PackMemberCountRect, packcount);

				Rect PackMemberListRect = new Rect(outRect.xMin, rectPosY, 500f, outRect.height);
				Rect PackMemberListViewRect = new Rect(outRect.xMin, rectPosY, 480f, pack.pawns.Count * 30f);

				Widgets.BeginScrollView(PackMemberListRect, ref membersScrollPos, PackMemberListViewRect, true);
				float num = rectPosY;
				float y = membersScrollPos.y;
				float num2 = membersScrollPos.y + PackMemberListRect.height;

				for (int i = 0; i < pack.pawns.Count; i++)
				{
					float rowHeight = 30f;
					if (num > y - rowHeight && num < num2)
					{
						DrawMemberRow(num, PackMemberListRect.width, pack.pawns[i]);
					}
					num += rowHeight;
				}
				Widgets.EndScrollView();

			}
			else
			{
				Widgets.Label(rect, "NoPack".Translate());
				//Log.Message("Pack list size: " + AvaliPackDriver.packs.Count);						
				//Widgets.EndScrollView();
			}
			//Widgets.Label(rect, "test");
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
					if (otherPawn.Dead)
					{
						Messages.Message("MessageCantSelectDeadPawn".Translate(otherPawn.LabelShort, otherPawn).CapitalizeFirst(), MessageTypeDefOf.RejectInput, false);
					}
					else if (otherPawn.SpawnedOrAnyParentSpawned)
					{
						CameraJumper.TryJumpAndSelect(otherPawn);
					}
					else
					{
						Messages.Message("MessageCantSelectOffMapPawn".Translate(otherPawn.LabelShort, otherPawn).CapitalizeFirst(), MessageTypeDefOf.RejectInput, false);
					}
				}
			}
			Rect rect4 = new Rect(0f, y + 3f, 500f, rowHeight - 3f);
			Widgets.Label(rect4, otherPawn.Name.ToStringFull);

			//SocialCardUtility.DrawPawnLabel(otherPawn, rect3);

		}

		public AvaliTab()
		{
			this.size = new Vector2(460f, 450f);
			this.labelKey = "PackTab";
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000034 RID: 52 RVA: 0x00004078 File Offset: 0x00003078
		public override bool IsVisible
		{
			get
			{
				return packsEnabled;
			}
		}
		private Vector2 scrollPosition = Vector2.zero;

		// Token: 0x04006154 RID: 24916
		private float scrollViewHeight;

	}

}