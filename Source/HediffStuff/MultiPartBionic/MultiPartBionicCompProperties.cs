using System.Collections.Generic;
using Verse;

namespace AvaliMod
{
    public class MultiPartBionicCompProperties : HediffCompProperties
    {
        public List<BodyPartDef> bodyPartsMustBeOn;
        public List<BodyPartDef> bodyPartsToAffect;
        public bool displayTextWhenChanged;
        public List<HediffDef> hediffsToAdd;
        public List<HediffDef> otherHediffs;
        public string stringForHediffsLeft;
        public string textOnAdd;
        public string textOnRemove;
        public int timeToFade;

        public MultiPartBionicCompProperties()
        {
            compClass = typeof(MultiPartBionic);
        }
    }
}
