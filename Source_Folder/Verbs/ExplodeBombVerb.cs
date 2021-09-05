using RimWorld;
using Verse;
namespace AvaliMod
{
    public class Verb_Explode : Verb
    {
        protected override bool TryCastShot()
        {
            Verb_Explode.Explode(ReloadableCompSource);
            return true;
        }
        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = false;
            return EquipmentSource.GetStatValue(AvaliDefs.ExplodeBombRadius, true);
        }

        public static void Explode(CompReloadable comp)
        {
            if (comp == null || !comp.CanBeUsed)
            {
                return;
            }

            ThingWithComps parent = comp.parent;
            Pawn wearer = comp.Wearer;
            GenExplosion.DoExplosion(wearer.Position, wearer.Map, parent.GetStatValue(AvaliDefs.ExplodeBombRadius, true), DamageDefOf.Bomb, null, -1, -1f, null, null, null, null, null, 0.0f, 1, false, null, 0.0f, 1, 0.0f, false, new float?(), null);
            GenExplosion.DoExplosion(wearer.Position, wearer.Map, 1f, DamageDefOf.Bomb, null, -1, -1f, null, null, null, null, null, 0.0f, 1, false, null, 0.0f, 1, 0.0f, false, new float?(), null);

            comp.UsedOnce();

        }
    }
}