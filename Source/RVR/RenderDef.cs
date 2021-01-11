using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using Unity;
using UnityEngine;
using System;

namespace AvaliMod
{
    public class baseTex
    {
        public string tex;
        public string femaleTex;
    }

    public class hediffTex : baseTex
    {
        public HediffDef hediff;
    }
    public class backstoryTex : baseTex
    {
        public string backstoryTitle;
    }

    public class hediffStoryTex : baseTex
    {
        public string backstoryTitle;
        public HediffDef hediffDef;
    }
   
    public class RenderableDef : Def
    {
        public bool StoryIsName(Backstory story, string title)
        {
            //I have to check if everything is null so we get this mess, otherwise sometimes a null reference exception occurs.
            //There probably is a cleaner way of doing this I'm not aware of.
            return ((story.untranslatedTitle != null && story.untranslatedTitle == title)
                        || ((story.untranslatedTitle != null && story.untranslatedTitle == title)
                        || (story.untranslatedTitleShort != null && story.untranslatedTitleShort == title)
                        ||(story.untranslatedTitleFemale != null && story.untranslatedTitleFemale == title)
                        //This does not need to be checked, as it literally cannot ever be null.
                        || story.identifier == title
                        || (story.titleShort != null && story.titleShort == title)
                        || (story.titleFemale != null && story.titleFemale == title)
                        || (story.titleShortFemale != null && story.titleShortFemale == title
                        //Same here.
                        || story.title == title)));
            //Now we hope Tynan never changes backstories. Ever. Or else this thing breaks.
        }
        public string texPath(Pawn pawn)
        {
            string path = tex;

            if (femaleTex != null && pawn.gender == Gender.Female)
            {
                path = femaleTex;
            }

            //HediffStory gets highests priority here, by being lowest on this set
            Backstory adulthood = null;
            if (pawn.story.adulthood != null)
            {
                adulthood = pawn.story.adulthood;
            }
            Backstory childhood = pawn.story.childhood;
            foreach (backstoryTex backstoryTex in backstoryTextures)
            {
                //Log.Message(backstoryTex.backstoryTitle);
                if ((adulthood != null && StoryIsName(adulthood, backstoryTex.backstoryTitle)) || StoryIsName(childhood, backstoryTex.backstoryTitle))
                {

                    if (backstoryTex.femaleTex != null && pawn.gender == Gender.Female)
                    {
                        path = backstoryTex.femaleTex;
                    }
                    path = backstoryTex.tex; ;
                }
            }
            foreach (hediffTex hediffTex in hediffTextures)
            {
                foreach (BodyPartRecord bodyPartRecord in pawn.def.race.body.AllParts)
                {
                    BodyPartDef def = bodyPartRecord.def;
                    if (def.defName.ToLower() == bodyPart.ToLower() || def.label.ToLower() == bodyPart.ToLower())
                    {
                        if (pawn.health.hediffSet.HasHediff(hediffTex.hediff, bodyPartRecord, false))
                        {

                            if (hediffTex.femaleTex != null && pawn.gender == Gender.Female)
                            {
                                path = hediffTex.femaleTex;
                            }
                            path = hediffTex.tex; ;
                        }
                    }
                }

            }

            foreach (hediffStoryTex hediffStoryTex in hediffStoryTextures)
            {
                if ((adulthood != null && StoryIsName(adulthood, hediffStoryTex.backstoryTitle)) || StoryIsName(childhood, hediffStoryTex.backstoryTitle))
                {
                    foreach (BodyPartRecord bodyPartRecord in pawn.def.race.body.AllParts)
                    {
                        BodyPartDef def = bodyPartRecord.def;
                        if (def.defName.ToLower() == bodyPart.ToLower() || def.label.ToLower() == bodyPart.ToLower())
                        {
                            if (pawn.health.hediffSet.HasHediff(hediffStoryTex.hediffDef, bodyPartRecord, false))
                            {

                                if (def.defName.ToLower() == bodyPart.ToLower() || def.label.ToLower() == bodyPart.ToLower())
                                {
                                    if (pawn.health.hediffSet.HasHediff(hediffStoryTex.hediffDef, bodyPartRecord, false))
                                    {
                                        if (hediffStoryTex.femaleTex != null && pawn.gender == Gender.Female)
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
        public bool CanShowPortrait(Pawn pawn)
        {
            if (bodyPart == null)
            {
                return true;
            }
            IEnumerable<BodyPartRecord> bodyParts = pawn.health.hediffSet.GetNotMissingParts();
            //Log.Message(bodyParts.Any(x => x.def.defName.ToLower() == "left lower ear" || x.untranslatedCustomLabel.ToLower() == "left lower ear".ToLower()).ToString());
            try
            {
                if (bodyParts.Any(x => x.def.defName.ToLower() == bodyPart.ToLower() || x.Label.ToLower() == bodyPart.ToLower()))
                {
                    if (!pawn.Spawned)
                    {
                        return true;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                //Log.Message(e.ToString(), true);
                return true;
            }
        }
        public bool CanShow(Pawn pawn)
        {
            if (bodyPart == null)
            {
                return true;
            }
            IEnumerable<BodyPartRecord> bodyParts = pawn.health.hediffSet.GetNotMissingParts();
            //Log.Message(bodyParts.Any(x => x.def.defName.ToLower() == "left lower ear" || x.untranslatedCustomLabel.ToLower() == "left lower ear".ToLower()).ToString());
            try
            {
                if (bodyParts.Any(x => x.def.defName.ToLower() == bodyPart.ToLower() || x.Label.ToLower() == bodyPart.ToLower()))
                {
                    if (!pawn.Spawned)
                    {
                        return true;
                    }
                    else
                    {
                        if(!this.showsInBed && pawn.InBed() && !pawn.CurrentBed().def.building.bed_showSleeperBody)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }catch {
                //Log.Message(e.ToString(), true);
                return true;
            }
        }
    }
}