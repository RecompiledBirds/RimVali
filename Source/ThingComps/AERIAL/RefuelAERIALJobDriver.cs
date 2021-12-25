using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace AvaliMod
{
    public class RefuelAERIALJobDriver : JobDriver
    {
        private static bool GunNeedsLoading(Building b)
        {
            if (!(b is AERIALSystem aerialSystem))
            {
                return false;
            }

            var compChangeableProjectile = aerialSystem.gun.TryGetComp<AERIALChangeableProjectile>();
            if (compChangeableProjectile == null)
            {
                return false;
            }

            return !(compChangeableProjectile.loadedShells.Count >= RimValiMod.settings.AERIALShellCap);
        }

        public static Thing FindAmmo(Pawn pawn, AERIALSystem aerial)
        {
            StorageSettings allowed = pawn.IsColonist
                ? aerial.gun.TryGetComp<AERIALChangeableProjectile>().allowedShellsSettings
                : null;

            bool Validator(Thing t)
            {
                return !t.IsForbidden(pawn) && pawn.CanReserve(t, 10, 1) &&
                       (allowed == null || allowed.AllowedToAccept(t));
            }

            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.Shell), PathEndMode.OnCell, TraverseParms.For(pawn), 40f,
                Validator);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            var loadIfNeeded = new Toil();
            loadIfNeeded.initAction = delegate
            {
                Pawn actor = loadIfNeeded.actor;
                var building = (Building)actor.CurJob.targetA.Thing;
                var building_TurretGun = building as AERIALSystem;
                if (!GunNeedsLoading(building))
                    //this.JumpToToil(gotoTurret);
                {
                    return;
                }

                Thing thing = FindAmmo(pawn, building_TurretGun);
                if (thing == null)
                {
                    if (actor.Faction == Faction.OfPlayer)
                    {
                        Messages.Message(
                            "MessageOutOfNearbyShellsFor".Translate(actor.LabelShort, building_TurretGun.Label,
                                actor.Named("PAWN"), building_TurretGun.Named("GUN")).CapitalizeFirst(),
                            building_TurretGun, MessageTypeDefOf.NegativeEvent);
                    }

                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                }

                actor.CurJob.targetB = thing;
                actor.CurJob.count = 1;
            };
            yield return loadIfNeeded;
            yield return Toils_Reserve.Reserve(TargetIndex.B, 10, 1);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return new Toil
            {
                initAction = delegate
                {
                    Pawn actor = loadIfNeeded.actor;
                    var building_TurretGun = (Building)actor.CurJob.targetA.Thing as AERIALSystem;

                    building_TurretGun.gun.TryGetComp<AERIALChangeableProjectile>()
                        .NewLoadShell(actor.CurJob.targetB.Thing.def, 1);
                    actor.carryTracker.innerContainer.ClearAndDestroyContents();
                }
            };
            //yield return gotoTurret;
        }
    }
}
