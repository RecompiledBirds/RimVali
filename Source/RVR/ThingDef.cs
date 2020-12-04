using RimWorld;
using Verse;
using System.Collections.Generic;
namespace AvaliMod
{
    public class RimValiRaceDef : ThingDef
    {
        public List<RenderableDef> bodyPartGraphics = new List<RenderableDef>();
        public graphics graphics = new graphics();
        public bool hasHair = false;
        public restrictions restrictions = new restrictions();
        public Main mainSettings = new Main();

    }
}