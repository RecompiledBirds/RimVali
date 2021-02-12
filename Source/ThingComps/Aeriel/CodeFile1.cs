using RimWorld;
using Verse;
using System;
using Verse.AI;
using System.Collections.Generic;

namespace AvaliMod
{
	public class AerielProps : CompProperties
	{
		// Token: 0x06005547 RID: 21831 RVA: 0x0003B191 File Offset: 0x00039391
		public AerielProps()
		{
			this.compClass = typeof(AerialTurretComp);
		}
	}
	public class AerialTurretComp : ThingComp
	{
		public AerielProps Props
		{
			get
			{
				return (AerielProps)this.props;
			}
		}
		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
		{
			if (!pawn.RaceProps.ToolUser)
			{
				yield break;
			}
			if (!pawn.CanReserveAndReach(this.parent, PathEndMode.InteractionCell, Danger.Deadly, 1, -1, null, false))
			{
				yield break;
			}
			FloatMenuOption floatMenuOption = new FloatMenuOption("RefuelThing".Translate(this.parent.LabelShort, this.parent), delegate ()
			{
				Job job = JobMaker.MakeJob(AvaliDefs.refuelAeriel, this.parent);
				pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}, MenuOptionPriority.Default, null, null, 0f, null, null);
			yield return floatMenuOption;
			yield break;
		}
	}

	public class RefuelAerielJobDriver : JobDriver
	{

		private static bool GunNeedsLoading(Building b)
		{
			AERIALSYSTEM building_TurretGun = b as AERIALSYSTEM;
			if (building_TurretGun == null)
			{
				return false;
			}
			CompChangeableProjectile compChangeableProjectile = building_TurretGun.gun.TryGetComp<CompChangeableProjectile>();
			return compChangeableProjectile != null && !compChangeableProjectile.Loaded;
		}
		public static Thing findAmmo(Pawn pawn, AERIALSYSTEM aeriel)
		{
			StorageSettings allowed = pawn.IsColonist ? aeriel.gun.TryGetComp<CompChangeableProjectile>().allowedShellsSettings : null;
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

					building_TurretGun.gun.TryGetComp<CompChangeableProjectile>().LoadShell(actor.CurJob.targetB.Thing.def, 1);
					actor.carryTracker.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
				}
			};
			//yield return gotoTurret;
		}
	}
}