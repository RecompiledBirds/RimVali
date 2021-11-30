using System.Collections.Generic;
using Verse;

namespace AvaliMod
{
    public class ConflictsWithProps : HediffCompProperties
    {
        public List<HediffDef> conflictingHediffs;
        public bool showConflicts;
        public bool debugInfo;

        public ConflictsWithProps()
        {
            compClass = typeof(ConflictsWith);
        }
    }
}