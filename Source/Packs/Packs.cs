using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AvaliMod
{
    public class AvaliPack : IExposable
    {
        public string name;
        public List<Pawn> pawns;
        public int size;
        public List<Date> deathDates;
       public void ExposeData()
        {
            Scribe_Values.Look(ref name, "packName", "NoName", true);
            Scribe_Values.Look(ref pawns, "pawns", null, true);
            Scribe_Values.Look(ref size, "size", 1, true);
            Scribe_Values.Look(ref deathDates, "deathDates", null, true);
        }
    }

    public class Date
    {
        public int day;
        public Pawn deadPawn;
        public Quadrum quadrum;
    }
}