using System;
using System.Collections.Generic;
using System.Linq;
using RimValiCore;
using RimWorld;
using Verse;

namespace AvaliMod
{
    public class AvaliPackHediffCompProps : HediffCompProperties
    {
        public AvaliPackSkillDef associatedDef;

        public AvaliPackHediffCompProps()
        {
            compClass = typeof(AvaliPackHediffComp);
        }
    }

    public class AvaliPackHediffComp : HediffComp
    {
        // FIXME: tick never gets incremented
        private readonly int tick = 0;

        public AvaliPackHediffCompProps Props => (AvaliPackHediffCompProps)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (tick == 120)
            {
                if (RimValiUtility.Driver.GetCurrentPack(parent.pawn) != null)
                {
                    AvaliPack pack = RimValiUtility.Driver.GetCurrentPack(parent.pawn);
                    if (pack.GetPackSkillDef() != null && pack.GetPackSkillDef() != Props.associatedDef)
                    {
                        parent.pawn.health.RemoveHediff(parent);
                    }
                }
            }

            base.CompPostTick(ref severityAdjustment);
        }
    }

    public class AvaliPackSkillDef : Def
    {
        public List<string> effectList = new List<string>();

        public HediffDef hediffEffectApplied;
        public SkillDef skill;
        public string specialityLabel = "Unlabeled";
    }

    public class AvaliPack : Thing, ILoadReferenceable
    {
        public static List<SkillDef> avoidSkills = new List<SkillDef>();
        public Date creationDate = new Date();
        public List<DeathDate> deathDates = new List<DeathDate>();
        public Faction faction;
        public Pawn leaderPawn;
        public string name = "NoName";
        public HashSet<Pawn> pawns = new HashSet<Pawn>();

        public AvaliPack()
        {
        }

        public AvaliPack(Pawn leader)
        {
            leaderPawn = leader;
            faction = leader.Faction;
            name = leader.Name.ToStringShort + "'s pack";
            pawns.Add(leader);
        }

        public HashSet<Pawn> GetAllNonNullPawns
        {
            get { return pawns.Where(pawn => pawn != null).ToHashSet(); }
        }


        public new string GetUniqueLoadID()
        {
            return "pack_" + GetHashCode();
        }


        public override void ExposeData()
        {
            Scribe_References.Look(ref leaderPawn, "leaderPawn");
            Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
            Scribe_Values.Look(ref name, "packName", "NoName", true);
            Scribe_References.Look(ref faction, "faction");
            Scribe_Collections.Look(ref deathDates, "deathDates", LookMode.Deep, Array.Empty<object>());
            Scribe_Deep.Look(ref creationDate, "cDate");
        }

        public void UpdateHediffForAllMembers()
        {
            AvaliPackSkillDef packSkillDef = GetPackSkillDef();
            if (RimValiMod.settings.enableDebugMode)
            {
                Log.Message($"Def is: {packSkillDef.defName}");
            }

            if (packSkillDef?.hediffEffectApplied != null)
            {
                foreach (Pawn pawn in GetAllNonNullPawns.Where(p => p.Alive() && p.Spawned))
                {
                    if (!pawn.health.hediffSet.HasHediff(packSkillDef.hediffEffectApplied))
                    {
                        pawn.health.AddHediff(packSkillDef.hediffEffectApplied);
                        if (RimValiMod.settings.enableDebugMode)
                        {
                            Log.Message($"Added def: {packSkillDef.hediffEffectApplied.defName}");
                        }
                    }

                    foreach (HediffDef hediffDef in DefDatabase<HediffDef>.AllDefs.Where(hediffEffect =>
                                 hediffEffect != packSkillDef.hediffEffectApplied &&
                                 DefDatabase<AvaliPackSkillDef>.AllDefs.Any(
                                     otherPackSkillDef =>
                                         otherPackSkillDef != packSkillDef &&
                                         otherPackSkillDef.hediffEffectApplied == hediffEffect &&
                                         pawn.health.hediffSet.HasHediff(otherPackSkillDef.hediffEffectApplied))))
                    {
                        pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef));
                        if (RimValiMod.settings.enableDebugMode)
                        {
                            Log.Message($"Removed def: {hediffDef.defName}");
                        }
                    }
                }
            }
        }


        public AvaliPackSkillDef GetPackSkillDef()
        {
            if (!DefDatabase<AvaliPackSkillDef>.AllDefs.Any() || GetAllNonNullPawns.Count <= 1)
            {
                return null;
            }

            var prevSkills = new List<SkillDef>(avoidSkills);
            SkillDef packSkill = SkillDefOf.Intellectual;


            while (DefDatabase<AvaliPackSkillDef>.AllDefs.All(packSkillDef => packSkillDef.skill != packSkill))
            {
                avoidSkills.Add(packSkill);
                SkillRecord record = RimValiUtility.GetHighestSkillOfpack(this, avoidSkills);
                packSkill = record?.def ?? SkillDefOf.Intellectual;
            }

            avoidSkills = prevSkills;
            return DefDatabase<AvaliPackSkillDef>.AllDefs.ToList()
                .Find(packSkillDef => packSkillDef.skill == packSkill);
        }
    }

    public class Date : Thing, ILoadReferenceable, IExposable
    {
        public long date = 0;
        public int day;
        public Quadrum quadrum = Quadrum.Undefined;
        public int ticks;

        public Date()
        {
            try
            {
                GetCurrentDate();
            }
            catch
            {
                Log.Message("Not on a map yet!");
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref ticks, "ticks", ticks, true);
            Scribe_Values.Look(ref day, "Day", day, true);
            Scribe_Values.Look(ref quadrum, "Quadrum", quadrum, true);
        }

        public new string GetUniqueLoadID()
        {
            return "date_" + GetHashCode() + ticks;
        }

        public void GetCurrentDate()
        {
            day = GenDate.DayOfYear(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile).x);
            quadrum = GenDate.Quadrum(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile).x);
            ticks = GenTicks.TicksGame;
        }


        public override string ToString()
        {
            return day + quadrum.ToString();
        }
    }

    public class DeathDate : Date
    {
        public Pawn deadPawn;

        public DeathDate(Pawn pawn)
        {
            day = GenDate.DayOfYear(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile).x);
            quadrum = GenDate.Quadrum(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile).x);
            if (pawn != null)
            {
                deadPawn = pawn;
            }
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref deadPawn, "dPawn");
            base.ExposeData();
        }
    }
}
