using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AvaliMod
{
    public class RimValiRaceDef : ThingDef
    {
        public List<RenderableDef> bodyPartGraphics = new List<RenderableDef>();
        public graphics graphics = new graphics();
        public bool hasHair = false;
        public restrictions restrictions = new restrictions();
        public Main mainSettings = new Main();
        public bool useHumanRecipes = true;

        public List<ReplaceableThoughts> replaceableThoughts = new List<ReplaceableThoughts>();

        public override void ResolveReferences()
        {
            this.comps.Add(new colorCompProps());
            base.ResolveReferences();
        }
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
        public void GenColors(Pawn pawn)
        {
            if(pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                colorComp colorcomp = pawn.TryGetComp<colorComp>();
                foreach(Colors color in rimValiRaceDef.graphics.colorSets)
                {
                    if (!colorcomp.colors.ContainsKey(color.name))
                    {
                        Color color1 = color.colorGenerator.firstColor.NewRandomizedColor();
                        Color color2 = color.colorGenerator.secondColor.NewRandomizedColor();
                        Color color3 = color.colorGenerator.thirdColor.NewRandomizedColor();
                        colorcomp.colors.Add(color.name, new ColorSet(color1,color2,color3));
                    }
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