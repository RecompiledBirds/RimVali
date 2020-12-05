using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
namespace AvaliMod
{
    public class RimValiRaceDef : ThingDef, IExposable
    {
        public List<RenderableDef> bodyPartGraphics = new List<RenderableDef>();
        public graphics graphics = new graphics();
        public bool hasHair = false;
        public restrictions restrictions = new restrictions();
        public Main mainSettings = new Main();
        public bool useHumanRecipes = true;

        public List<ReplaceableThoughts> replaceableThoughts = new List<ReplaceableThoughts>();

        public Dictionary<string, ColorSet> colors = new Dictionary<string, ColorSet>();
        
        public ThoughtDef replaceThought(ThoughtDef thought)
        {
            foreach(ReplaceableThoughts replaceable in this.replaceableThoughts)
            {
                if (replaceable.thoughtToReplace == thought)
                {
                    Log.Message("Replacing thought.");
                    thought = replaceable.replacementThought;
                }
            }
            return thought;
        }
        public Dictionary<string, ColorSet> colorSets;
        public void ExposeData()
        {
            Scribe_Collections.Look<string, ColorSet>(ref colorSets, "pawnColorSet");
            if(colors == null)
            {
                colors = new Dictionary<string, ColorSet>();
            }
        }
        public void GenColors()
        {
            foreach(Colors colors in graphics.colorSets)
            {
                if (!colorSets.ContainsKey(colors.name))
                {
                    ColorSet colorSet = new ColorSet(colors.colorGenerator.firstColor.NewRandomizedColor(), colors.colorGenerator.secondColor.NewRandomizedColor(), colors.colorGenerator.thirdColor.NewRandomizedColor());
                    colorSets.Add(colors.name, colorSet);
                }
            }
        }
        
       
        /*
        public ThoughtDef ReplaceThought(ThoughtDef thought) =>
            (this.replaceableThoughts == null || this.replaceableThoughts.Select(x=>x.).Contains(thought))
               ? thought : this.replaceableThoughts.FirstOrDefault(predicate: tr => tr.original == thought)?.replacer ?? thought;
        */
        public class ReplaceableThoughts
        {
            public ThoughtDef thoughtToReplace;
            public ThoughtDef replacementThought;
        }
    }

    
}