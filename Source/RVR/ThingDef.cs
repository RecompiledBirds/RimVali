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
        public RVRRaceInsertion raceInsertion = new RVRRaceInsertion();
        public List<ReplaceableThoughts> replaceableThoughts = new List<ReplaceableThoughts>();

        public List<BodyTypeDef> bodyTypes = new List<BodyTypeDef>();

        public butcherAndHarvestThoughts butcherAndHarvestThoughts = new butcherAndHarvestThoughts();

        public override void ResolveReferences()
        {
            this.comps.Add(new colorCompProps());
            base.ResolveReferences();
        }
        public bool replaceThought(ref ThoughtDef thought, bool log = false)
        {
            //Log.Message(replaceableThoughts.Count.ToString());
            //Log.Message("checking thought list..", true);
            foreach (ReplaceableThoughts replaceable in this.replaceableThoughts)
            {
               
                //The issue seems to be in this check, although i cannot imagine why
                if (replaceable.thoughtToReplace.defName == thought.defName)
                {
                    thought = replaceable.replacementThought;
                    return true;
                }
             

            }
            return false;
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
                        Color color1 = color.Generator(pawn).firstColor.NewRandomizedColor();
                        Color color2 = color.Generator(pawn).secondColor.NewRandomizedColor();
                        Color color3 = color.Generator(pawn).thirdColor.NewRandomizedColor();
                        colorcomp.colors.Add(color.name, new ColorSet(color1,color2,color3,color.isDyeable));
                    }
                }
            }
        }
        
       
        
        public class ReplaceableThoughts
        {
            public ThoughtDef thoughtToReplace;
            public ThoughtDef replacementThought;
        }
    }

    
}