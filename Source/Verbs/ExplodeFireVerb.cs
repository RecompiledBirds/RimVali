using RimWorld;
using System.Collections.Generic;
using Verse;
namespace AvaliMod
{
    public class Verb_Fire : Verb
    {
        protected override bool TryCastShot()
        {
            Verb_Fire.Flame(this.ReloadableCompSource);
            return true;
        }

        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = false;
            return EquipmentSource.GetStatValue(AvaliDefs.ExplodeFireRadius);
        }

        public static void Flame(CompReloadable comp)
        {
            if (comp == null || !comp.CanBeUsed)
                return;
            ThingWithComps parent = comp.parent;
            Pawn wearer = comp.Wearer;

            GenExplosion.DoExplosion(wearer.Position, wearer.Map, parent.GetStatValue(AvaliDefs.ExplodeFireRadius),
                DamageDefOf.Flame, null);

            comp.UsedOnce();
        }
    }
}
