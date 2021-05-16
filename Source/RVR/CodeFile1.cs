using RimWorld;
using Verse;
using System.Collections.Generic;
namespace AvaliMod { 
    public class skillGains
    {
        public SkillDef skill;
        public int amount;
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
        public BackstorySlot backstorySlot = BackstorySlot.Adulthood;
        public List<string> spawnInCategories = new List<string>();
        public List<skillGains> skillGains = new List<skillGains>();
        public List<TraitEntry> forcedTraits = new List<TraitEntry>();
        public List<TraitEntry> disabledTraits = new List<TraitEntry>();
        public int maleCommonality;
        public int femaleCommonality;
        public Backstory story;
        public bool CanSpawn(Pawn pawn)
        {
            if(!canSpawnFemale && pawn.gender == Gender.Female)
            {
                return false;
            }
            if(!canSpawnMale && pawn.gender == Gender.Male)
            {
                return false;
            }
            return true;
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
                forcedTraits = this.forcedTraits,
                disallowedTraits = this.disabledTraits,
            };

            BackstoryDatabase.AddBackstory(story);
        }
    }
}