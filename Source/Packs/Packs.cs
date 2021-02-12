using RimWorld;
using Verse;
using System.Collections.Generic;
using System;

namespace AvaliMod
{
#pragma warning disable CS0114 // Member hides inherited member; missing override keyword
    public class AvaliPack : Thing, ILoadReferenceable, IExposable
    {

        public string name = "NoName";
        public List<Pawn> pawns = new List<Pawn>();
        public Faction faction = null;
        public List<DeathDate> deathDates = new List<DeathDate>();
        public Date creationDate = new Date();

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