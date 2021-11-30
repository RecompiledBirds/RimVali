using RimWorld;
using Verse;
using System.Collections.Generic;
using System;
using System.Linq;
using RimValiCore;

namespace AvaliMod
{
    public class AvaliPackHediffCompProps : HediffCompProperties
    {
        public AvaliPackSkillDef associatedDef;

        public AvaliPackHediffCompProps()
        {
            this.compClass = typeof(AvaliPackHediffComp);
        }
    }

    public class AvaliPackHediffComp : HediffComp
    {
        public AvaliPackHediffCompProps Props
        {
            get
            {
                return (AvaliPackHediffCompProps)this.props;
            }
        }
        int tick = 0;
        
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
        public HediffDef hediffEffectApplied;
        public SkillDef skill;
        public string specialityLabel = "Unlabeled";
        public List<string> effectList = new List<string>();
    }
#pragma warning disable CS0114 // Member hides inherited member; missing override keyword
    public class AvaliPack : Thing, ILoadReferenceable, IExposable
    {
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

        public static List<SkillDef> avoidSkills = new List<SkillDef>();
        public string name = "NoName";
        public Pawn leaderPawn;
        public HashSet<Pawn> pawns = new HashSet<Pawn>();
        public Faction faction = null;
        public List<DeathDate> deathDates = new List<DeathDate>();
        public Date creationDate = new Date();

        public void CleanupPack(Pawn pawn)
        {
            if (GetAllNonNullPawns.Where(x => x == pawn).Count() > 1)
            {
                for (int a = 0; a < GetAllNonNullPawns.Where(x => x == pawn).Count() - 1; a++){pawns.Remove(GetAllNonNullPawns.Where(x => x == pawn).ToList()[a]);}
            }
        }

        public void UpdateHediffForAllMembers()
        {
            AvaliPackSkillDef def = GetPackSkillDef();
            if (RimValiMod.settings.enableDebugMode)
            {
                // Log.Message($"[RimVali FFA/Packs] UpdateHediffForAllMembers - Def is: {def.defName}");
            }
            if (def?.hediffEffectApplied != null)
            {
                foreach (Pawn pawn in GetAllNonNullPawns.Where(p => p.Alive()&&p.Spawned))
                {
                    if (!pawn.health.hediffSet.HasHediff(def.hediffEffectApplied))
                    {
                        pawn.health.AddHediff(def.hediffEffectApplied);
                        if (RimValiMod.settings.enableDebugMode){Log.Message($"Added def: {def.hediffEffectApplied.defName}");}
                    }

                    foreach (HediffDef hDef in DefDatabase<HediffDef>.AllDefs.Where(h=> h != def.hediffEffectApplied && DefDatabase<AvaliPackSkillDef>.AllDefs.Any(APSD=>APSD != def && APSD.hediffEffectApplied == h && pawn.health.hediffSet.HasHediff(APSD.hediffEffectApplied)))){
                        pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(hDef));
                        if (RimValiMod.settings.enableDebugMode){Log.Message($"Removed def: {hDef.defName}");}
                    }
                }
            }
        }

        public AvaliPackSkillDef GetPackSkillDef()
        {
            if (DefDatabase<AvaliPackSkillDef>.AllDefs.Count() > 0 && this != null && GetAllNonNullPawns.Count>1)
            {
                List<SkillDef> prevSkills = new List<SkillDef>();
                prevSkills.AddRange(avoidSkills);
                SkillRecord record;
                SkillDef def = SkillDefOf.Intellectual;

                while (!DefDatabase<AvaliPackSkillDef>.AllDefs.Any(APSD => APSD.skill == def))
                {
                    avoidSkills.Add(def);
                    record = RimValiUtility.GetHighestSkillOfpack(this, avoidSkills);
                    if (record != null && record.def != null)
                    {
                        def = record.def;
                    }
                    else
                    {
                        //defaulting if record is null
                        def = SkillDefOf.Intellectual;
                    }
                }
                avoidSkills = prevSkills;
                AvaliPackSkillDef avaliPackSkillDef = DefDatabase<AvaliPackSkillDef>.AllDefs.ToList().Find(APSD => APSD.skill == def);
                return avaliPackSkillDef;
            }
            return null;
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref leaderPawn, "leaderPawn");
            Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
            Scribe_Values.Look(ref name, "packName", "NoName", true);
            Scribe_References.Look(ref faction, "faction", false);
            Scribe_Collections.Look(ref deathDates, "deathDates", LookMode.Deep, Array.Empty<object>());
            Scribe_Deep.Look(ref creationDate, "cDate");
        }

        public HashSet<Pawn> GetAllNonNullPawns => pawns.Where(pawn => pawn != null).ToHashSet();

        new public string GetUniqueLoadID()
        {
            return "pack_" + this.GetHashCode().ToString();
        }
    }

    public class Date : Thing, ILoadReferenceable, IExposable
    {
        public long date = 0;
        public int day = 0;
        public int ticks = 0;
        public Quadrum quadrum = Quadrum.Undefined;

        public Date()
        {
            try { this.GetCurrentDate(); }
            catch { Log.Message("Not on a map yet!"); }
        }

        public void GetCurrentDate(bool forSaving = false)
        {
            day = GenDate.DayOfYear(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile).x);
            quadrum = GenDate.Quadrum(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile).x);
            ticks = GenTicks.TicksGame;
        }

        new public string GetUniqueLoadID()
        {
            return "date_" + GetHashCode().ToString()+ticks.ToString();
        }
        
        public override void ExposeData()
        {
            Scribe_Values.Look(ref ticks, "ticks", ticks, true);
            Scribe_Values.Look(ref day, "Day", day, true);
            Scribe_Values.Look(ref quadrum, "Quadrum", quadrum, true);
        }

        public override string ToString()
        {
            return day.ToString() + quadrum.ToString();
        }
    }

    public class DeathDate : Date
    {
        public Pawn deadPawn;

        public DeathDate(Pawn pawn)
        {
            this.day = GenDate.DayOfYear(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile).x);
            this.quadrum = GenDate.Quadrum(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile).x);
            if (pawn != null) { this.deadPawn = pawn; }
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref deadPawn, "dPawn", false);
            base.ExposeData();
        }
    }
}
