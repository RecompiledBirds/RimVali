using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
namespace AvaliMod
{
    [StaticConstructorOnStartup]
    public class RimvaliPotentialPackRaces
    {
        public static List<ThingDef> potentialPackRaces = new List<ThingDef>();
        public static IEnumerable<ThingDef> potentialRaces = DefDatabase<ThingDef>.AllDefs.Where<ThingDef>((x => x.race != null));
        public static void GetRaces()
        {
            foreach (ThingDef race in potentialRaces)
            {
                if (race.defName.ToLower().Contains("avali") && !potentialPackRaces.Contains(race))
                {

                    potentialPackRaces.Add(race);
                    Log.Message("Added possible avali race to packs: " + race.defName);
                }
            }
            Log.Message("RimVali found " + potentialPackRaces.Count.ToString() + " to add.");
        }
    }
    [StaticConstructorOnStartup]
    public class RimValiRelationsFound
    {
        public static IEnumerable<PawnRelationDef> relationsFound = DefDatabase<PawnRelationDef>.AllDefs;
        public static void GetRelations()
        {
            Log.Message("Rimvali found " + relationsFound.Count().ToString() + " relation defs.");
        }
    }
}