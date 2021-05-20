using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
namespace AvaliMod
{
    public class RenderObject
    {
        public Pawn pawn;
        public RenderableDef rDef;
    }
    public static class Renders
    {
        public static Dictionary<RenderObject, AvaliGraphic> graphics;


    }
}
