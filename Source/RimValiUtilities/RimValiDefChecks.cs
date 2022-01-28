using System.Collections.Generic;
using System.Linq;
using RimValiCore.RVR;
using Verse;

namespace AvaliMod
{
    public static class RimValiDefChecks
    {
        public static List<ThingDef> potentialRaces;

        public static IEnumerable<RimValiRaceDef> races = DefDatabase<RimValiRaceDef>.AllDefs;

        public static void Initalize()
        {
            potentialRaces = DefDatabase<ThingDef>.AllDefs.Where(x =>
                    x.race != null && x.comps.Any(comp => comp.compClass == typeof(PackComp))).ToList();
        }

        public static List<ThingDef> PotentialPackRaces
        {
            get
            {
                return potentialRaces;
            }
        }
    }
}
