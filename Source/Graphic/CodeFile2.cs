/*using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
namespace AvaliMod
{
    public class AvaliPawnGraphicSet
    {
        public static readonly Color RottingColor;
        public static readonly Color DessicatedColorInsect;
        public List<ApparelGraphicRecord> apparelGraphics;
        public AvaliGraphic hairGraphic;
        public AvaliGraphic desiccatedHeadStumpGraphic;
        public AvaliGraphic headStumpGraphic;
        public AvaliGraphic skullGraphic;
        public Pawn pawn;
        public AvaliGraphic headGraphic;
        public DamageFlasher flasher;
        public AvaliGraphic packGraphic;
        public AvaliGraphic dessicatedGraphic;
        public AvaliGraphic rottingGraphic;
        public AvaliGraphic nakedGraphic;
        public AvaliGraphic desiccatedHeadGraphic;

        public PawnGraphicSet(Pawn pawn);

        public GraphicMeshSet HairMeshSet { get; }
        public bool AllResolved { get; }

        public void ClearCache();
        [Obsolete("Only need this overload to not break mod compatibility.")]
        public Material HairMatAt(Rot4 facing);
        public Material HairMatAt_NewTemp(Rot4 facing, bool portrait = false);
        [Obsolete("Only need this overload to not break mod compatibility.")]
        public Material HeadMatAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh, bool stump = false);
        public Material HeadMatAt_NewTemp(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh, bool stump = false, bool portrait = false);
        public List<Material> MatsBodyBaseAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh);
        public void ResolveAllGraphics();
        public void ResolveApparelGraphics();
        public void SetAllGraphicsDirty();
        public void SetApparelGraphicsDirty();
    }
}*/