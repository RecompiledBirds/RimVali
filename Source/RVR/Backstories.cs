using RimWorld;
using Verse;
using System.Collections.Generic;
using System;

namespace AvaliMod { 

    public class BodyPartToAffect
    {
        public HediffDef hediffDef;
        public BodyPartDef bodyPartDef = BodyPartDefOf.Torso;
    }

    public class skillGains
    {
        public SkillDef skill;
        public int amount;
    }
    public class traitList
    {
        public TraitDef def;
        public int degree;
    }
    public class RVRBackstory : Def
    {
        public string storyDesc;
        public string title;
        public string femaleTitle;
        public string shortTitle;
        public string shortFemaleTitle;
        
        public bool canSpawnMale = true;
        public bool canSpawnFemale = true;

        //These stack!
        //A global chance of 50 and a female chance of 50 would be 25 for female pawns.
        public int femaleChance = 100;
        public int maleChance = 100;
        public int globalChance = 100;
        
        public BackstorySlot backstorySlot = BackstorySlot.Adulthood;
        
        
        public List<string> spawnInCategories = new List<string>();
        public List<skillGains> skillGains = new List<skillGains>();
        public List<traitList> forcedTraits = new List<traitList>();
        public List<traitList> disabledTraits = new List<traitList>();
        public List<WorkTags> disabledWorkTypes = new List<WorkTags>();

        public string linkedStoryIdentifier;

        //Not accessible in XML, dont use them.
        public Backstory story;
        private List<TraitEntry> traitsToForce = new List<TraitEntry>();
        private List<TraitEntry> traitsToDisable = new List<TraitEntry>();
        public bool CanSpawn(Pawn pawn)
        {
            if(this.backstorySlot == BackstorySlot.Adulthood)
            {
                if(linkedStoryIdentifier != null && !(pawn.story.childhood.identifier == linkedStoryIdentifier))
                {
                    //Log.Message("Story can't spawn: linked story not avalible");
                    return false;
                }
            }
            else
            {
                if(linkedStoryIdentifier !=null &&! (pawn.story.adulthood.identifier == linkedStoryIdentifier))
                {
                    //Log.Message("Story can't spawn: linked story not avalible");
                    return false;
                }
            }
            if (Rand.Range(0, 100) < globalChance)
            {

                if (!canSpawnFemale && pawn.gender == Gender.Female)
                {
                    return false;
                }
                if (!canSpawnMale && pawn.gender == Gender.Male)
                {
                    return false;
                }
                if (pawn.gender == Gender.Female)
                {
                    if (Rand.Range(0, 100) < femaleChance)
                    {
                        //Log.Message("Story can't spawn: chance roll (female)");
                        return true;
                    }
                    return false;
                }
                else
                {
                    if (Rand.Range(0, 100) < maleChance)
                    {
                        return true;
                    }
                    //Log.Message("Story can't spawn: chance roll (male)");
                    return false;
                }
            }
           // Log.Message("Story can't spawn: chance roll (global)");
            return false;
        }
        public override void ResolveReferences()
        {
            Dictionary<SkillDef, int> skills = new Dictionary<SkillDef, int>();
            base.ResolveReferences();
            foreach(skillGains skillGains in this.skillGains)
            {
                if (!skills.ContainsKey(skillGains.skill))
                {
                    skills.Add(skillGains.skill, skillGains.amount);
                }
            }
            foreach (traitList traitItem in forcedTraits){
                traitsToForce.Add(new TraitEntry(traitItem.def, traitItem.degree));
            }
            foreach(traitList traitItem in disabledTraits)
            {

            }
            this.story = new Backstory
            {
                slot = this.backstorySlot,
                title = this.title,
                titleFemale = this.femaleTitle,
                titleShort = this.shortTitle,
                titleShortFemale = this.shortFemaleTitle,
                identifier = this.defName,
                baseDesc = this.storyDesc,
                spawnCategories = this.spawnInCategories,
                skillGainsResolved = skills,
                forcedTraits = this.traitsToForce,
                disallowedTraits = this.traitsToDisable,
                workDisables = ((Func<WorkTags>)delegate
                {
                    WorkTags tags = WorkTags.None;
                    this.disabledWorkTypes.ForEach(tag => tags |= tag);
                    return tags;
                }).Invoke()

                };
            

            BackstoryDatabase.AddBackstory(story);
            //Log.Message("created story: " + this.defName);
        }
    }
}