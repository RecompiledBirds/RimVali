using RimValiCore.RVR;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AvaliMod
{




    [StaticConstructorOnStartup]
    public static class RimValiDefChecks
    {
        static RimValiDefChecks()
        {
           // setup();
        }

        public static List<ThingDef> PotentialPackRaces
        {
            get
            {
                if (potentialRaces == null) {
                    potentialRaces = DefDatabase<ThingDef>.AllDefs.Where(x => x.race != null && x.comps.Any(comp => comp.compClass == typeof(PackComp))).ToList();
                }
                return potentialRaces;
            }
        }
        public static List<ThingDef> potentialRaces;

        public static IEnumerable<RimValiRaceDef> races = DefDatabase<RimValiRaceDef>.AllDefs.Where(x => x is RimValiRaceDef);

    }
}