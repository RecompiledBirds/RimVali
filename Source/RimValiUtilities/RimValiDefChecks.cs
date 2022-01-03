using System.Collections.Generic;
using System.Linq;
using RimValiCore.RVR;
using Verse;

namespace AvaliMod
{
    [StaticConstructorOnStartup]
    public static class RimValiDefChecks
    {
        public static List<ThingDef> potentialRaces;

        public static IEnumerable<RimValiRaceDef> races = DefDatabase<RimValiRaceDef>.AllDefs;

        static RimValiDefChecks()
        {
        }

        public static List<ThingDef> PotentialPackRaces
        {
            get
            {
                if (potentialRaces != null)
                {
                    return potentialRaces;
                }

                return potentialRaces = DefDatabase<ThingDef>.AllDefs.Where(x =>
                    x.race != null && x.comps.Any(comp => comp.compClass == typeof(PackComp))).ToList();
            }
        }
    }
}
