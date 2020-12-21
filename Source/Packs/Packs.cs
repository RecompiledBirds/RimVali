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
        public int size = 1;
        public Faction faction = null;
        public List<DeathDate> deathDates = new List<DeathDate>();
        public Date creationDate = new Date();

       public override void ExposeData()
        {
            Scribe_Collections.Look<Pawn>(ref pawns, "pawns", LookMode.Reference);
            Scribe_Values.Look(ref name, "packName", "NoName", true);
            Scribe_References.Look<Faction>(ref faction, "faction", false);
            Scribe_Collections.Look<DeathDate>(ref deathDates, "deathDates", LookMode.Deep, Array.Empty<object>());
            Scribe_Deep.Look<Date>(ref creationDate, "cDate", new Date());
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
            return "pack_" +this.GetHashCode().ToString();
        }

    }
    public class Date : Thing, ILoadReferenceable, IExposable
    {
        public int day;
        public Quadrum quadrum;
        public Date()
        {
            try
            {
                this.day = GenDate.DayOfYear(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile).x);
                this.quadrum = GenDate.Quadrum(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile).x);
            }
            catch (Exception e)
            {
                Log.ErrorOnce("Failure while creating date:" +e.Message, 1);
            }

        }

#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public string GetUniqueLoadID()
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
        {
            return "date_" + this.GetHashCode().ToString();
        }
        public override void ExposeData()
        {
            try
            {
                Scribe_Values.Look(ref day, "Day", this.day, true);
                Scribe_Values.Look(ref quadrum, "Quadrum", this.quadrum, true);
            }catch(Exception e)
            {
                Log.ErrorOnce(e.Message,1);
            }
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
            this.deadPawn = pawn;
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref deadPawn, "dPawn", false);
            base.ExposeData();
        }
    }
}