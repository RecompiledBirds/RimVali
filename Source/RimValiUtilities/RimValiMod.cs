using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace AvaliMod
{
    public class RimValiMod : Mod
    {
        public static RimValiModSettings settings;
        private readonly bool hasCollectedModules;
        public ModContentPack mod;
        private SettingsWindow windowToShow;

        public RimValiMod(ModContentPack content) : base(content)
        {
            GetDir = content.RootDir;

            if (!hasCollectedModules)
            {
                RVFFAInit.Startup();
                hasCollectedModules = true;
            }

            mod = content;
            if (LoadedModManager.RunningModsListForReading.Any(x => x.Name.ToLower().Contains("avali continued")))
            {
                Log.Warning(
                    "It appears avali continued or a mod made for it is running with RimVali! This may cause issues, and is not recommended.");
            }

            if (!LoadedModManager.RunningModsListForReading.Any(x => x.Name.ToLower() == "rimvali: core"))
            {
                Log.Error("RIMVALI CORE IS NOT LOADED. THIS WILL CRASH THE GAME");
            }

            
            settings = GetSettings<RimValiModSettings>();
        }

        public static string GetDir { get; private set; }

        private void Backbutton(Listing_Standard listing_Standard)
        {
            bool goBack = listing_Standard.ButtonText("goBack".Translate());
            if (goBack)
            {
                windowToShow = 0;
            }
        }

        public override void DoSettingsWindowContents(Rect rect)
        {
            if (settings.enabledRaces == null)
            {
                settings.enabledRaces = new Dictionary<string, bool>();
            }


            Rect TopHalf = rect.TopHalf();
            Rect TopLeft = TopHalf.LeftHalf();
            TopHalf.RightHalf();
            Rect BottomHalf = rect.BottomHalf();
            BottomHalf.LeftHalf();
            BottomHalf.RightHalf();
            var listing_Standard = new Listing_Standard();

            #region main

            //Main page
            if (SettingsWindow.Main == windowToShow)
            {
                listing_Standard.Begin(rect);
                bool packSettings = listing_Standard.ButtonText("PackLabel".Translate());
                bool gameplaySettings = listing_Standard.ButtonText("GameplayLabel".Translate());
                bool debug = listing_Standard.ButtonText("DebugLabel".Translate());
                // bool pawnsSettings = listing_Standard.ButtonText("Pawns");
                if (packSettings)
                {
                    windowToShow = SettingsWindow.Packs;
                }

                if (gameplaySettings)
                {
                    windowToShow = SettingsWindow.Gameplay;
                }

                if (debug)
                {
                    windowToShow = SettingsWindow.Debug;
                }
                // if (pawnsSettings)
                //   windowToShow = SettingsWindow.pawns;
            }

            #endregion

            #region pack settings

            //Pack settings
            if (windowToShow == SettingsWindow.Packs)
            {
                listing_Standard.Begin(rect);
                Backbutton(listing_Standard);
                listing_Standard.CheckboxLabeled("PackLossCheck".Translate(), ref settings.packLossEnabled,
                    "PackLossDesc".Translate());
                listing_Standard.CheckboxLabeled("PacksCheck".Translate(), ref settings.packsEnabled,
                    "PacksDesc".Translate());
                listing_Standard.CheckboxLabeled("PawnsCanHavePackThoughts".Translate(),
                    ref settings.packThoughtsEnabled);
                listing_Standard.CheckboxLabeled("PawnsCanBePackBroken".Translate(), ref settings.canGetPackBroken,
                    "PackBrokenDesc".Translate());
                listing_Standard.Label("MaxPackSize".Translate(settings.maxPackSize.Named("COUNT")), -1,
                    "PackCountDescription".Translate());
                settings.maxPackSize = (int)listing_Standard.Slider(settings.maxPackSize, 2, 50);
                listing_Standard.Label("PackOpinionReq".Translate(settings.packOpReq.Named("COUNT")));
                listing_Standard.Label("TicksBetweenPackUpdates".Translate());
                int.TryParse(listing_Standard.TextEntry(settings.ticksBetweenPackUpdate.ToString()),
                    out settings.ticksBetweenPackUpdate);


                settings.packOpReq = (int)listing_Standard.Slider(settings.packOpReq, 0, 100);
                listing_Standard.Gap();
                listing_Standard.Label("PackLossSettingsLabel".Translate());
                listing_Standard.Label(
                    "PackLossStageOneSetting".Translate(settings.stageOneDaysPackloss.Named("TIME")));
                settings.stageOneDaysPackloss = (int)listing_Standard.Slider(settings.stageOneDaysPackloss, 1, 100);
                if (settings.stageOneDaysPackloss > settings.stageTwoDaysPackloss)
                {
                    settings.stageTwoDaysPackloss = settings.stageOneDaysPackloss;
                }

                listing_Standard.Label(
                    "PackLossStageTwoSetting".Translate(settings.stageTwoDaysPackloss.Named("TIME")));
                settings.stageTwoDaysPackloss = (int)listing_Standard.Slider(settings.stageTwoDaysPackloss,
                    settings.stageOneDaysPackloss, settings.stageThreeDaysPackloss);
                listing_Standard.Label(
                    "PackLossStageThreeSetting".Translate(settings.stageThreeDaysPackloss.Named("TIME")));
                if (settings.stageTwoDaysPackloss > settings.stageThreeDaysPackloss)
                {
                    settings.stageThreeDaysPackloss = settings.stageTwoDaysPackloss;
                }

                settings.stageThreeDaysPackloss = (int)listing_Standard.Slider(settings.stageThreeDaysPackloss,
                    settings.stageTwoDaysPackloss, 100);

                listing_Standard.Label("PackBrokenChance".Translate(settings.packBrokenChance.Named("CHANCE")));
                settings.packBrokenChance = (int)listing_Standard.Slider(settings.packBrokenChance, 0, 100);
            }

            #endregion

            #region gameplay

            //Gameplay settings
            if (windowToShow == SettingsWindow.Gameplay)
            {
                listing_Standard.Begin(rect);
                Backbutton(listing_Standard);
                //listing_Standard.CheckboxLabeled("RimValiLiteMode".Translate(), ref settings.liteMode, "RimValiLiteModeDesc".Translate());
                listing_Standard.CheckboxLabeled("CanHaveEggs".Translate(), ref settings.avaliLayEggs,
                    "EggsDesc".Translate());

                listing_Standard.CheckboxLabeled("ShowText".Translate(), ref settings.textEnabled,
                    "ShowTextLabel".Translate());
                listing_Standard.CheckboxLabeled("AirdropsText".Translate(), ref settings.enableAirdrops,
                    "AirdropsLabel".Translate());
                listing_Standard.Label("AvaliForDropReq".Translate(settings.avaliRequiredForDrop.Named("COUNT")));
                settings.avaliRequiredForDrop = (int)listing_Standard.Slider(settings.avaliRequiredForDrop, 0, 100);
                listing_Standard.Label("HPScaler".Translate(settings.healthScale.Named("SCALE")));
                settings.healthScale = listing_Standard.Slider(settings.healthScale, 0.01f, 2.5f);

                listing_Standard.Label("ChanceToHackTech".Translate(settings.hackChance.Named("CHANCE")));
                settings.hackChance = (int)listing_Standard.Slider(settings.hackChance, 0, 100);
                listing_Standard.Label("AERIALShellCapSetting".Translate(settings.AERIALShellCap.Named("CAP")));
                settings.AERIALShellCap = (int)listing_Standard.Slider(settings.AERIALShellCap, 1, 100);
                {
                    listing_Standard.Label("IlluminateAvaliMaxTemp".Translate());
                    int.TryParse(listing_Standard.TextEntry(settings.IlluminateAvaliMaxTemp.ToString()),
                        out int newTemp);
                    if (newTemp != settings.IlluminateAvaliMaxTemp)
                    {
                        newTemp = Mathf.Clamp(newTemp, -200, 1000);
                        settings.IlluminateAvaliMaxTemp = newTemp;
                    }

                    listing_Standard.Label("IlluminateAvaliMinTemp".Translate());
                    int.TryParse(listing_Standard.TextEntry(settings.IlluminateAvaliMinTemp.ToString()),
                        out int newTempMin);
                    if (newTempMin != settings.IlluminateAvaliMinTemp)
                    {
                        newTempMin = Mathf.Clamp(newTempMin, -200, newTemp);
                        settings.IlluminateAvaliMinTemp = newTempMin;
                    }
                }
                {
                    listing_Standard.Label("IWAvaliMaxTemp".Translate());
                    int.TryParse(listing_Standard.TextEntry(settings.IWAvaliMaxTemp.ToString()), out int newTemp);
                    if (newTemp != settings.IWAvaliMaxTemp)
                    {
                        newTemp = Mathf.Clamp(newTemp, -200, 1000);
                        settings.IWAvaliMaxTemp = newTemp;
                    }

                    listing_Standard.Label("IWAvaliMinTemp".Translate());
                    int.TryParse(listing_Standard.TextEntry(settings.IWAvaliMinTemp.ToString()), out int newTempMin);
                    if (newTempMin != settings.IWAvaliMinTemp)
                    {
                        newTempMin = Mathf.Clamp(newTempMin, -200, newTemp);
                        settings.IWAvaliMinTemp = newTempMin;
                    }
                }
            }

            #endregion

            #region Debug

            //Debug
            if (windowToShow == SettingsWindow.Debug)
            {
                listing_Standard.Begin(TopLeft);
                RVFFAInit.Startup();
                listing_Standard.Label("DebugSettings");
                listing_Standard.GapLine(10);
                listing_Standard.CheckboxLabeled("ToggleDebug".Translate(), ref settings.enableDebugMode);
                listing_Standard.Label("RVBuild".Translate(RimValiUtility.build.Named("BUILD")));
                listing_Standard.CheckboxLabeled("Enable map component", ref settings.mapCompOn);
                listing_Standard.ButtonText("ResetPackLoss");

                listing_Standard.Label(RimValiUtility.FoundModulesString);
                Backbutton(listing_Standard);
            }

            #endregion

            #region Multithreading warn

            
            #endregion
            bool dateHasPassed = DateTime.Today.Day >= 1 && DateTime.Today.Month >= 5 && DateTime.Today.Year >= 2021;
            if(!dateHasPassed)
                listing_Standard.CheckboxLabeled($"Enhanced mode", ref settings.unstable, "Enables new features. Disabled by default to preserve old saves.");
            
            if (windowToShow != SettingsWindow.Pawns)
            {
                listing_Standard.End();
            }

            base.DoSettingsWindowContents(rect);
        }

        public override string SettingsCategory()
        {
            return "RimVali: Far From Avalon";
        }

        private enum SettingsWindow
        {
            Main = 0,
            Packs = 1,
            Gameplay = 2,
            Debug = 3,
            Threading = 4,
            Pawns = 5,
        }
    }
}
