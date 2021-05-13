using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
namespace AvaliMod
{

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
        public Dictionary<string, bool> enabledRaces = new Dictionary<string, bool>();


        public RimValiModSettings()
        {
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
        }
        public override void ExposeData()
        {
            Scribe_Values.Look(ref healthScale, "healthScale",1.3f, true);
            Scribe_Values.Look(ref packLossEnabled, "packLossEnabled", true, true);
            Scribe_Values.Look(ref packsEnabled, "packsEnabled", true, true);
            Scribe_Values.Look(ref checkOtherRaces, "checkOtherRaces", true, true);
            Scribe_Values.Look(ref allowAllRaces, "allowAllRaces", false , true);
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
            Scribe_Collections.Look<string, bool>(ref enabledRaces, "enabledRaces", LookMode.Undefined, LookMode.Undefined);
            base.ExposeData();
        }
    }
    
    public class RimValiMod : Mod
    {
        public int windowToShow;
        public static RimValiModSettings settings;
        public ModContentPack mod;
        private bool hasCollectedModules = false;
        public RimValiMod(ModContentPack content) : base(content)
        {
            if (!hasCollectedModules)
            {
                Modulefinder.startup();
                hasCollectedModules = true;
            }
            this.mod = content;
            settings = GetSettings<RimValiModSettings>();
            RimValiUtility.dir = this.mod.RootDir.ToString();
            Log.Message(RimValiUtility.dir);
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
            //Main page
            if (0 == windowToShow)
            {
                
                listing_Standard.Begin(rect);
                bool packSettings = listing_Standard.ButtonText("PackLabel".Translate());
                bool gameplaySettings = listing_Standard.ButtonText("GameplayLabel".Translate());
                bool debug = listing_Standard.ButtonText("DebugLabel".Translate());
                if (packSettings)
                {
                    windowToShow = 1;
                }
                if (gameplaySettings)
                    windowToShow = 2;
                if (debug)
                    windowToShow = 3;
            }
            //Pack settings
            if(windowToShow == 1)
            {
                listing_Standard.Begin(TopLeft);
                Backbutton(listing_Standard);
                listing_Standard.CheckboxLabeled("PackLossCheck".Translate(), ref settings.packLossEnabled, "PackLossDesc".Translate());
                bool threading = listing_Standard.ButtonText("MultithreadingCheck".Translate() +" "+((Func<string>) delegate { if (settings.packMultiThreading) { return "Y"; } else { return "N"; }; })());
                if (threading)
                {
                    windowToShow = 4;
                }
                listing_Standard.CheckboxLabeled("MultithreadingCheck".Translate(), ref settings.packMultiThreading, "MultiThreadingDesc".Translate());
                listing_Standard.CheckboxLabeled("PacksCheck".Translate(), ref settings.packsEnabled, "PacksDesc".Translate());
                listing_Standard.Label("MaxPackSize".Translate(settings.maxPackSize.Named("COUNT")), -1, "PacksNum".Translate());
                settings.maxPackSize = (int)listing_Standard.Slider(settings.maxPackSize, 2, 50);
                listing_Standard.Label("PackOpinionReq".Translate(settings.packOpReq.Named("COUNT")));
                settings.packOpReq = (int)listing_Standard.Slider(settings.packOpReq, 0, 100);
            }
            //Gameplay settings
            if (windowToShow == 2)
            {
                listing_Standard.Begin(rect);
                Backbutton(listing_Standard);
                listing_Standard.CheckboxLabeled("CanHaveEggs".Translate(), ref settings.avaliLayEggs, "EggsDesc".Translate());

                listing_Standard.CheckboxLabeled("ShowText".Translate(), ref settings.textEnabled, "ShowTextLabel".Translate());
                listing_Standard.CheckboxLabeled("AirdropsText".Translate(), ref settings.enableAirdrops, "AirdropsLabel".Translate());
                listing_Standard.Label("AvaliForDropReq".Translate(settings.avaliRequiredForDrop.Named("COUNT")));
                settings.avaliRequiredForDrop = (int)listing_Standard.Slider(settings.avaliRequiredForDrop, 0, 100);
                listing_Standard.Label("HPScaler".Translate(settings.healthScale.Named("SCALE")));
                settings.healthScale = (float)listing_Standard.Slider(settings.healthScale, 1f, 2.5f);

                listing_Standard.Label("PackLossStageOneSetting".Translate(settings.stageOneDaysPackloss.Named("TIME")));
                settings.stageOneDaysPackloss = (int)listing_Standard.Slider(settings.stageOneDaysPackloss, 1, 100);
                if(settings.stageOneDaysPackloss > settings.stageTwoDaysPackloss)
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
                listing_Standard.Label($"Chance to hack Illuminate tech as Independent Worlds: {settings.hackChance}");
                settings.hackChance = (int)listing_Standard.Slider(settings.hackChance, 0, 100);
            }
            
            //Debug
            if(windowToShow == 3)
            {
                listing_Standard.Begin(TopLeft);
                Modulefinder.startup();
                listing_Standard.Label("Debug settings");
                listing_Standard.GapLine(10);
                listing_Standard.CheckboxLabeled("ToggleDebug".Translate(), ref settings.enableDebugMode);
                listing_Standard.Label("RVBuild".Translate(RimValiUtility.build.Named("BUILD")));
                listing_Standard.CheckboxLabeled("Enable map component", ref settings.mapCompOn);
                listing_Standard.Label(RimValiUtility.modulesFound);
                Backbutton(listing_Standard);
            }
            //Multithreading reboot warn
            if(windowToShow == 4)
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
                    windowToShow = 1;
                }
            }
            listing_Standard.End();
           
          /*
            else if (settingsAreStable == 1)
            {
                resetSettings = listing_Standard.ButtonText("PotentialIssuesSettings".Translate());
            }
            else
            {
                resetSettings = listing_Standard.ButtonText("UnstableSettings".Translate());
            }
            if (resetSettings)
            {
                settings.maxPackSize = 5;
                settings.packLossEnabled = true;
                settings.packsEnabled = true;
                settings.textEnabled = true;
                settings.checkOtherRaces = true;
                settings.allowAllRaces = false;
                settings.enableDebugMode = false;
                settings.avaliLayEggs = false;
            }
            
            listing_Standard.End();*/
            base.DoSettingsWindowContents(rect);

        }
        public override string SettingsCategory()
        {
            return "RimValiCore";
        }
    }
}