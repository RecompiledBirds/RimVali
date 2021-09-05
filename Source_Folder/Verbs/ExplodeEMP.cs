using RimWorld;
using Verse;
namespace AvaliMod
{
    public class Verb_EMP : Verb
    {
        protected override bool TryCastShot()
        {
            Verb_EMP.EMP(ReloadableCompSource);
            return true;
        }

        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = false;
            return EquipmentSource.GetStatValue(AvaliDefs.ExplodeEMPRadius, true);
        }

        public static void EMP(CompReloadable comp)
        {
            if (comp == null || !comp.CanBeUsed)
            {
                return;
            }

            ThingWithComps parent = comp.parent;
            Pawn wearer = comp.Wearer;
            GenExplosion.DoExplosion(wearer.Position, wearer.Map, parent.GetStatValue(AvaliDefs.ExplodeEMPRadius, true), DamageDefOf.EMP, null, -1, -1f, null, null, null, null, null, 0.0f, 1, false, null, 0.0f, 1, 0.0f, false, new float?(), null);
            comp.UsedOnce();

        }
    }
}