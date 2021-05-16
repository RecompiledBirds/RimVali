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
<<<<<<< HEAD
=======
		public Vector2 bonusScrollPos = new Vector2();
>>>>>>> beta
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
		public Vector2 WinSize = new Vector2(630f, 510f);
		protected override void FillTab()
		{
<<<<<<< HEAD
=======
			bool debugSquares = RimValiMod.settings.enableDebugMode;
			Listing_Standard ls = new Listing_Standard();


>>>>>>> beta
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
<<<<<<< HEAD
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
=======
			AvaliPack pack = pawn.GetPack();
			float rectPosY = 0f;

			if (pack != null && pack.pawns.Count > 1)
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
						foreach(string str in skillDef.effectList)
                        {
							effects = effects + $"{str}\n";

						}
					}
					
				}
				Text.Font = GameFont.Medium;
				Rect PackNameRect = new Rect(outRect.xMin, rectPosY, 500f, 30f);
				Rect RenameButtonRect = new Rect(outRect.xMax - 30f, rectPosY, 30f, 30f);
>>>>>>> beta
				Widgets.DrawLineHorizontal(0f, (PackNameRect.yMax + 10f), rect.width);
				Text.Font = GameFont.Small;
				rectPosY = PackNameRect.yMax + 10f;
				rectPosY += 20f;

<<<<<<< HEAD
				Rect PackMemberCountRect = new Rect(outRect.xMax - 40f, rectPosY, 40f, 30f);
				string packcount = pack.pawns.Count.ToString() + "/" + maxSize.ToString();
				Widgets.Label(PackMemberCountRect, packcount);
=======
>>>>>>> beta

				Rect PackMemberListRect = new Rect(outRect.xMin, rectPosY, 500f, outRect.height);
				Rect PackMemberListViewRect = new Rect(outRect.xMin, rectPosY, 480f, pack.pawns.Count * 30f);

<<<<<<< HEAD
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

=======
				Rect PackMemberCountRect = new Rect(outRect.RightHalf().x, rectPosY, 40f, 30f);
				string packcount = pack.pawns.Count.ToString() + "/" + maxSize.ToString();

				Rect bonusRect = new Rect(PackMemberCountRect.xMax, rectPosY, outRect.RightHalf().width, outRect.RightHalf().height);
				Rect bonusViewRect = new Rect(PackMemberCountRect.x + PackMemberCountRect.width, rectPosY, outRect.RightHalf().width-20, listCount*30f);
				float num = rectPosY;
				float y = membersScrollPos.y;
				float num2 = membersScrollPos.y + PackMemberListRect.height;
				void drawLabels()
                {
					//Count
					Widgets.Label(PackMemberCountRect, packcount);
					
					//List of packmates
					num = rectPosY;
					y = membersScrollPos.y;
					num2 = membersScrollPos.y + PackMemberListRect.height;
					Widgets.BeginScrollView(PackMemberListRect, ref membersScrollPos, PackMemberListViewRect, true);
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
					
					//Name
					Widgets.Label(PackNameRect, GetPackName(rect, pack));
					if (Widgets.ButtonImage(RenameButtonRect, UIResources.Rename, true))
					{
						Find.WindowStack.Add(new Dialog_NamePack(pawn));
					}
					//Border line
					float offset = 10f;
					float heightOffset = 12f;
					Widgets.DrawLine(new Vector2(bonusRect.x-offset,PackNameRect.y+PackNameRect.height+heightOffset),new Vector2(bonusRect.x-offset,bonusRect.y+bonusRect.height),Color.white,1f);

					//Speciality

					Widgets.Label(bonusRect, new GUIContent { text = $"{"PackSpeciality".Translate(packSpecialityName.Named("SPECIALITY"))} \n\n{"PackEffects".Translate()} \n{effects}" });

				

				}
				drawLabels();
				
				//Makes it easier to see the GUI layout
				if (debugSquares)
				{
					Widgets.DrawBoxSolid(PackMemberListRect,Color.red);
					Widgets.DrawBoxSolid(PackMemberListViewRect,Color.blue);
					Widgets.DrawBoxSolid(PackNameRect,Color.magenta);
					Widgets.DrawBoxSolid(bonusRect,Color.green);
					Widgets.DrawBoxSolid(PackMemberCountRect, Color.cyan);
					Widgets.DrawBoxSolid(bonusViewRect, Color.black);


					drawLabels();
					
				}
>>>>>>> beta
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
<<<<<<< HEAD
			this.size = new Vector2(460f, 450f);
=======
			this.size = new Vector2(600f, 450f);
>>>>>>> beta
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