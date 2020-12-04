using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using Unity;
using UnityEngine;

namespace AvaliMod
{
    public class RenderableDef : Def
    {
        public string texPath;
        public string linkedBodyPart = null;

        public string useColorSet;
        public BodyPartGraphicPos east = new BodyPartGraphicPos();
        public BodyPartGraphicPos north = new BodyPartGraphicPos();
        public BodyPartGraphicPos south = new BodyPartGraphicPos();
        public BodyPartGraphicPos west;

  

        public bool CanShow(Pawn pawn)
        {
            if(linkedBodyPart == null)
            {
                return true;
            }
            try
            {
                IEnumerable<BodyPartRecord> bodyParts = pawn.health.hediffSet.GetNotMissingParts();

                if (bodyParts.Where<BodyPartRecord>(x => x.def.defName.ToLower() == linkedBodyPart.ToLower() || x.untranslatedCustomLabel.ToLower() == linkedBodyPart.ToLower()).Count() > 0)
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