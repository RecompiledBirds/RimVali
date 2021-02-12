using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using AlienRace;
using HarmonyLib;
using AvaliMod;
using System;


namespace AvaliMod
{




    [StaticConstructorOnStartup]
    public static class RimValiDefChecks
    {
        static RimValiDefChecks()
        {
            setup();
        }
       public static void setup()
        {
            potentialRaces = DefDatabase<RimValiRaceDef>.AllDefs.Where(x => x.race != null).ToList();
        }
        public static List<ThingDef> potentialPackRaces = new List<ThingDef>();
        public static List<RimValiRaceDef> potentialRaces = new List<RimValiRaceDef>();
        
        public static IEnumerable<RimValiRaceDef> races = DefDatabase<RimValiRaceDef>.AllDefs.Where<RimValiRaceDef>(x => x is RimValiRaceDef);
       
    }
}