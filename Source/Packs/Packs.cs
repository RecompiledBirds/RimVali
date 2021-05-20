using RimWorld;
using Verse;
using System.Collections.Generic;
using System;
using System.Linq;

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
                if (parent.pawn.GetPack() != null)
                {
                    AvaliPack pack = parent.pawn.GetPack();
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
        public static List<SkillDef> avoidSkills = new List<SkillDef>();
        public string name = "NoName";
        public List<Pawn> pawns = new List<Pawn>();
        public Faction faction = null;
        public List<DeathDate> deathDates = new List<DeathDate>();
        public Date creationDate = new Date();

        
        public void UpdateHediffForAllMembers()
        {
            AvaliPackSkillDef def = GetPackSkillDef();
            if (RimValiMod.settings.enableDebugMode)
            {
                Log.Message($"Def is: {def.defName}");
            }
            if (def != null && def.hediffEffectApplied != null)
            {
                foreach (Pawn pawn in pawns.Where(p => !p.Dead))
                {
                    if (!pawn.health.hediffSet.HasHediff(def.hediffEffectApplied))
                    {
                        pawn.health.AddHediff(def.hediffEffectApplied);
                        if (RimValiMod.settings.enableDebugMode)
                        {
                            Log.Message($"Added def: {def.hediffEffectApplied.defName}");
                        }
                    }

                    foreach (HediffDef hDef in DefDatabase<HediffDef>.AllDefs.Where(h=> h != def.hediffEffectApplied && DefDatabase<AvaliPackSkillDef>.AllDefs.Any(APSD=>APSD != def && APSD.hediffEffectApplied == h && pawn.health.hediffSet.HasHediff(APSD.hediffEffectApplied)))){
                        pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(hDef));
                        if (RimValiMod.settings.enableDebugMode)
                        {
                            Log.Message($"Removed def: {hDef.defName}");
                        }
                    }
                }
                
            }
           
        }


        
        public AvaliPackSkillDef GetPackSkillDef()
        {
            if (DefDatabase<AvaliPackSkillDef>.AllDefs.Count() > 0)
            {
                List<SkillDef> prevSkills = new List<SkillDef>();
                prevSkills.AddRange(avoidSkills);
                SkillRecord record = RimValiUtility.GetHighestSkillOfpack(this, avoidSkills);
                SkillDef def = record.def;

                while (!DefDatabase<AvaliPackSkillDef>.AllDefs.Any(APSD => APSD.skill == def))
                {
                    avoidSkills.Add(def);
                    record = RimValiUtility.GetHighestSkillOfpack(this, avoidSkills);
                    def = record.def;
                    if (def == null)
                    {
                        def = SkillDefOf.Intellectual;
                    }
                }
                avoidSkills = prevSkills;
                return DefDatabase<AvaliPackSkillDef>.AllDefs.ToList().Find(APSD => APSD.skill == def);
            }
            return null;
        }


       public override void ExposeData()
        {
            Scribe_Collections.Look<Pawn>(ref pawns, "pawns", LookMode.Reference);
            Scribe_Values.Look(ref name, "packName", "NoName", true);
            Scribe_References.Look<Faction>(ref faction, "faction", false);
            Scribe_Collections.Look<DeathDate>(ref deathDates, "deathDates", LookMode.Deep, Array.Empty<object>());
            Scribe_Deep.Look<Date>(ref creationDate, "cDate");
        }

        public AvaliPack()
        {
        }
        public AvaliPack(Faction faction)
        {
            this.faction = faction;
        }
        
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public string GetUniqueLoadID()
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
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
            this.day = GenDate.DayOfYear(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile).x);
            this.quadrum = GenDate.Quadrum(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile).x);
            this.ticks = GenTicks.TicksGame;
            
        }

#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public string GetUniqueLoadID()
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
        {
            return "date_" + GetHashCode().ToString()+ticks.ToString();
        }
        
        public override void ExposeData()
        {
            Scribe_Values.Look<int>(ref ticks, "ticks", this.ticks, true);
            Scribe_Values.Look<int>(ref day, "Day", this.day, true);
            Scribe_Values.Look<Quadrum>(ref quadrum, "Quadrum", this.quadrum, true);
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