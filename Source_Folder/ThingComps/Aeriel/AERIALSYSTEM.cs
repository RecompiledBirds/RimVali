using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

// A lot of this is copied from decompiled Building_TurretGun, is that necessary?

namespace AvaliMod
{
    [StaticConstructorOnStartup]
    public class AERIALSYSTEM : Building_TurretGun
    {
        public override LocalTargetInfo CurrentTarget => currentTargetInt;

        private bool WarmingUp => burstWarmupTicksLeft > 0;

        public override Verb AttackVerb => GunCompEq.PrimaryVerb;

        private bool PlayerControlled => (Faction == Faction.OfPlayer || MannedByColonist) && !MannedByNonColonist;

        private bool CanSetForcedTarget => PlayerControlled;

        private bool CanToggleHoldFire => PlayerControlled;

        private bool IsMortar => true;

        private bool IsMortarOrProjectileFliesOverhead => AttackVerb.ProjectileFliesOverhead() || IsMortar;

        private bool CanExtractShell
        {
            get
            {
                if (!PlayerControlled)
                {
                    return false;
                }
                AERIALChangeableProjectile compChangeableProjectile = gun.TryGetComp<AERIALChangeableProjectile>();
                return compChangeableProjectile != null && compChangeableProjectile.Loaded;
            }
        }

        private bool MannedByColonist => mannableComp != null && mannableComp.ManningPawn != null && mannableComp.ManningPawn.Faction == Faction.OfPlayer;

        private bool MannedByNonColonist => mannableComp != null && mannableComp.ManningPawn != null && mannableComp.ManningPawn.Faction != Faction.OfPlayer;

        public AERIALSYSTEM()
        {
            top = new TurretTop(this);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            ResetCurrentTarget();
            Effecter effecter = progressBarEffecter;
            if (effecter == null)
            {
                return;
            }
            effecter.Cleanup();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref burstCooldownTicksLeft, "burstCooldownTicksLeft", 0, false);
            Scribe_Values.Look(ref burstWarmupTicksLeft, "burstWarmupTicksLeft", 0, false);
            Scribe_TargetInfo.Look(ref currentTargetInt, "currentTarget");
            Scribe_Values.Look(ref holdFire, "holdFire", false, false);
            Scribe_Deep.Look(ref gun, "gun", Array.Empty<object>());
            BackCompatibility.PostExposeData(this);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                UpdateGunVerbs();
            }
        }

        public override bool ClaimableBy(Faction by)
        {
            return base.ClaimableBy(by) && (mannableComp == null || mannableComp.ManningPawn == null) && (!Active || mannableComp != null) && (((dormantComp == null || dormantComp.Awake) && (initiatableComp == null || initiatableComp.Initiated)) || (powerComp != null && !powerComp.PowerOn));
        }

        public override void OrderAttack(LocalTargetInfo targ)
        {
            if (!targ.IsValid)
            {
                if (forcedTarget.IsValid)
                {
                    ResetForcedTarget();
                }
                return;
            }
            if ((targ.Cell - Position).LengthHorizontal < AttackVerb.verbProps.EffectiveMinRange(targ, this))
            {
                Messages.Message("MessageTargetBelowMinimumRange".Translate(), this, MessageTypeDefOf.RejectInput, false);
                return;
            }
            if ((targ.Cell - Position).LengthHorizontal > AttackVerb.verbProps.range)
            {
                Messages.Message("MessageTargetBeyondMaximumRange".Translate(), this, MessageTypeDefOf.RejectInput, false);
                return;
            }
            if (forcedTarget != targ)
            {
                forcedTarget = targ;
                if (burstCooldownTicksLeft <= 0)
                {
                    TryStartShootSomethingAERIAL(false);
                }
            }
            if (holdFire)
            {
                Messages.Message("MessageTurretWontFireBecauseHoldFire".Translate(def.label), this, MessageTypeDefOf.RejectInput, false);
            }
        }

        public override void Tick()
        {
            if (CanExtractShell && MannedByColonist)
            {
                AERIALChangeableProjectile compChangeableProjectile = gun.TryGetComp<AERIALChangeableProjectile>();
                if (!compChangeableProjectile.allowedShellsSettings.AllowedToAccept(compChangeableProjectile.loadedShells[compChangeableProjectile.loadedShells.Count - 1]))
                {
                    ExtractShell();
                }
            }
            if (forcedTarget.IsValid && !CanSetForcedTarget)
            {
                ResetForcedTarget();
            }
            if (!CanToggleHoldFire)
            {
                holdFire = false;
            }
            if (forcedTarget.ThingDestroyed)
            {
                ResetForcedTarget();
            }
            if (Active && !stunner.Stunned && Spawned)
            {
                GunCompEq.verbTracker.VerbsTick();
                if (AttackVerb.state != VerbState.Bursting)
                {
                    if (WarmingUp)
                    {
                        burstWarmupTicksLeft--;
                        if (burstWarmupTicksLeft == 0)
                        {
                            BeginBurst();
                        }
                    }
                    else
                    {
                        if (burstCooldownTicksLeft > 0)
                        {
                            burstCooldownTicksLeft--;
                            if (IsMortar)
                            {
                                if (progressBarEffecter == null)
                                {
                                    progressBarEffecter = EffecterDefOf.ProgressBar.Spawn();
                                }
                                progressBarEffecter.EffectTick(this, TargetInfo.Invalid);
                                MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBarEffecter.children[0]).mote;
                                mote.progress = 1f - Math.Max(burstCooldownTicksLeft, 0) / (float)BurstCooldownTime().SecondsToTicks();
                                mote.offsetZ = -0.8f;
                            }
                        }
                        if (burstCooldownTicksLeft <= 0 && this.IsHashIntervalTick(10))
                        {
                            TryStartShootSomethingAERIAL(true);
                        }
                    }
                    top.TurretTopTick();
                    return;
                }
            }
            else
            {
                ResetCurrentTarget();
            }
        }

        protected void TryStartShootSomethingAERIAL(bool canBeginBurstImmediately)
        {
            if (progressBarEffecter != null)
            {
                progressBarEffecter.Cleanup();
                progressBarEffecter = null;
            }
            if (!Spawned || (holdFire && CanToggleHoldFire) || (AttackVerb.ProjectileFliesOverhead() && Map.roofGrid.Roofed(Position)) || !AttackVerb.Available())
            {
                //Log.Message($"reset current targ. \n SPAWNED: {base.Spawned} \n HOLDING FIRE: {holdFire && CanToggleHoldFire}\n ROOFED: {AttackVerb.ProjectileFliesOverhead() && base.Map.roofGrid.Roofed(base.Position)} \n VERB AVALIBLE: {AttackVerb.Available()}");
                ResetCurrentTarget();
                return;
            }
            //Log.Message($"Is forced targ valid: {forcedTarget.IsValid}");
            if (forcedTarget.IsValid)
            {
                //Log.Message("forced target is valid");
                currentTargetInt = forcedTarget;
            }
            else
            {
                //Log.Message("trying to find new target");
                currentTargetInt = TryFindNewTarget();
            }
            if (currentTargetInt.IsValid)
            {
                //Log.Message("current target is valid");
                SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(Position, Map, false));
            }
            if (!currentTargetInt.IsValid)
            {
                //Log.Message("reset current target, invalid");
                ResetCurrentTarget();
                return;
            }

            if (def.building.turretBurstWarmupTime > 0f)
            {
                burstWarmupTicksLeft = def.building.turretBurstWarmupTime.SecondsToTicks();
                return;
            }

            if (canBeginBurstImmediately)
            {
                //Log.Message("beginBurst");
                BeginBurst();

                return;
            }
            gun.TryGetComp<AERIALChangeableProjectile>().loadedShells.RemoveAt(gun.TryGetComp<AERIALChangeableProjectile>().loadedShells.Count - 1);
            Log.Message($"{gun.TryGetComp<AERIALChangeableProjectile>().loadedShells.Count}");
            burstWarmupTicksLeft = 1;
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            string inspectString = "";
            AERIALChangeableProjectile compChangeableProjectile = gun.TryGetComp<AERIALChangeableProjectile>();
            if (!inspectString.NullOrEmpty())
            {
                stringBuilder.AppendLine(inspectString);
            }
            if (AttackVerb.verbProps.minRange > 0f)
            {
                stringBuilder.AppendLine("MinimumRange".Translate() + ": " + AttackVerb.verbProps.minRange.ToString("F0"));
            }

            stringBuilder.AppendLine("AERIALShellSpaceLeft".Translate($"{compChangeableProjectile.loadedShells.Count}/{RimValiMod.settings.AERIALShellCap}".Named("SPACE")));
            if (Spawned && IsMortarOrProjectileFliesOverhead && Position.Roofed(Map))
            {
                stringBuilder.AppendLine("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
            }
            else if (Spawned && burstCooldownTicksLeft > 0 && BurstCooldownTime() > 5f)
            {
                stringBuilder.AppendLine("CanFireIn".Translate() + ": " + burstCooldownTicksLeft.ToStringSecondsFromTicks());
            }

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

        public override void Draw()
        {
            top.DrawTurret(Vector3.zero, 0.0f);
            base.Draw();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (CanExtractShell)
            {
                AERIALChangeableProjectile compChangeableProjectile = gun.TryGetComp<AERIALChangeableProjectile>();
                yield return new Command_Action
                {
                    defaultLabel = "CommandExtractShell".Translate(),
                    defaultDesc = "CommandExtractShellDesc".Translate(),
                    icon = compChangeableProjectile.loadedShells[compChangeableProjectile.loadedShells.Count - 1].uiIcon,
                    iconAngle = compChangeableProjectile.loadedShells[compChangeableProjectile.loadedShells.Count - 1].uiIconAngle,
                    iconOffset = compChangeableProjectile.loadedShells[compChangeableProjectile.loadedShells.Count - 1].uiIconOffset,
                    iconDrawScale = GenUI.IconDrawScale(compChangeableProjectile.loadedShells[compChangeableProjectile.loadedShells.Count - 1]),
                    action = delegate ()
                    {
                        ExtractShell();
                    }
                };
            }
            AERIALChangeableProjectile compChangeableProjectile2 = gun.TryGetComp<AERIALChangeableProjectile>();
            if (compChangeableProjectile2 != null)
            {
                StorageSettings storeSettings = compChangeableProjectile2.GetStoreSettings();
                foreach (Gizmo gizmo2 in StorageSettingsClipboard.CopyPasteGizmosFor(storeSettings))
                {
                    yield return gizmo2;
                }
            }
            if (CanSetForcedTarget)
            {
                Command_VerbTarget command_VerbTarget = new Command_VerbTarget
                {
                    defaultLabel = "CommandSetForceAttackTarget".Translate(),
                    defaultDesc = "CommandSetForceAttackTargetDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack", true),
                    verb = AttackVerb,
                    hotKey = KeyBindingDefOf.Misc4,
                    drawRadius = false
                };
                if (Spawned && IsMortarOrProjectileFliesOverhead && Position.Roofed(Map))
                {
                    command_VerbTarget.Disable("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
                }

                yield return command_VerbTarget;
            }
            if (forcedTarget.IsValid)
            {
                Command_Action command_Action = new Command_Action
                {
                    defaultLabel = "CommandStopForceAttack".Translate(),
                    defaultDesc = "CommandStopForceAttackDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt", true),
                    action = delegate ()
                    {
                        ResetForcedTarget();
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
                    }
                };
                if (!forcedTarget.IsValid)
                {
                    command_Action.Disable("CommandStopAttackFailNotForceAttacking".Translate());
                }
                command_Action.hotKey = KeyBindingDefOf.Misc5;
                yield return command_Action;
            }
            if (CanToggleHoldFire)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "CommandHoldFire".Translate(),
                    defaultDesc = "CommandHoldFireDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/HoldFire", true),
                    hotKey = KeyBindingDefOf.Misc6,
                    toggleAction = delegate ()
                    {
                        holdFire = !holdFire;
                        if (holdFire)
                        {
                            ResetForcedTarget();
                        }
                    },
                    isActive = (() => holdFire)
                };
            }
            yield break;
        }

        private void ExtractShell()
        {
            GenPlace.TryPlaceThing(gun.TryGetComp<AERIALChangeableProjectile>().NewRemoveShell(), Position, Map, ThingPlaceMode.Near, null, null, default);
        }

        private void ResetForcedTarget()
        {
            forcedTarget = LocalTargetInfo.Invalid;
            burstWarmupTicksLeft = 0;
            if (burstCooldownTicksLeft <= 0)
            {
                TryStartShootSomething(false);
            }
        }

        private void ResetCurrentTarget()
        {
            currentTargetInt = LocalTargetInfo.Invalid;
            burstWarmupTicksLeft = 0;
        }

        private void UpdateGunVerbs()
        {
            List<Verb> allVerbs = gun.TryGetComp<CompEquippable>().AllVerbs;
            for (int i = 0; i < allVerbs.Count; i++)
            {
                Verb verb = allVerbs[i];
                verb.caster = this;
                verb.castCompleteCallback = new Action(BurstComplete);
            }
        }

        private bool holdFire;
    }
}