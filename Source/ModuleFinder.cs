using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AvaliMod
{
    [StaticConstructorOnStartup]
    public static class ModuleFinder
    {
        public static IEnumerable<ModContentPack> loadedMods =
            LoadedModManager.RunningMods.Where(x => x.Name.ToLower().Contains("rimvali"));

        public static IEnumerable<ModuleDef> moduleDefs = DefDatabase<ModuleDef>.AllDefs;

        static ModuleFinder()
        {
            Startup();
        }

        public static void Startup()
        {
            //  AvaliDefs.RimVali.race.baseHealthScale = RimValiMod.settings.healthScale;
            //  AvaliDefs.IWAvaliRace.race.baseHealthScale = RimValiMod.settings.healthScale;

            moduleDefs = DefDatabase<ModuleDef>.AllDefs;

            foreach (ModuleDef module in moduleDefs)
            {
                if (!RimValiUtility.FoundModulesString.ToLower().Contains(module.name.ToLower()))
                {
                    Log.Message(module.name);
                    RimValiUtility.foundModules.Append(module);

                    if (RimValiMod.settings.liteMode)
                    {
                        AvaliDefs.AvaliNanoForge.thingCategories = new List<ThingCategoryDef>
                            { AvaliDefs.BuildingsSpecial };
                        AvaliDefs.AvaliNanoForge.PostLoad();
                        AvaliDefs.AvaliNanoForge.ResolveReferences();

                        AvaliDefs.AvaliNanoLoom.thingCategories = new List<ThingCategoryDef>
                            { AvaliDefs.BuildingsSpecial };
                        AvaliDefs.AvaliNanoLoom.PostLoad();
                        AvaliDefs.AvaliNanoLoom.ResolveReferences();

                        AvaliDefs.AvaliResearchBench.thingCategories = new List<ThingCategoryDef>
                            { AvaliDefs.BuildingsSpecial };
                        AvaliDefs.AvaliResearchBench.PostLoad();
                        AvaliDefs.AvaliResearchBench.ResolveReferences();
                        AvaliDefs.AvaliResearchBench.designationCategory.PostLoad();
                        AvaliDefs.AvaliResearchBench.designationCategory.ResolveReferences();
                        foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(x =>
                                     x.IsWeapon && x.modContentPack.PackageId.ToLower() == "nezitheavali.rimvali"))
                        {
                            def.recipeMaker = new RecipeMakerProperties();
                            def.weaponTags = new List<string>();
                            def.tradeTags = new List<string>();

                            def.PostLoad();
                            def.ResolveReferences();
                        }

                        foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(x =>
                                     x.IsApparel && x.modContentPack.PackageId.ToLower() == "nezitheavali.rimvali"))
                        {
                            def.recipeMaker = new RecipeMakerProperties();
                            def.weaponTags = new List<string>();
                            def.tradeTags = new List<string>();

                            def.PostLoad();
                            def.ResolveReferences();
                        }
                    }
                }
            }
        }
    }
}