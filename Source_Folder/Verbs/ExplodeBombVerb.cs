using RimWorld;
using System.Collections.Generic;
using Verse;
namespace AvaliMod
{
    public class Verb_Explode : Verb
    {
        protected override bool TryCastShot()
        {
            Verb_Explode.Explode(this.ReloadableCompSource);
            return true;
        }
        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = false;
            return this.EquipmentSource.GetStatValue(AvaliDefs.ExplodeBombRadius, true);
        }

        public static void Explode(CompReloadable comp)
        {
            if (comp == null || !comp.CanBeUsed)
                return;
            ThingWithComps parent = comp.parent;
            Pawn wearer = comp.Wearer;
            GenExplosion.DoExplosion(wearer.Position, wearer.Map, parent.GetStatValue(AvaliDefs.ExplodeBombRadius, true), DamageDefOf.Bomb, (Thing)null, -1, -1f, (SoundDef)null, (ThingDef)null, (ThingDef)null, (Thing)null, (ThingDef)null, 0.0f, 1, false, (ThingDef)null, 0.0f, 1, 0.0f, false, new float?(), (List<Thing>)null);
            GenExplosion.DoExplosion(wearer.Position, wearer.Map, 1f, DamageDefOf.Bomb, (Thing)null, -1, -1f, (SoundDef)null, (ThingDef)null, (ThingDef)null, (Thing)null, (ThingDef)null, 0.0f, 1, false, (ThingDef)null, 0.0f, 1, 0.0f, false, new float?(), (List<Thing>)null);

            comp.UsedOnce();

        }
    }
}