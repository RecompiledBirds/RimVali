using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;
using Verse.AI;
using Verse.Sound;

namespace AvaliMod
{

	[StaticConstructorOnStartup]
	public class AERIALSYSTEM : Building_TurretGun
	{


		// Token: 0x17001362 RID: 4962
		// (get) Token: 0x06007DBA RID: 32186 RVA: 0x0005473B File Offset: 0x0005293B
		public override LocalTargetInfo CurrentTarget
		{
			get
			{
				return this.currentTargetInt;
			}
		}

		// Token: 0x17001363 RID: 4963
		// (get) Token: 0x06007DBB RID: 32187 RVA: 0x00054743 File Offset: 0x00052943
		private bool WarmingUp
		{
			get
			{
				return this.burstWarmupTicksLeft > 0;
			}
		}

		// Token: 0x17001364 RID: 4964
		// (get) Token: 0x06007DBC RID: 32188 RVA: 0x0005474E File Offset: 0x0005294E
		public override Verb AttackVerb
		{
			get
			{

				return this.GunCompEq.PrimaryVerb;
			}
		}



		// Token: 0x17001366 RID: 4966
		// (get) Token: 0x06007DBE RID: 32190 RVA: 0x00054766 File Offset: 0x00052966
		private bool PlayerControlled
		{
			get
			{
				return (base.Faction == Faction.OfPlayer || this.MannedByColonist) && !this.MannedByNonColonist;
			}
		}

		// Token: 0x17001367 RID: 4967
		// (get) Token: 0x06007DBF RID: 32191 RVA: 0x00054788 File Offset: 0x00052988
		private bool CanSetForcedTarget
		{
			get
			{
				return this.PlayerControlled;
			}
		}

		// Token: 0x17001368 RID: 4968
		// (get) Token: 0x06007DC0 RID: 32192 RVA: 0x0005479A File Offset: 0x0005299A
		private bool CanToggleHoldFire
		{
			get
			{
				return this.PlayerControlled;
			}
		}

		// Token: 0x17001369 RID: 4969
		// (get) Token: 0x06007DC1 RID: 32193 RVA: 0x000547A2 File Offset: 0x000529A2
		private bool IsMortar
		{
			get
			{
				return true ;
			}
		}

		// Token: 0x1700136A RID: 4970
		// (get) Token: 0x06007DC2 RID: 32194 RVA: 0x000547B4 File Offset: 0x000529B4
		private bool IsMortarOrProjectileFliesOverhead
		{
			get
			{
				return this.AttackVerb.ProjectileFliesOverhead() || this.IsMortar;
			}
		}

		// Token: 0x1700136B RID: 4971
		// (get) Token: 0x06007DC3 RID: 32195 RVA: 0x0025808C File Offset: 0x0025628C
		private bool CanExtractShell
		{
			get
			{
				if (!this.PlayerControlled)
				{
					return false;
				}
				AERIALChangeableProjectile compChangeableProjectile = this.gun.TryGetComp<AERIALChangeableProjectile>();
				return compChangeableProjectile != null && compChangeableProjectile.Loaded;
			}
		}

		// Token: 0x1700136C RID: 4972
		// (get) Token: 0x06007DC4 RID: 32196 RVA: 0x000547CB File Offset: 0x000529CB
		private bool MannedByColonist
		{
			get
			{
				return this.mannableComp != null && this.mannableComp.ManningPawn != null && this.mannableComp.ManningPawn.Faction == Faction.OfPlayer;
			}
		}

		// Token: 0x1700136D RID: 4973
		// (get) Token: 0x06007DC5 RID: 32197 RVA: 0x000547FB File Offset: 0x000529FB
		private bool MannedByNonColonist
		{
			get
			{
				return this.mannableComp != null && this.mannableComp.ManningPawn != null && this.mannableComp.ManningPawn.Faction != Faction.OfPlayer;
			}
		}

		// Token: 0x06007DC6 RID: 32198 RVA: 0x0005482E File Offset: 0x00052A2E
		public AERIALSYSTEM()
		{
			this.top = new TurretTop(this);
		}


		// Token: 0x06007DC9 RID: 32201 RVA: 0x0005485B File Offset: 0x00052A5B
		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			base.DeSpawn(mode);
			this.ResetCurrentTarget();
			Effecter effecter = this.progressBarEffecter;
			if (effecter == null)
			{
				return;
			}
			effecter.Cleanup();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.burstCooldownTicksLeft, "burstCooldownTicksLeft", 0, false);
			Scribe_Values.Look<int>(ref this.burstWarmupTicksLeft, "burstWarmupTicksLeft", 0, false);
			Scribe_TargetInfo.Look(ref this.currentTargetInt, "currentTarget");
			Scribe_Values.Look<bool>(ref this.holdFire, "holdFire", false, false);
			Scribe_Deep.Look<Thing>(ref this.gun, "gun", Array.Empty<object>());
			BackCompatibility.PostExposeData(this);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.UpdateGunVerbs();
			}
		}

		public override bool ClaimableBy(Faction by)
		{
			return base.ClaimableBy(by) && (this.mannableComp == null || this.mannableComp.ManningPawn == null) && (!this.Active || this.mannableComp != null) && (((this.dormantComp == null || this.dormantComp.Awake) && (this.initiatableComp == null || this.initiatableComp.Initiated)) || (this.powerComp != null && !this.powerComp.PowerOn));
		}
		public override void OrderAttack(LocalTargetInfo targ)
		{
			if (!targ.IsValid)
			{
				if (this.forcedTarget.IsValid)
				{
					this.ResetForcedTarget();
				}
				return;
			}
			if ((targ.Cell - base.Position).LengthHorizontal < this.AttackVerb.verbProps.EffectiveMinRange(targ, this))
			{
				Messages.Message("MessageTargetBelowMinimumRange".Translate(), this, MessageTypeDefOf.RejectInput, false);
				return;
			}
			if ((targ.Cell - base.Position).LengthHorizontal > this.AttackVerb.verbProps.range)
			{
				Messages.Message("MessageTargetBeyondMaximumRange".Translate(), this, MessageTypeDefOf.RejectInput, false);
				return;
			}
			if (this.forcedTarget != targ)
			{
				this.forcedTarget = targ;
				if (this.burstCooldownTicksLeft <= 0)
				{
					this.TryStartShootSomething(false);
				}
			}
			if (this.holdFire)
			{
				Messages.Message("MessageTurretWontFireBecauseHoldFire".Translate(this.def.label), this, MessageTypeDefOf.RejectInput, false);
			}
		}


		public override void Tick()
		{
			base.Tick();
			if (this.CanExtractShell && this.MannedByColonist)
			{
				AERIALChangeableProjectile compChangeableProjectile = this.gun.TryGetComp<AERIALChangeableProjectile>();
				if (!compChangeableProjectile.allowedShellsSettings.AllowedToAccept(compChangeableProjectile.loadedShells[compChangeableProjectile.loadedShells.Count-1]))
				{
					this.ExtractShell();
				}
			}
			if (this.forcedTarget.IsValid && !this.CanSetForcedTarget)
			{
				Log.Message("targ reset");
				this.ResetForcedTarget();
			}
			if (!this.CanToggleHoldFire)
			{
				this.holdFire = false;
			}
			if (this.forcedTarget.ThingDestroyed)
			{
				Log.Message("targ destroyed");
				this.ResetForcedTarget();
			}
			if (this.Active  && !this.stunner.Stunned && base.Spawned)
			{
				this.GunCompEq.verbTracker.VerbsTick();
				if (this.AttackVerb.state != VerbState.Bursting)
				{
					if (this.WarmingUp)
					{
						Log.Message("warming up");
						this.burstWarmupTicksLeft--;
						if (this.burstWarmupTicksLeft == 0)
						{
							Log.Message("beginburst");
							this.BeginBurst();
						}
					}
					else
					{
						if (this.burstCooldownTicksLeft > 0)
						{
							this.burstCooldownTicksLeft--;
							if (this.IsMortar)
							{
								Log.Message("isMortar");
								if (this.progressBarEffecter == null)
								{
									this.progressBarEffecter = EffecterDefOf.ProgressBar.Spawn();
								}
								this.progressBarEffecter.EffectTick(this, TargetInfo.Invalid);
								MoteProgressBar mote = ((SubEffecter_ProgressBar)this.progressBarEffecter.children[0]).mote;
								mote.progress = 1f - (float)Math.Max(this.burstCooldownTicksLeft, 0) / (float)this.BurstCooldownTime().SecondsToTicks();
								mote.offsetZ = -0.8f;
							}
						}
						if (this.burstCooldownTicksLeft <= 0 && this.IsHashIntervalTick(10))
						{
							Log.Message("trystartshootsomething");
							this.TryStartShootSomethingAERIAL(true);
						}
					}
					this.top.TurretTopTick();
					return;
				}
			}
			else
			{
				Log.Message("resetcurrenttarget");
				this.ResetCurrentTarget();
			}
		}




		protected void TryStartShootSomethingAERIAL(bool canBeginBurstImmediately)
		{
			if (this.progressBarEffecter != null)
			{
				this.progressBarEffecter.Cleanup();
				this.progressBarEffecter = null;
			}
			if (!base.Spawned || (this.holdFire && this.CanToggleHoldFire) || (this.AttackVerb.ProjectileFliesOverhead() && base.Map.roofGrid.Roofed(base.Position)) || !this.AttackVerb.Available())
			{
				//Log.Message($"reset targ. \n SPAWNED: {base.Spawned} \n HOLDING FIRE: {holdFire && CanToggleHoldFire}\n ROOFED: {AttackVerb.ProjectileFliesOverhead() && base.Map.roofGrid.Roofed(base.Position)} \n VERB AVALIBLE: {AttackVerb.Available()}");
				this.ResetCurrentTarget();
				return;
			}
			bool isValid = this.currentTargetInt.IsValid;
			if (this.forcedTarget.IsValid)
			{
				this.currentTargetInt = this.forcedTarget;
			}
			else
			{
				this.currentTargetInt = this.TryFindNewTarget();
			}
			if (currentTargetInt.IsValid)		{
				SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
			}
			if (!this.currentTargetInt.IsValid)
			{
				Log.Message("reset targ, invalid");
				this.ResetCurrentTarget();
				return;
			}
			if (this.def.building.turretBurstWarmupTime > 0f)
			{
				this.burstWarmupTicksLeft = this.def.building.turretBurstWarmupTime.SecondsToTicks();
				return;
			}
			if (canBeginBurstImmediately)
			{
				Log.Message("beginBurst");
				this.BeginBurst();
				return;
			}
			this.burstWarmupTicksLeft = 1;
		}

		// Token: 0x06007DD5 RID: 32213 RVA: 0x002587E8 File Offset: 0x002569E8
		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			string inspectString = base.GetInspectString();
			if (!inspectString.NullOrEmpty())
			{
				stringBuilder.AppendLine(inspectString);
			}
			if (this.AttackVerb.verbProps.minRange > 0f)
			{
				stringBuilder.AppendLine("MinimumRange".Translate() + ": " + this.AttackVerb.verbProps.minRange.ToString("F0"));
			}
			if (base.Spawned && this.IsMortarOrProjectileFliesOverhead && base.Position.Roofed(base.Map))
			{
				stringBuilder.AppendLine("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
			}
			else if (base.Spawned && this.burstCooldownTicksLeft > 0 && this.BurstCooldownTime() > 5f)
			{
				stringBuilder.AppendLine("CanFireIn".Translate() + ": " + this.burstCooldownTicksLeft.ToStringSecondsFromTicks());
			}
			AERIALChangeableProjectile compChangeableProjectile = this.gun.TryGetComp<AERIALChangeableProjectile>();
			if (compChangeableProjectile != null)
			{
				if (compChangeableProjectile.Loaded)
				{
					stringBuilder.AppendLine("ShellLoaded".Translate(compChangeableProjectile.loadedShells[compChangeableProjectile.loadedShells.Count - 1].LabelCap, compChangeableProjectile.loadedShells[compChangeableProjectile.loadedShells.Count - 1]));
				}
				else
				{
					stringBuilder.AppendLine("ShellNotLoaded".Translate());
				}
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}

		// Token: 0x06007DD6 RID: 32214 RVA: 0x0005490D File Offset: 0x00052B0D
		public override void Draw()
		{
			this.top.DrawTurret();
			base.Draw();
		}

		// Token: 0x06007DD7 RID: 32215 RVA: 0x00258980 File Offset: 0x00256B80
		public override void DrawExtraSelectionOverlays()
		{
			float range = this.AttackVerb.verbProps.range;
			if (range < 90f)
			{
				GenDraw.DrawRadiusRing(base.Position, range);
			}
			float num = this.AttackVerb.verbProps.EffectiveMinRange(true);
			if (num < 90f && num > 0.1f)
			{
				GenDraw.DrawRadiusRing(base.Position, num);
			}
			if (this.WarmingUp)
			{
				int degreesWide = (int)((float)this.burstWarmupTicksLeft * 0.5f);
				GenDraw.DrawAimPie(this, this.CurrentTarget, degreesWide, (float)this.def.size.x * 0.5f);
			}
			if (this.forcedTarget.IsValid && (!this.forcedTarget.HasThing || this.forcedTarget.Thing.Spawned))
			{
				Vector3 vector;
				if (this.forcedTarget.HasThing)
				{
					vector = this.forcedTarget.Thing.TrueCenter();
				}
				else
				{
					vector = this.forcedTarget.Cell.ToVector3Shifted();
				}
				Vector3 a = this.TrueCenter();
				vector.y = AltitudeLayer.MetaOverlays.AltitudeFor();
				a.y = vector.y;
				GenDraw.DrawLineBetween(a, vector, Building_TurretGun.ForcedTargetLineMat);
			}
		}

		// Token: 0x06007DD8 RID: 32216 RVA: 0x00054920 File Offset: 0x00052B20
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			IEnumerator<Gizmo> enumerator = null;
			if (this.CanExtractShell)
			{
				AERIALChangeableProjectile compChangeableProjectile = this.gun.TryGetComp<AERIALChangeableProjectile>();
				yield return new Command_Action
				{
					defaultLabel = "CommandExtractShell".Translate(),
					defaultDesc = "CommandExtractShellDesc".Translate(),
					icon = compChangeableProjectile.loadedShells[compChangeableProjectile.loadedShells.Count - 1].uiIcon,
					iconAngle = compChangeableProjectile.loadedShells[compChangeableProjectile.loadedShells.Count-1].uiIconAngle,
					iconOffset = compChangeableProjectile.loadedShells[compChangeableProjectile.loadedShells.Count - 1].uiIconOffset,
					iconDrawScale = GenUI.IconDrawScale(compChangeableProjectile.loadedShells[compChangeableProjectile.loadedShells.Count - 1]),
					action = delegate ()  
					{
						this.ExtractShell();
					}
				};
			}
			AERIALChangeableProjectile compChangeableProjectile2 = this.gun.TryGetComp<AERIALChangeableProjectile>();
			if (compChangeableProjectile2 != null)
			{
				StorageSettings storeSettings = compChangeableProjectile2.GetStoreSettings();
				foreach (Gizmo gizmo2 in StorageSettingsClipboard.CopyPasteGizmosFor(storeSettings))
				{
					yield return gizmo2;
				}
				enumerator = null;
			}
			if (this.CanSetForcedTarget)
			{
				Command_VerbTarget command_VerbTarget = new Command_VerbTarget();
				command_VerbTarget.defaultLabel = "CommandSetForceAttackTarget".Translate();
				command_VerbTarget.defaultDesc = "CommandSetForceAttackTargetDesc".Translate();
				command_VerbTarget.icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack", true);
				command_VerbTarget.verb = this.AttackVerb;
				command_VerbTarget.hotKey = KeyBindingDefOf.Misc4;
				command_VerbTarget.drawRadius = false;
				if (base.Spawned && this.IsMortarOrProjectileFliesOverhead && base.Position.Roofed(base.Map))
				{
					command_VerbTarget.Disable("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
				}
				
				//testing something
				//this.forcedTarget = command_VerbTarget.verb.CurrentTarget;
				yield return command_VerbTarget;
			}
			if (this.forcedTarget.IsValid)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "CommandStopForceAttack".Translate();
				command_Action.defaultDesc = "CommandStopForceAttackDesc".Translate();
				command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt", true);
				command_Action.action = delegate ()
				{
					this.ResetForcedTarget();
					SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
				};
				if (!this.forcedTarget.IsValid)
				{
					command_Action.Disable("CommandStopAttackFailNotForceAttacking".Translate());
				}
				command_Action.hotKey = KeyBindingDefOf.Misc5;
				yield return command_Action;
			}
			if (this.CanToggleHoldFire)
			{
				yield return new Command_Toggle
				{
					defaultLabel = "CommandHoldFire".Translate(),
					defaultDesc = "CommandHoldFireDesc".Translate(),
					icon = ContentFinder<Texture2D>.Get("UI/Commands/HoldFire", true),
					hotKey = KeyBindingDefOf.Misc6,
					toggleAction = delegate ()
					{
						this.holdFire = !this.holdFire;
						if (this.holdFire)
						{
							this.ResetForcedTarget();
						}
					},
					isActive = (() => this.holdFire)
				};
			}
			yield break;
			yield break; 
		}

		// Token: 0x06007DD9 RID: 32217 RVA: 0x00258AB0 File Offset: 0x00256CB0
		private void ExtractShell()
		{
			GenPlace.TryPlaceThing(this.gun.TryGetComp<AERIALChangeableProjectile>().NewRemoveShell(), base.Position, base.Map, ThingPlaceMode.Near, null, null, default);
		}

		// Token: 0x06007DDA RID: 32218 RVA: 0x00054930 File Offset: 0x00052B30
		private void ResetForcedTarget()
		{
			this.forcedTarget = LocalTargetInfo.Invalid;
			this.burstWarmupTicksLeft = 0;
			if (this.burstCooldownTicksLeft <= 0)
			{
				this.TryStartShootSomething(false);
			}
		}

		// Token: 0x06007DDB RID: 32219 RVA: 0x00054954 File Offset: 0x00052B54
		private void ResetCurrentTarget()
		{
			this.currentTargetInt = LocalTargetInfo.Invalid;
			this.burstWarmupTicksLeft = 0;
		}


		// Token: 0x06007DDD RID: 32221 RVA: 0x00258AEC File Offset: 0x00256CEC
		private void UpdateGunVerbs()
		{
			List<Verb> allVerbs = this.gun.TryGetComp<CompEquippable>().AllVerbs;
			for (int i = 0; i < allVerbs.Count; i++)
			{
				Verb verb = allVerbs[i];
				verb.caster = this;
				verb.castCompleteCallback = new Action(this.BurstComplete);
			}
		}


		private bool holdFire;



	}
}
