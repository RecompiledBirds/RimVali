using RimWorld;
using Verse;

namespace AvaliMod
{
    public class Verb_Explode : Verb
    {
        protected override bool TryCastShot()
        {
            Explode(ReloadableCompSource);
            return true;
        }

        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = false;
            return EquipmentSource.GetStatValue(AvaliDefs.ExplodeBombRadius);
        }

        public static void Explode(CompReloadable comp)
        {
            if (comp == null || !comp.CanBeUsed)
            {
                return;
            }

            ThingWithComps parent = comp.parent;
            Pawn wearer = comp.Wearer;
            GenExplosion.DoExplosion(wearer.Position, wearer.Map, parent.GetStatValue(AvaliDefs.ExplodeBombRadius),
                DamageDefOf.Bomb, null);
            GenExplosion.DoExplosion(wearer.Position, wearer.Map, 1f, DamageDefOf.Bomb, null);

            comp.UsedOnce();
        }
    }
}
