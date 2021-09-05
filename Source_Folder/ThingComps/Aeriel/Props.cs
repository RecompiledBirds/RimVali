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
    // Token: 0x020017AB RID: 6059
    public class AERIALChangeableProjectile : CompChangeableProjectile
    {
        private bool hasSetup = false;
        public int maxShells;
        // Token: 0x170014BB RID: 5307
        // (get) Token: 0x060085F8 RID: 34296 RVA: 0x00059C88 File Offset: 0x00057E88
        public new AERIALChangable Props => (AERIALChangable)props;



        // Token: 0x170014BD RID: 5309
        // (get) Token: 0x060085FA RID: 34298 RVA: 0x00059CA8 File Offset: 0x00057EA8
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

        // Token: 0x170014BE RID: 5310
        // (get) Token: 0x060085FB RID: 34299 RVA: 0x00059CBF File Offset: 0x00057EBF
        public new bool Loaded => loadedShells.Count > 0;

        public new bool StorageTabVisible => true;

        public override void PostExposeData()
        {
            //Scribe_Defs.Look<ThingDef>(ref this.loadedShell, "loadedShell");
            Scribe_Values.Look<int>(ref loadedCount, "loadedCount", 0, false);
            Scribe_Deep.Look<StorageSettings>(ref allowedShellsSettings, "allowedShellsSettings", Array.Empty<object>());
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
        // Token: 0x060085FF RID: 34303 RVA: 0x00059D03 File Offset: 0x00057F03
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
        // Token: 0x06008602 RID: 34306 RVA: 0x00059D71 File Offset: 0x00057F71
        public new StorageSettings GetStoreSettings()
        {
            return allowedShellsSettings;
        }

        // Token: 0x06008603 RID: 34307 RVA: 0x00059D79 File Offset: 0x00057F79
        public new StorageSettings GetParentStoreSettings()
        {
            return parent.def.building.fixedStorageSettings;
        }

        // Token: 0x04005666 RID: 22118
        public new int loadedCount;

        // Token: 0x04005667 RID: 22119
        public new StorageSettings allowedShellsSettings;
    }
    public class AerielProps : CompProperties
    {
        // Token: 0x06005547 RID: 21831 RVA: 0x0003B191 File Offset: 0x00039391
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
            Predicate<Thing> validator = (Thing t) => !t.IsForbidden(pawn) && pawn.CanReserve(t, 10, 1, null, false) && (allowed == null || allowed.AllowedToAccept(t));
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