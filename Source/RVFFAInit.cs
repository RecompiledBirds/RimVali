using System.Collections.Generic;
using System.Linq;
using Verse;
using RimValiCore;
using System;
using UnityEngine;
using System.IO;

namespace AvaliMod
{
    public class RVFFAInit : RVCInitalizer.RVCPreLoad
    {
        public static IEnumerable<ModContentPack> loadedMods =
            LoadedModManager.RunningMods.Where(x => x.Name.ToLower().Contains("rimvali"));

        public static IEnumerable<ModuleDef> moduleDefs = DefDatabase<ModuleDef>.AllDefs;

        /// <summary>
        /// Ensures FFA startup runs after RVC has started.
        /// </summary>
        public override void InitAction()
        {
            void PostLoadAction()
            {
                Log.Message("<color=orange>[RimVali]: Starting up RimVali..</color>");
                Startup();
                RimValiDefChecks.Initalize();
                RimValiPatches.Initalize();
                Log.Message("<color=orange>[RimVali]: Loaded successfully!</color>");
            }
            RVCInitalizer.AddPostLoadAction(PostLoadAction);
            Log.Message("<color=orange>[RimVali]: Ready for RVC postload.</color>");

           
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
                    RimValiUtility.foundModules.Append(module);

                   
                }
            }
        }
    }
}