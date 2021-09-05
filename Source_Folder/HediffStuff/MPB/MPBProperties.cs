using System.Collections.Generic;
using Verse;
namespace AvaliMod
{
    public class MultiPartBionicCompProperties : HediffCompProperties
    {
        public List<HediffDef> otherHediffs;
        public List<HediffDef> hediffsToAdd;
        public bool displayTextWhenChanged;
        public string textOnAdd;
        public string textOnRemove;
        public int timeToFade;
        public string stringForHediffsLeft;
        public List<BodyPartDef> bodyPartsToAffect;
        public List<BodyPartDef> bodyPartsMustBeOn;
        public MultiPartBionicCompProperties()
        {
            compClass = typeof(MultiPartBionic);
        }
    }
}