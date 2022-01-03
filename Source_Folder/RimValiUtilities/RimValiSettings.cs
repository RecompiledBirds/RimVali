using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;
namespace AvaliMod
{
    public enum settingsWindow
    {
        main = 0,
        packs = 1,
        gameplay = 2,
        debug = 3,
        threading = 4,
        pawns = 5
    }
    public class RimValiModSettings : ModSettings
    {
        public bool packLossEnabled;
        public bool packsEnabled;
        public bool checkOtherRaces;
        public bool allowAllRaces;
        public int maxPackSize;
        public int packOpReq;
        public bool enableDebugMode;
        public bool packMultiThreading;
        public bool textEnabled;
        public bool enableAirdrops;
        public bool mapCompOn;
        public bool avaliLayEggs;
        public float healthScale;
        public int avaliRequiredForDrop;
        public int stageOneDaysPackloss;
        public int stageTwoDaysPackloss;
        public int stageThreeDaysPackloss;
        public int hackChance;
        public int AERIALShellCap;
        public bool canGetPackBroken;
        public int packBrokenChance;
        public int IlluminateAvaliMaxTemp;
        public int IlluminateAvaliMinTemp;
        public int IWAvaliMaxTemp;
        public int IWAvaliMinTemp;
        public Dictionary<string, bool> enabledRaces = new Dictionary<string, bool>();
        public bool liteMode;
        public bool packThoughtsEnabled;
        public int ticksBetweenPackUpdate;

        public RimValiModSettings()
        {
            ticksBetweenPackUpdate = 120;
            packThoughtsEnabled = true;
            enabledRaces = new Dictionary<string, bool>();
            healthScale = 1.3f;
            packLossEnabled = true;
            packMultiThreading = true;
            packOpReq = 30;
            maxPackSize = 5;
            mapCompOn = true;
            textEnabled = true;
            enableAirdrops = true;
            checkOtherRaces = true;
            packsEnabled = true;
            avaliRequiredForDrop = 5;
            stageOneDaysPackloss = 1;
            stageTwoDaysPackloss = 2;
            stageThreeDaysPackloss = 3;
            hackChance = 30;
            AERIALShellCap = 6;
            canGetPackBroken = true;
            packBrokenChance = 5;
            IlluminateAvaliMaxTemp = 15;
            IlluminateAvaliMinTemp = -30;
            IWAvaliMaxTemp = 25;
            IWAvaliMinTemp = -5;
            liteMode = false;
        }
        public override void ExposeData()
        {
            Scribe_Values.Look(ref packThoughtsEnabled, "packThoughtsEnabled", true);
            Scribe_Values.Look(ref healthScale, "healthScale", 1.3f, true);
            Scribe_Values.Look(ref packLossEnabled, "packLossEnabled", true, true);
            Scribe_Values.Look(ref packsEnabled, "packsEnabled", true, true);
            Scribe_Values.Look(ref checkOtherRaces, "checkOtherRaces", true, true);
            Scribe_Values.Look(ref allowAllRaces, "allowAllRaces", false, true);
            Scribe_Values.Look(ref enableAirdrops, "airdropsEnabled", true, true);
            Scribe_Values.Look(ref packMultiThreading, "threading", true, true);
            Scribe_Values.Look(ref maxPackSize, "maxPackSize", 5, true);
            Scribe_Values.Look(ref avaliRequiredForDrop, "avaliRequiredForDrop", 5, true);
            Scribe_Values.Look(ref avaliLayEggs, "avaliLayEggs", false, true);
            Scribe_Values.Look(ref enableDebugMode, "debugModeOn", false, true);
            Scribe_Values.Look(ref mapCompOn, "mapCompOn", true, true);
            Scribe_Values.Look(ref textEnabled, "textEnabled", true, true);
            Scribe_Values.Look(ref packOpReq, "packOpReq", 30, true);
            Scribe_Values.Look(ref stageOneDaysPackloss, "stageOneDaysPackloss", 1, true);
            Scribe_Values.Look(ref stageTwoDaysPackloss, "stageTwoDaysPackloss", 2, true);
            Scribe_Values.Look(ref stageThreeDaysPackloss, "stageThreeDaysPackloss", 3, true);
            Scribe_Values.Look(ref hackChance, "hackChance", 30, true);
            Scribe_Values.Look(ref AERIALShellCap, "AERIALShellCap", 6, true);
            Scribe_Values.Look(ref IlluminateAvaliMinTemp, "IlluminateAvaliMinTemp", -30, true);
            Scribe_Values.Look(ref IlluminateAvaliMaxTemp, "IlluminateAvaliMaxTemp", 15, true);
            Scribe_Values.Look(ref IWAvaliMinTemp, "IWAvaliMinTemp", -5, true);
            Scribe_Values.Look(ref IWAvaliMaxTemp, "IWAvaliMaxTemp", 25, true);
            Scribe_Collections.Look<string, bool>(ref enabledRaces, "enabledRaces", LookMode.Undefined, LookMode.Undefined);
            Scribe_Values.Look(ref packBrokenChance, "packBrokenChance", 5, true);
            Scribe_Values.Look(ref ticksBetweenPackUpdate, "ticksBetweenPackUpdate", 120, true);
            Scribe_Values.Look(ref canGetPackBroken, "canGetPackBroken", true, true);
            Scribe_Values.Look(ref liteMode, "liteMode", false, true);
            base.ExposeData();
        }
    }

    public class RimValiMod : Mod
    {
        public settingsWindow windowToShow;
        public static RimValiModSettings settings;
        public ModContentPack mod;
        private bool hasCollectedModules = false;
        private static string dir;
        public static string GetDir
        {
            get
            {
                return dir;
            }
        }
        public RimValiMod(ModContentPack content) : base(content)
        {
            dir = content.RootDir.ToString();
           
            if (!hasCollectedModules)
            {
                Modulefinder.startup();
                hasCollectedModules = true;
            }
            this.mod = content;
            if (LoadedModManager.RunningModsListForReading.Any(x => x.Name.ToLower().Contains("avali continued")))
            {
                Log.Warning("It appears avali continued or a mod made for it is running with RimVali! This may cause issues, and is not recommended.");
            }
            if (!LoadedModManager.RunningModsListForReading.Any(x => x.Name.ToLower() == "rimvali: core"))
            {
                Log.Error("RIMVALI CORE IS NOT LOADED. THIS WILL CRASH THE GAME");
            }
            settings = GetSettings<RimValiModSettings>();

            if (settings.packMultiThreading)
            {
                Log.Message("!---RIMVALI PACK MULTITHREADING IS ACTIVE.---!");
            }
        }

        void Backbutton(Listing_Standard listing_Standard)
        {
            bool goBack = listing_Standard.ButtonText("goBack".Translate());
            if (goBack)
                windowToShow = 0;
        }
        public override void DoSettingsWindowContents(Rect rect)
        {
            Window window = Find.WindowStack.currentlyDrawnWindow;
            bool threaded = settings.packMultiThreading;
            if (settings.enabledRaces == null)
            {
                settings.enabledRaces = new Dictionary<string, bool>();
            }


            Rect TopHalf = rect.TopHalf();
            Rect TopLeft = TopHalf.LeftHalf();
            Rect TopRight = TopHalf.RightHalf();
            Rect BottomHalf = rect.BottomHalf();
            Rect BottomLeft = BottomHalf.LeftHalf();
            Rect BottomRight = BottomHalf.RightHalf();
            Listing_Standard listing_Standard = new Listing_Standard();
            #region main
            //Main page
            if (settingsWindow.main == windowToShow)
            {

                listing_Standard.Begin(rect);
                bool packSettings = listing_Standard.ButtonText("PackLabel".Translate());
                bool gameplaySettings = listing_Standard.ButtonText("GameplayLabel".Translate());
                bool debug = listing_Standard.ButtonText("DebugLabel".Translate());
                // bool pawnsSettings = listing_Standard.ButtonText("Pawns");
                if (packSettings)
                {
                    windowToShow = settingsWindow.packs;
                }
                if (gameplaySettings)
                    windowToShow = settingsWindow.gameplay;
                if (debug)
                    windowToShow = settingsWindow.debug;
                // if (pawnsSettings)
                //   windowToShow = settingsWindow.pawns;
            }
            #endregion
            #region pack settings
            //Pack settings
            if (windowToShow == settingsWindow.packs)
            {
                listing_Standard.Begin(rect);
                Backbutton(listing_Standard);
                listing_Standard.CheckboxLabeled("PackLossCheck".Translate(), ref settings.packLossEnabled, "PackLossDesc".Translate());
                listing_Standard.CheckboxLabeled("PacksCheck".Translate(), ref settings.packsEnabled, "PacksDesc".Translate());
                listing_Standard.CheckboxLabeled("PawnsCanHavePackThoughts".Translate(), ref settings.packThoughtsEnabled);
                listing_Standard.CheckboxLabeled("PawnsCanBePackBroken".Translate(), ref settings.canGetPackBroken, "PackBrokenDesc".Translate());
                listing_Standard.Label("MaxPackSize".Translate(settings.maxPackSize.Named("COUNT")), -1, "PackCountDescription".Translate());
                settings.maxPackSize = (int)listing_Standard.Slider(settings.maxPackSize, 2, 50);
                listing_Standard.Label("PackOpinionReq".Translate(settings.packOpReq.Named("COUNT")));
                listing_Standard.Label("TicksBetweenPackUpdates".Translate());
                Int32.TryParse(listing_Standard.TextEntry(settings.ticksBetweenPackUpdate.ToString()), out settings.ticksBetweenPackUpdate);
                bool threading = listing_Standard.ButtonText("MultithreadingCheck".Translate() + " " + ((Func<string>)delegate { if (settings.packMultiThreading) { return "Y"; } else { return "N"; }; })());
                if (threading)
                {
                    windowToShow = settingsWindow.threading;
                }
                settings.packOpReq = (int)listing_Standard.Slider(settings.packOpReq, 0, 100);
                listing_Standard.Gap(12);
                listing_Standard.Label("PackLossSettingsLabel".Translate());
                listing_Standard.Label("PackLossStageOneSetting".Translate(settings.stageOneDaysPackloss.Named("TIME")));
                settings.stageOneDaysPackloss = (int)listing_Standard.Slider(settings.stageOneDaysPackloss, 1, 100);
                if (settings.stageOneDaysPackloss > settings.stageTwoDaysPackloss)
                {
                    settings.stageTwoDaysPackloss = settings.stageOneDaysPackloss;
                }
                listing_Standard.Label("PackLossStageTwoSetting".Translate(settings.stageTwoDaysPackloss.Named("TIME")));
                settings.stageTwoDaysPackloss = (int)listing_Standard.Slider(settings.stageTwoDaysPackloss, settings.stageOneDaysPackloss, settings.stageThreeDaysPackloss);
                listing_Standard.Label("PackLossStageThreeSetting".Translate(settings.stageThreeDaysPackloss.Named("TIME")));
                if (settings.stageTwoDaysPackloss > settings.stageThreeDaysPackloss)
                {
                    settings.stageThreeDaysPackloss = settings.stageTwoDaysPackloss;
                }
                settings.stageThreeDaysPackloss = (int)listing_Standard.Slider(settings.stageThreeDaysPackloss, settings.stageTwoDaysPackloss, 100);
                
                listing_Standard.Label("PackBrokenChance".Translate(settings.packBrokenChance.Named("CHANCE")));
                settings.packBrokenChance = (int)listing_Standard.Slider(settings.packBrokenChance, 0, 100);
                
            }
            #endregion
            #region gameplay
            //Gameplay settings
            if (windowToShow == settingsWindow.gameplay)
            {
                listing_Standard.Begin(rect);
                Backbutton(listing_Standard);
                //listing_Standard.CheckboxLabeled("RimValiLiteMode".Translate(), ref settings.liteMode, "RimValiLiteModeDesc".Translate());
                listing_Standard.CheckboxLabeled("CanHaveEggs".Translate(), ref settings.avaliLayEggs, "EggsDesc".Translate());

                listing_Standard.CheckboxLabeled("ShowText".Translate(), ref settings.textEnabled, "ShowTextLabel".Translate());
                listing_Standard.CheckboxLabeled("AirdropsText".Translate(), ref settings.enableAirdrops, "AirdropsLabel".Translate());
                listing_Standard.Label("AvaliForDropReq".Translate(settings.avaliRequiredForDrop.Named("COUNT")));
                settings.avaliRequiredForDrop = (int)listing_Standard.Slider(settings.avaliRequiredForDrop, 0, 100);
                float scale = settings.healthScale;
                listing_Standard.Label("HPScaler".Translate(settings.healthScale.Named("SCALE")));
                settings.healthScale = (float)listing_Standard.Slider(settings.healthScale, 0.01f, 2.5f);

                listing_Standard.Label("ChanceToHackTech".Translate(settings.hackChance.Named("CHANCE")));
                settings.hackChance = (int)listing_Standard.Slider(settings.hackChance, 0, 100);
                listing_Standard.Label("AERIALShellCapSetting".Translate(settings.AERIALShellCap.Named("CAP")));
                settings.AERIALShellCap = (int)listing_Standard.Slider(settings.AERIALShellCap, 1, 100);
                {
                    int newTemp;
                    listing_Standard.Label("IlluminateAvaliMaxTemp".Translate());
                    int.TryParse(listing_Standard.TextEntry(settings.IlluminateAvaliMaxTemp.ToString()), out newTemp);
                    if (newTemp != settings.IlluminateAvaliMaxTemp)
                    {
                        newTemp= Mathf.Clamp(newTemp, -200, 1000);
                        settings.IlluminateAvaliMaxTemp = newTemp;
                    }
                    int newTempMin;
                    listing_Standard.Label("IlluminateAvaliMinTemp".Translate());
                    int.TryParse(listing_Standard.TextEntry(settings.IlluminateAvaliMinTemp.ToString()), out newTempMin);
                    if (newTempMin != settings.IlluminateAvaliMinTemp)
                    {
                        newTempMin = Mathf.Clamp(newTempMin,-200, newTemp);
                        settings.IlluminateAvaliMinTemp = newTempMin;
                    }
                }
                {
                    int newTemp;
                    listing_Standard.Label("IWAvaliMaxTemp".Translate());
                    int.TryParse(listing_Standard.TextEntry(settings.IWAvaliMaxTemp.ToString()), out newTemp);
                    if (newTemp != settings.IWAvaliMaxTemp)
                    {
                        newTemp = Mathf.Clamp(newTemp, -200, 1000);
                        settings.IWAvaliMaxTemp = newTemp;
                    }
                    int newTempMin;
                    listing_Standard.Label("IWAvaliMinTemp".Translate());
                    int.TryParse(listing_Standard.TextEntry(settings.IWAvaliMinTemp.ToString()), out newTempMin);
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
            if (windowToShow == settingsWindow.debug)
            {
                listing_Standard.Begin(TopLeft);
                Modulefinder.startup();
                listing_Standard.Label("DebugSettings");
                listing_Standard.GapLine(10);
                listing_Standard.CheckboxLabeled("ToggleDebug".Translate(), ref settings.enableDebugMode);
                listing_Standard.Label("RVBuild".Translate(RimValiUtility.build.Named("BUILD")));
                listing_Standard.CheckboxLabeled("Enable map component", ref settings.mapCompOn);
                bool rL = listing_Standard.ButtonText("ResetPackLoss");

                listing_Standard.Label(RimValiUtility.modulesFound);
                Backbutton(listing_Standard);
            }
            #endregion
            #region Multithreading warn
            //Multithreading reboot warn
            if (windowToShow == settingsWindow.threading)
            {
                listing_Standard.Begin(rect);
                listing_Standard.Label("MultiThreadWarnLabel".Translate());
                listing_Standard.Label("MultiThreadWarnDesc".Translate());
                bool ready = listing_Standard.ButtonText("MultiThreadYes".Translate(), null);
                bool goBack = listing_Standard.ButtonText("MultiThreadNo".Translate());
                if (ready)
                {
                    void UpdateBool(ref bool val)
                    {
                        val = !val;
                    }
                    UpdateBool(ref settings.packMultiThreading);
                    settings.Write();
                    GenCommandLine.Restart();
                }
                if (goBack)
                {
                    windowToShow = settingsWindow.main;
                }
            }
            #endregion
            if (windowToShow != settingsWindow.pawns)
            {
                listing_Standard.End();
            }

            base.DoSettingsWindowContents(rect);

        }
        public override string SettingsCategory()
        {
            return "RimVali: Far From Avalon";
        }
    }

}