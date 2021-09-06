using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace AvaliMod
{
    public class AERIALChangable : CompProperties
    {
        public int maxShellCount = 6;

        public AERIALChangable()
        {
            compClass = typeof(AERIALChangeableProjectile);
        }
    }

    public class AERIALChangeableProjectile : CompChangeableProjectile
    {
        private bool hasSetup = false;
        public int maxShells;

        public new AERIALChangable Props => (AERIALChangable)props;

        public new ThingDef Projectile
        {
            get
            {
                if (!Loaded)
                {
                    return null;
                }
                return loadedShells[loadedShells.Count - 1].projectileWhenLoaded;
            }
        }

        public new bool Loaded => loadedShells.Count > 0;

        public new bool StorageTabVisible => true;

        public override void PostExposeData()
        {
            //Scribe_Defs.Look<ThingDef>(ref this.loadedShell, "loadedShell");
            Scribe_Values.Look(ref loadedCount, "loadedCount", 0, false);
            Scribe_Deep.Look(ref allowedShellsSettings, "allowedShellsSettings", Array.Empty<object>());
            Scribe_Collections.Look(ref loadedShells, "loadedShells");
            if (loadedShells == null)
            {
                loadedShells = new List<ThingDef>();
            }
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            allowedShellsSettings = new StorageSettings(this);
            if (parent.def.building.defaultStorageSettings != null)
            {
                allowedShellsSettings.CopyFrom(parent.def.building.defaultStorageSettings);
            }
        }

        public override void CompTick()
        {
            if (!hasSetup)
            {
                maxShells = Props.maxShellCount;
                if (parent.def == AvaliDefs.Aerial)
                {
                    maxShells = RimValiMod.settings.AERIALShellCap;
                }
                hasSetup = true;
            }
            base.CompTick();
        }

        public override void Notify_ProjectileLaunched()
        {
            if (loadedCount > 0)
            { loadedCount--; }
            if (loadedCount <= 0)
            {
                loadedShells[loadedShells.Count - 1] = null;
            }
        }

        public List<ThingDef> loadedShells = new List<ThingDef>();

        public void NewLoadShell(ThingDef shell, int _)
        {
            loadedShells.Add(shell);
        }

        public Thing NewRemoveShell()
        {
            Thing thing = ThingMaker.MakeThing(loadedShells[loadedShells.Count - 1], null);
            thing.stackCount = 1;
            loadedShells.RemoveAt(loadedShells.Count - 1);
            return thing;
        }

        public new StorageSettings GetStoreSettings()
        {
            return allowedShellsSettings;
        }

        public new StorageSettings GetParentStoreSettings()
        {
            return parent.def.building.fixedStorageSettings;
        }

        public new int loadedCount;

        public new StorageSettings allowedShellsSettings;
    }

    public class AerielProps : CompProperties
    {
        public AerielProps()
        {
            compClass = typeof(AerialTurretComp);
        }
    }

    public class AerialTurretComp : ThingComp
    {
        public AerielProps Props => (AerielProps)props;

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
        {
            if (!pawn.RaceProps.ToolUser)
            {
                yield break;
            }
            if (!pawn.CanReserveAndReach(parent, PathEndMode.InteractionCell, Danger.Deadly, 1, -1, null, false))
            {
                yield break;
            }
            FloatMenuOption floatMenuOption = new FloatMenuOption("RefuelThing".Translate(parent.LabelShort, parent), delegate ()
            {
                Job job = JobMaker.MakeJob(AvaliDefs.refuelAeriel, parent);
                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }, MenuOptionPriority.Default, null, null, 0f, null, null);
            yield return floatMenuOption;
            yield break;
        }
    }

    public class RefuelAerielJobDriver : JobDriver
    {
        private static bool GunNeedsLoading(Building building)
        {
            if (!(building is AERIALSYSTEM building_TurretGun))
            {
                return false;
            }
            AERIALChangeableProjectile compChangeableProjectile = building_TurretGun.gun.TryGetComp<AERIALChangeableProjectile>();
            return compChangeableProjectile != null && !(compChangeableProjectile.loadedShells.Count >= RimValiMod.settings.AERIALShellCap);
        }

        public static Thing findAmmo(Pawn pawn, AERIALSYSTEM aeriel)
        {
            StorageSettings allowed = pawn.IsColonist ? aeriel.gun.TryGetComp<AERIALChangeableProjectile>().allowedShellsSettings : null;
            bool validator(Thing t) => !t.IsForbidden(pawn) && pawn.CanReserve(t, 10, 1, null, false) && (allowed == null || allowed.AllowedToAccept(t));
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Shell), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 40f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            Toil aerialGogo = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil loadIfNeeded = new Toil();
            loadIfNeeded.initAction = delegate ()
            {
                Pawn actor = loadIfNeeded.actor;
                Building building = (Building)actor.CurJob.targetA.Thing;
                AERIALSYSTEM building_TurretGun = building as AERIALSYSTEM;
                if (!GunNeedsLoading(building))
                {
                    //this.JumpToToil(gotoTurret);
                    return;
                }
                Thing thing = findAmmo(pawn, building_TurretGun);
                if (thing == null)
                {
                    if (actor.Faction == Faction.OfPlayer)
                    {
                        Messages.Message("MessageOutOfNearbyShellsFor".Translate(actor.LabelShort, building_TurretGun.Label, actor.Named("PAWN"), building_TurretGun.Named("GUN")).CapitalizeFirst(), building_TurretGun, MessageTypeDefOf.NegativeEvent, true);
                    }
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable, true, true);
                }
                actor.CurJob.targetB = thing;
                actor.CurJob.count = 1;
            };
            yield return loadIfNeeded;
            yield return Toils_Reserve.Reserve(TargetIndex.B, 10, 1, null);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false, false);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    Pawn actor = loadIfNeeded.actor;
                    AERIALSYSTEM building_TurretGun = ((Building)actor.CurJob.targetA.Thing) as AERIALSYSTEM;

                    building_TurretGun.gun.TryGetComp<AERIALChangeableProjectile>().NewLoadShell(actor.CurJob.targetB.Thing.def, 1);
                    actor.carryTracker.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
                }
            };
            //yield return gotoTurret;
        }
    }
}