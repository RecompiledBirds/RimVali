using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AvaliMod
{
    public class RimValiRaceDef : ThingDef
    {
        public List<RenderableDef> renderableDefs = new List<RenderableDef>();
        public raceColors graphics = new raceColors();
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
        public bool ReplaceThought(ref ThoughtDef thought, bool log = false)
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


        public void HeadOffsetPawn(Rot4 rot,Pawn pawn ,ref Vector3 __result)
        {

            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                //This is an automatic check to see if we can put the head position here.
                //no human required
                if (rimValiRaceDef.renderableDefs.Where(x => x.defName.ToLower() == "head").Count() > 0)
                {

                    Vector2 offset = new Vector2(0, 0);

                    RenderableDef headDef = rimValiRaceDef.renderableDefs.First(x => x.defName.ToLower() == "head");
                    Vector3 pos = new Vector3(0, 0, 0);
                    pos.y = __result.y;
                    if (headDef.west == null)
                    {
                        headDef.west = headDef.east;
                    }
                    if (rot == Rot4.South)
                    {
                        pos.x = headDef.south.position.x + offset.x;
                        pos.z = headDef.south.position.y + offset.y;

                    }
                    else if (rot == Rot4.North)
                    {
                        pos.x = headDef.north.position.x + offset.x;
                        pos.z = headDef.north.position.y + offset.y;
                    }
                    else if (rot == Rot4.East)
                    {
                        pos.x = headDef.east.position.x + offset.x;
                        pos.z = headDef.east.position.y + offset.y;
                    }
                    else
                    {
                        pos.x = headDef.west.position.x + offset.x;
                        pos.z = headDef.west.position.y + offset.y;
                    }
                    //Log.Message(pos.ToString());
                    __result = __result + pos;
                }

            }
        }
        public void HeadOffsetPawn(PawnRenderer __instance, ref Vector3 __result)
        {
            Pawn pawn = __instance.graphics.pawn;
            PawnGraphicSet set = __instance.graphics;
            if (pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                //This is an automatic check to see if we can put the head position here.
                //no human required
                if (rimValiRaceDef.renderableDefs.Where(x => x.defName.ToLower() == "head").Count() > 0)
                {

                    Vector2 offset = new Vector2(0, 0);

                    RenderableDef headDef = rimValiRaceDef.renderableDefs.First(x => x.defName.ToLower() == "head");
                    __instance.graphics.headGraphic.drawSize = headDef.south.size;
                    Vector3 pos = new Vector3(0, 0, 0);
                    pos.y = __result.y;
                    if (headDef.west == null)
                    {
                        headDef.west = headDef.east;
                    }
                    if (pawn.Rotation == Rot4.South)
                    {
                        pos.x = headDef.south.position.x + offset.x;
                        pos.z = headDef.south.position.y + offset.y;

                    }
                    else if (pawn.Rotation == Rot4.North)
                    {
                        pos.x = headDef.north.position.x + offset.x;
                        pos.z = headDef.north.position.y + offset.y;
                    }
                    else if (pawn.Rotation == Rot4.East)
                    {
                        pos.x = headDef.east.position.x + offset.x;
                        pos.z = headDef.east.position.y + offset.y;
                    }
                    else
                    {
                        pos.x = headDef.west.position.x + offset.x;
                        pos.z = headDef.west.position.y + offset.y;
                    }
                    //Log.Message(pos.ToString());
                    __result = __result + pos;
                }

            }
        }
        public void GenGraphics(Pawn pawn)
        {
            
            if(pawn.def is RimValiRaceDef rimValiRaceDef)
            {
                colorComp colorcomp = pawn.TryGetComp<colorComp>();
                
                foreach (RenderableDef renderableDef in renderableDefs)
                {
                    if (!colorcomp.renderableDefIndexes.ContainsKey(renderableDef.defName))
                    {
                        if(renderableDef.linkIndexWithDef != null)
                        {
                            if (colorcomp.renderableDefIndexes.ContainsKey(renderableDef.linkIndexWithDef.defName))
                            {
                                colorcomp.renderableDefIndexes.Add(renderableDef.defName, colorcomp.renderableDefIndexes[renderableDef.linkIndexWithDef.defName]);
                            }
                            else
                            {
                                var Rand = new System.Random();
                                int index = Rand.Next(renderableDef.textures.Count);
                                colorcomp.renderableDefIndexes.Add(renderableDef.linkIndexWithDef.defName, index);
                                colorcomp.renderableDefIndexes.Add(renderableDef.defName, index);
                            }
                        }
                        else
                        {
                            var Rand = new System.Random();
                            int index = Rand.Next(renderableDef.textures.Count);
                            colorcomp.renderableDefIndexes.Add(renderableDef.defName, index);
                        }
                    }
                }
                if (!DefDatabase<RVRBackstory>.AllDefs.Where(x => (pawn.story.adulthood != null && x.defName == pawn.story.adulthood.identifier) || x.defName == pawn.story.childhood.identifier).EnumerableNullOrEmpty())
                {

                    List<RVRBackstory> backstories = DefDatabase<RVRBackstory>.AllDefs.Where(x => (pawn.story.adulthood != null && x.defName == pawn.story.adulthood.identifier) || x.defName == pawn.story.childhood.identifier).ToList();
                    RVRBackstory story = backstories[0];

                    if (!story.colorGenOverrides.NullOrEmpty())
                    {
                        foreach (Colors color in story.colorGenOverrides)
                        {
                            if (!colorcomp.colors.ContainsKey(color.name))
                            {
                                Color color1 = color.Generator(pawn).firstColor.NewRandomizedColor();
                                Color color2 = color.Generator(pawn).secondColor.NewRandomizedColor();
                                Color color3 = color.Generator(pawn).thirdColor.NewRandomizedColor();
                                colorcomp.colors.Add(color.name, new ColorSet(color1, color2, color3, color.isDyeable));
                            }
                        }

                    }
                }
                
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