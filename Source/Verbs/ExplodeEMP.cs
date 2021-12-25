using RimWorld;
using Verse;

namespace AvaliMod
{
    public class Verb_EMP : Verb
    {
        protected override bool TryCastShot()
        {
            EMP(ReloadableCompSource);
            return true;
        }

        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = false;
            return EquipmentSource.GetStatValue(AvaliDefs.ExplodeEMPRadius);
        }

        public static void EMP(CompReloadable comp)
        {
            if (comp == null || !comp.CanBeUsed)
            {
                return;
            }

            ThingWithComps parent = comp.parent;
            Pawn wearer = comp.Wearer;
            GenExplosion.DoExplosion(wearer.Position, wearer.Map, parent.GetStatValue(AvaliDefs.ExplodeEMPRadius),
                DamageDefOf.EMP, null);
            comp.UsedOnce();
        }
    }
}
