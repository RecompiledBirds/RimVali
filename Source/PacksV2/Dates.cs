using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Rimvali.Rewrite.Packs
{
    public class Date : IExposable
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

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref ticks, "ticks", ticks, true);
            Scribe_Values.Look(ref day, "Day", day, true);
            Scribe_Values.Look(ref quadrum, "Quadrum", quadrum, true);
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

        public string GetFormattedDate
        {
            get
            {
                return $"{day}/{quadrum}";
            }
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
        /// <summary>
        /// FOR USE WITH LOADING. DO NOT USE ELSEWHERE.
        /// </summary>
        public DeathDate() { }
        public override void ExposeData()
        {
            Scribe_References.Look(ref deadPawn, "dPawn");
            base.ExposeData();
        }
    }
}
