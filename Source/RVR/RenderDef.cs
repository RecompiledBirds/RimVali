using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using Unity;
using UnityEngine;

namespace AvaliMod
{

    public class hediffTex
    {
        public HediffDef hediff;
        public string tex;
        public string femaleTex;
    }
    public class backstoryTex
    {
        public string backstoryTitle;
        public string tex;
        public string femaleTex;
    }

    public class hediffStoryTex
    {
        public string backstoryTitle;
        public HediffDef hediffDef;
        public string tex;
        public string femaleTex;
    }

    public class RenderableDef : Def
    {
        public string texPath(Pawn pawn)
        {
            string path = tex;

            foreach (backstoryTex backstoryTex in backstoryTextures)
            {
                if (pawn.story.childhood.untranslatedTitle == backstoryTex.backstoryTitle 
                    || pawn.story.adulthood.untranslatedTitle == backstoryTex.backstoryTitle || pawn.story.adulthood.untranslatedTitleShort == backstoryTex.backstoryTitle || pawn.story.adulthood.untranslatedTitleFemale == backstoryTex.backstoryTitle 
                    || pawn.story.childhood.untranslatedTitleShort == backstoryTex.backstoryTitle || pawn.story.childhood.untranslatedTitleFemale == backstoryTex.backstoryTitle || pawn.story.childhood.title == backstoryTex.backstoryTitle
                    || pawn.story.adulthood.title == backstoryTex.backstoryTitle 
                    || pawn.story.adulthood.titleShort == backstoryTex.backstoryTitle || pawn.story.adulthood.titleFemale == backstoryTex.backstoryTitle 
                    || pawn.story.childhood.titleShort == backstoryTex.backstoryTitle || pawn.story.childhood.titleFemale == backstoryTex.backstoryTitle)
                {
                    path = backstoryTex.tex;
                    if (backstoryTex.femaleTex != null && pawn.gender == Gender.Female)
                    {
                        path = backstoryTex.femaleTex;
                    }
                    else
                    {

                    }
                }
            }

            foreach (hediffTex hediffTex in hediffTextures)
            {

                if (pawn.health.hediffSet.HasHediff(hediffTex.hediff, false))
                {
                    path = hediffTex.tex;
                    if (hediffTex.femaleTex != null && pawn.gender == Gender.Female)
                    {
                        path = hediffTex.femaleTex;
                    }
                }
            }

            if (femaleTex != null && pawn.gender == Gender.Female)
            {
                path = femaleTex;
            }
            foreach(hediffStoryTex hediffStoryTex in hediffStoryTextures) {
                if (pawn.story.childhood.untranslatedTitle == hediffStoryTex.backstoryTitle
                    || pawn.story.adulthood.untranslatedTitle == hediffStoryTex.backstoryTitle || pawn.story.adulthood.untranslatedTitleShort == hediffStoryTex.backstoryTitle || pawn.story.adulthood.untranslatedTitleFemale == hediffStoryTex.backstoryTitle
                    || pawn.story.childhood.untranslatedTitleShort == hediffStoryTex.backstoryTitle || pawn.story.childhood.untranslatedTitleFemale == hediffStoryTex.backstoryTitle || pawn.story.childhood.title == hediffStoryTex.backstoryTitle
                    || pawn.story.adulthood.title == hediffStoryTex.backstoryTitle
                    || pawn.story.adulthood.titleShort == hediffStoryTex.backstoryTitle || pawn.story.adulthood.titleFemale == hediffStoryTex.backstoryTitle
                    || pawn.story.childhood.titleShort == hediffStoryTex.backstoryTitle || pawn.story.childhood.titleFemale == hediffStoryTex.backstoryTitle)
                {
                    if (pawn.health.hediffSet.HasHediff(hediffStoryTex.hediffDef))
                    {
                        if(hediffStoryTex.femaleTex != null && pawn.gender == Gender.Female)
                        {
                            path = hediffStoryTex.femaleTex;
                        }
                        else
                        {
                            path = hediffStoryTex.tex;
                        }
                    }
                }
            }

            
            return path;

        }

        public string tex;
        public string femaleTex;
        public string bodyPart = null;

        public bool showsInBed = true;

        public string useColorSet;
        public BodyPartGraphicPos east = new BodyPartGraphicPos();
        public BodyPartGraphicPos north = new BodyPartGraphicPos();
        public BodyPartGraphicPos south = new BodyPartGraphicPos();
        public BodyPartGraphicPos west;

        List<backstoryTex> backstoryTextures = new List<backstoryTex>();
        List<hediffTex> hediffTextures = new List<hediffTex>();
        List<hediffStoryTex> hediffStoryTextures = new List<hediffStoryTex>();

        public bool CanShow(Pawn pawn)
        {
            if (bodyPart == null)
            {
                return true;
            }
            try
            {
                IEnumerable<BodyPartRecord> bodyParts = pawn.health.hediffSet.GetNotMissingParts();

                if (bodyParts.Where<BodyPartRecord>(x => x.def.defName.ToLower() == bodyPart.ToLower() || x.untranslatedCustomLabel.ToLower() == bodyPart.ToLower()).Count() > 0)
                {
                    
                    return true;
                }
            }
            catch
            {
                return true;
            }

            return false;
        }
    }
}