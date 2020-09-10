using RimWorld;
using System.Collections.Generic;
using Verse;
namespace AvaliMod
{
    public class ExplodeDeath : HediffComp
    {
        public override void Notify_PawnKilled()
        {
            this.Pawn.equipment.DestroyAllEquipment(DestroyMode.Vanish);
            this.Pawn.apparel.DestroyAll(DestroyMode.Vanish);
            this.Pawn.Destroy(DestroyMode.Vanish);
            base.Notify_PawnKilled();
            GenExplosion.DoExplosion(Pawn.Position, Pawn.Map, 5f, DamageDefOf.Bomb, (Thing)null, -1, -1f, (SoundDef)null, (ThingDef)null, (ThingDef)null, (Thing)null, (ThingDef)null, 0.0f, 1, false, (ThingDef)null, 0.0f, 1, 0.0f, false, new float?(), (List<Thing>)null);
        }

    }
}