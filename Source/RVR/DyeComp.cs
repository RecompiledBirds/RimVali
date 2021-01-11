using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
namespace AvaliMod
{
    public class DyeComp : HediffComp
    {
        public DyeCompProps Props
        {
            get
            {
                return (DyeCompProps)this.props;
            }
        }

        public Dictionary<string, ColorSet> oldColors = new Dictionary<string, ColorSet>();
        public List<string> colorKey = new List<string>();
        public List<ColorSet> colorValue = new List<ColorSet>();
        public override void CompExposeData()
        {
            Scribe_Collections.Look<string,ColorSet>(ref oldColors, "colors", LookMode.Value, LookMode.Deep, ref colorKey, ref colorValue);
            if(oldColors == null)
            {
                oldColors = new Dictionary<string, ColorSet>();
            }
        }
        public override void CompPostPostAdd(DamageInfo? dinfo)

        {
            Pawn pawn = this.parent.pawn;
            if(pawn.def is RimValiRaceDef def)
            {
                colorComp colorComp= pawn.GetComp<colorComp>();
                bool hasFound = false;
                foreach(string set in colorComp.colors.Keys)
                {
                    if (hasFound)
                    {
                        break;
                    }
                    if (Props.targetsSpecifcSet == true && Props.setsToChange.Contains(set))
                    {
                        if (colorComp.colors[set].dyeable)
                        {
                            oldColors.Add(set, new ColorSet(colorComp.colors[set].colorOne, colorComp.colors[set].colorTwo, colorComp.colors[set].colorThree, colorComp.colors[set].dyeable));

                            if (Props.changesFirstColor)
                            {

                                colorComp.colors[set].colorOne = Props.colors.firstColor.NewRandomizedColor();
                            }
                            if (Props.changesSecondColor)
                            {
                                colorComp.colors[set].colorTwo = Props.colors.secondColor.NewRandomizedColor();
                            }
                            if (Props.changesThirdColor)
                            {
                                colorComp.colors[set].colorThree = Props.colors.thirdColor.NewRandomizedColor();
                            }
                        }
                    }
                    else if(Props.targetsSpecifcSet != true)
                    {
                        if (colorComp.colors[set].dyeable)
                        {
                            oldColors.Add(set, new ColorSet(colorComp.colors[set].colorOne, colorComp.colors[set].colorTwo, colorComp.colors[set].colorThree, colorComp.colors[set].dyeable));

                            if (Props.changesFirstColor)
                            {

                                colorComp.colors[set].colorOne = Props.colors.firstColor.NewRandomizedColor();
                            }
                            if (Props.changesSecondColor)
                            {
                                colorComp.colors[set].colorTwo = Props.colors.secondColor.NewRandomizedColor();
                            }
                            if (Props.changesThirdColor)
                            {
                                colorComp.colors[set].colorThree = Props.colors.thirdColor.NewRandomizedColor();
                            }
                        }
                    }
                }
                PawnRenderer render = new PawnRenderer(pawn);
                render.graphics.ResolveAllGraphics();
            }
            //base.CompPostPostAdd(dinfo);
        }
        public override void CompPostPostRemoved()
        {
            Pawn pawn = this.parent.pawn;
            if (pawn.def is RimValiRaceDef def)
            {
                colorComp colorComp = pawn.GetComp<colorComp>();
                colorComp.colors = oldColors;
            }
            base.CompPostPostRemoved();
        }
    }
    public class DyeCompProps : HediffCompProperties
    {
        public TriColor_ColorGenerators colors;
        public bool changesFirstColor = true;
        public bool changesSecondColor = true;
        public bool changesThirdColor = true;

        public bool targetsSpecifcSet = false;
        public List<string> setsToChange;

        public DyeCompProps()
        {
            this.compClass = typeof(DyeComp);
        }
    }
}
