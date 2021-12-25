using System.Collections.Generic;
using Verse;

namespace AvaliMod
{
    public class ConflictsWithProps : HediffCompProperties
    {
        public List<HediffDef> conflictingHediffs;
        public bool debugInfo;
        public bool showConflicts;

        public ConflictsWithProps()
        {
            compClass = typeof(ConflictsWith);
        }
    }
}
