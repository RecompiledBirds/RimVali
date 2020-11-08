using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using RimWorld;
using UnityEngine;
using Verse;


namespace AvaliMod
{
    public class AvaliTab : ITab
    {
		private readonly int maxSize = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().maxPackSize;
		private readonly bool packsEnabled = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packsEnabled;
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


			Rect TopHalf = outRect.TopHalf();
			Rect TopLeft = TopHalf.LeftHalf();
			Rect TopRight = TopHalf.RightHalf();
			Rect BottomHalf = outRect.BottomHalf();
			Rect BottomLeft = BottomHalf.LeftHalf();
			Rect BottomRight = BottomHalf.RightHalf();


			pack = this.GetPack(pawn);
			
			if (!(pack == null))
			{
				string members = "Packmates: ";
				int onItem = 0;
				foreach(Pawn packmember in pack.pawns)
                {
					if ((onItem + 1) >= pack.pawns.Count)
						members = members + packmember.Name.ToStringShort + ". ";
					else
						members = members + packmember.Name.ToStringShort + ", ";
					onItem++;
				}
				Widgets.DrawBox(TopLeft);
				Widgets.DrawBox(TopRight);
				Widgets.DrawBox(BottomRight);
				Widgets.DrawBox(BottomLeft);
				Widgets.Label(TopLeft, "Pack name:\n" +GetPackName(rect, pack)+"\n" /*+ "Pack size: " + pack.size.ToString() + "/" + maxSize.ToString() + "\n"*//*+members*/);
				Widgets.Label(TopRight, members);
				Widgets.Label(BottomRight, "Pack size: " + pack.size.ToString() + "/" + maxSize.ToString() + "\n");
			}
            else
            {
				Widgets.Label(rect,"test");
            }
			//Widgets.Label(rect, "test");
			Widgets.EndScrollView();
			GUI.EndGroup();
		}
		public AvaliTab()
		{
			this.size = new Vector2(460f, 450f);
			this.labelKey = "TabAvaliPack";
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