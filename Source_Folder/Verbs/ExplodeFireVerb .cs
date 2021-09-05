using RimWorld;
using Verse;
namespace AvaliMod
{
    public class Verb_Fire : Verb
    {
        protected override bool TryCastShot()
        {
            Verb_Fire.Flame(ReloadableCompSource);
            return true;
        }

        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = false;
            return EquipmentSource.GetStatValue(AvaliDefs.ExplodeFireRadius, true);
        }

        public static void Flame(CompReloadable comp)
        {
            if (comp == null || !comp.CanBeUsed)
            {
                return;
            }

            ThingWithComps parent = comp.parent;
            Pawn wearer = comp.Wearer;

            GenExplosion.DoExplosion(wearer.Position, wearer.Map, parent.GetStatValue(AvaliDefs.ExplodeFireRadius, true), DamageDefOf.Flame, null, -1, -1f, null, null, null, null, null, 0.0f, 1, false, null, 0.0f, 1, 0.0f, false, new float?(), null);

            comp.UsedOnce();
        }
    }
}