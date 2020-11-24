using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using AlienRace;
namespace AvaliMod
{
    [StaticConstructorOnStartup]
    public class RimvaliPotentialPackRaces
    {
        public static List<AlienRace.ThingDef_AlienRace> potentialPackRaces = new List<AlienRace.ThingDef_AlienRace>();
        public static IEnumerable<AlienRace.ThingDef_AlienRace> potentialRaces = DefDatabase<AlienRace.ThingDef_AlienRace>.AllDefs.Where<AlienRace.ThingDef_AlienRace>((x => x.race != null));
        public static void GetRaces()
        {
            foreach (AlienRace.ThingDef_AlienRace race in potentialRaces)
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