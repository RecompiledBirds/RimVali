using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AvaliMod
{
    public class AvaliPack : Thing, ILoadReferenceable, IExposable
    {

        public string name = "NoName";
        public List<Pawn> pawns = new List<Pawn>();
        public int size = 1;
        public Faction faction = null;
        public List<DeathDate> deathDates;
        public Date creationDate;


        public void ExposeData()
        {
            Scribe_Collections.Look<Pawn>(ref pawns, "pawns", LookMode.Reference);
            Scribe_Values.Look(ref name, "packName", "NoName", true);
            Scribe_Values.Look(ref size, "size", 1, true);
            Scribe_References.Look<Faction>(ref faction, "faction", false);
            Scribe_Collections.Look<DeathDate>(ref deathDates, "deathDates", LookMode.Reference, true);
            Scribe_Values.Look(ref creationDate, "creationDate", null, true);
        }
        public string GetUniqueLoadID()
        {
            return "pack_" +this.name.GetHashCode().ToString();
        }

    }
    public class Date : Thing, ILoadReferenceable, IExposable
    {
        public int day;
        public Quadrum quadrum;
    }
    public class DeathDate : Date
    {
        public Pawn deadPawn;
    }
}