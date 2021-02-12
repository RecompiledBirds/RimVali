using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
namespace AvaliMod
{
    [StaticConstructorOnStartup]
    public static class Modulefinder
    {
        public static IEnumerable<ModContentPack> loadedMods = LoadedModManager.RunningMods.Where(x => x.Name.ToLower().Contains("rimvali"));
        public static IEnumerable<ModuleDef> moduleDefs = DefDatabase<ModuleDef>.AllDefs;
        public static void startup()
        {
            moduleDefs = DefDatabase<ModuleDef>.AllDefs;
            foreach (ModuleDef module in moduleDefs)
            {
                if (!RimValiUtility.modulesFound.ToLower().Contains(module.name.ToLower()))
                {


                    Log.Message(module.name);
                    RimValiUtility.modulesFound = RimValiUtility.modulesFound + module.name + "\n";
                    AvaliDefs.RimVali.race.baseHealthScale = RimValiMod.settings.healthScale;
                }
            }
        }
    }
}