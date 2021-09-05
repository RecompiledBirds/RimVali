using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AvaliMod
{

    [StaticConstructorOnStartup]
    public class AERIALSYSTEM : Building_TurretGun
    {


        // Token: 0x17001362 RID: 4962
        // (get) Token: 0x06007DBA RID: 32186 RVA: 0x0005473B File Offset: 0x0005293B
        public override LocalTargetInfo CurrentTarget => currentTargetInt;

        // Token: 0x17001363 RID: 4963
        // (get) Token: 0x06007DBB RID: 32187 RVA: 0x00054743 File Offset: 0x00052943
        private bool WarmingUp => burstWarmupTicksLeft > 0;

        // Token: 0x17001364 RID: 4964
        // (get) Token: 0x06007DBC RID: 32188 RVA: 0x0005474E File Offset: 0x0005294E
        public override Verb AttackVerb => GunCompEq.PrimaryVerb;



        // Token: 0x17001366 RID: 4966
        // (get) Token: 0x06007DBE RID: 32190 RVA: 0x00054766 File Offset: 0x00052966
        private bool PlayerControlled => (base.Faction == Faction.OfPlayer || MannedByColonist) && !MannedByNonColonist;

        // Token: 0x17001367 RID: 4967
        // (get) Token: 0x06007DBF RID: 32191 RVA: 0x00054788 File Offset: 0x00052988
        private bool CanSetForcedTarget => PlayerControlled;

        // Token: 0x17001368 RID: 4968
        // (get) Token: 0x06007DC0 RID: 32192 RVA: 0x0005479A File Offset: 0x0005299A
        private bool CanToggleHoldFire => PlayerControlled;

        // Token: 0x17001369 RID: 4969
        // (get) Token: 0x06007DC1 RID: 32193 RVA: 0x000547A2 File Offset: 0x000529A2
        private bool IsMortar => true;

        // Token: 0x1700136A RID: 4970
        // (get) Token: 0x06007DC2 RID: 32194 RVA: 0x000547B4 File Offset: 0x000529B4
        private bool IsMortarOrProjectileFliesOverhead => AttackVerb.ProjectileFliesOverhead() || IsMortar;

        // Token: 0x1700136B RID: 4971
        // (get) Token: 0x06007DC3 RID: 32195 RVA: 0x0025808C File Offset: 0x0025628C
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

        // Token: 0x1700136C RID: 4972
        // (get) Token: 0x06007DC4 RID: 32196 RVA: 0x000547CB File Offset: 0x000529CB
        private bool MannedByColonist => mannableComp != null && mannableComp.ManningPawn != null && mannableComp.ManningPawn.Faction == Faction.OfPlayer;

        // Token: 0x1700136D RID: 4973
        // (get) Token: 0x06007DC5 RID: 32197 RVA: 0x000547FB File Offset: 0x000529FB
        private bool MannedByNonColonist => mannableComp != null && mannableComp.ManningPawn != null && mannableComp.ManningPawn.Faction != Faction.OfPlayer;

        // Token: 0x06007DC6 RID: 32198 RVA: 0x0005482E File Offset: 0x00052A2E
        public AERIALSYSTEM()
        {
            top = new TurretTop(this);
        }


        // Token: 0x06007DC9 RID: 32201 RVA: 0x0005485B File Offset: 0x00052A5B
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
            Scribe_Values.Look<int>(ref burstCooldownTicksLeft, "burstCooldownTicksLeft", 0, false);
            Scribe_Values.Look<int>(ref burstWarmupTicksLeft, "burstWarmupTicksLeft", 0, false);
            Scribe_TargetInfo.Look(ref currentTargetInt, "currentTarget");
            Scribe_Values.Look<bool>(ref holdFire, "holdFire", false, false);
            Scribe_Deep.Look<Thing>(ref gun, "gun", Array.Empty<object>());
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
            if ((targ.Cell - base.Position).LengthHorizontal < AttackVerb.verbProps.EffectiveMinRange(targ, this))
            {
                Messages.Message("MessageTargetBelowMinimumRange".Translate(), this, MessageTypeDefOf.RejectInput, false);
                return;
            }
            if ((targ.Cell - base.Position).LengthHorizontal > AttackVerb.verbProps.range)
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
            if (Active && !stunner.Stunned && base.Spawned)
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
            if (!base.Spawned || (holdFire && CanToggleHoldFire) || (AttackVerb.ProjectileFliesOverhead() && base.Map.roofGrid.Roofed(base.Position)) || !AttackVerb.Available())
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
                SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
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

        // Token: 0x06007DD5 RID: 32213 RVA: 0x002587E8 File Offset: 0x002569E8
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
            if (base.Spawned && IsMortarOrProjectileFliesOverhead && base.Position.Roofed(base.Map))
            {
                stringBuilder.AppendLine("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
            }
            else if (base.Spawned && burstCooldownTicksLeft > 0 && BurstCooldownTime() > 5f)
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

        // Token: 0x06007DD6 RID: 32214 RVA: 0x0005490D File Offset: 0x00052B0D
        public override void Draw()
        {
            top.DrawTurret(Vector3.zero, 0.0f);
            base.Draw();
        }



        // Token: 0x06007DD8 RID: 32216 RVA: 0x00054920 File Offset: 0x00052B20
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
                if (base.Spawned && IsMortarOrProjectileFliesOverhead && base.Position.Roofed(base.Map))
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

        // Token: 0x06007DD9 RID: 32217 RVA: 0x00258AB0 File Offset: 0x00256CB0
        private void ExtractShell()
        {
            GenPlace.TryPlaceThing(gun.TryGetComp<AERIALChangeableProjectile>().NewRemoveShell(), base.Position, base.Map, ThingPlaceMode.Near, null, null, default);
        }

        // Token: 0x06007DDA RID: 32218 RVA: 0x00054930 File Offset: 0x00052B30
        private void ResetForcedTarget()
        {
            forcedTarget = LocalTargetInfo.Invalid;
            burstWarmupTicksLeft = 0;
            if (burstCooldownTicksLeft <= 0)
            {
                TryStartShootSomething(false);
            }
        }

        // Token: 0x06007DDB RID: 32219 RVA: 0x00054954 File Offset: 0x00052B54
        private void ResetCurrentTarget()
        {
            currentTargetInt = LocalTargetInfo.Invalid;
            burstWarmupTicksLeft = 0;
        }


        // Token: 0x06007DDD RID: 32221 RVA: 0x00258AEC File Offset: 0x00256CEC
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
