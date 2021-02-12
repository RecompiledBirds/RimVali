using Verse;
using RimWorld;
using System.Collections.Generic;

namespace AvaliMod
{
    public class FactionResearchRestrictionDef : Def
    {
        public List<FactionResearchRestriction> factionResearchRestrictions = new List<FactionResearchRestriction>();
        public List<FactionResearchRestriction> factionResearchRestrictionBlackList = new List<FactionResearchRestriction>();
    }
    public class FactionResearchRestriction
    {
        public ResearchProjectDef researchProj;
        public FactionDef factionDef;
    }
}
