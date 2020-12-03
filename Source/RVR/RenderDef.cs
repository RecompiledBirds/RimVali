using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using Unity;

namespace AvaliMod
{
    public class RenderableDef : Def
    {
        public string texPath;
        public string linkedBodyPart = null;
        public IntVec2 position;
        public bool CanShow(Pawn pawn)
        {
            if(linkedBodyPart == null)
            {
                return true;
            }
            IEnumerable<BodyPartRecord> bodyParts = pawn.health.hediffSet.GetNotMissingParts();
            if (bodyParts.Where<BodyPartRecord>(x => x.def.defName == linkedBodyPart || x.untranslatedCustomLabel == linkedBodyPart).Count() > 0){
                return true;
            }

            return false;
        }
    }
}