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
        public bool enableDebugMode;
        public bool packMultiThreading;
        public bool textEnabled;
        public bool enableAirdrops;
        public bool mapCompOn;
        public bool avaliLayEggs;
        public Dictionary<string, bool> enabledRaces = new Dictionary<string, bool>();
        public override void ExposeData()
        {
            Scribe_Values.Look(ref packLossEnabled, "packLossEnabled", true);
            Scribe_Values.Look(ref packsEnabled, "packsEnabled", true);
            Scribe_Values.Look(ref checkOtherRaces, "checkOtherRaces", true);
            Scribe_Values.Look(ref allowAllRaces, "allowAllRaces", false);
            Scribe_Values.Look(ref enableAirdrops, "airdropsEnabled", true);
            Scribe_Values.Look(ref packMultiThreading, "threading", true);
            Scribe_Values.Look(ref maxPackSize, "maxPackSize", 5);
            Scribe_Values.Look(ref avaliLayEggs, "avaliLayEggs", false);
            Scribe_Values.Look(ref enableDebugMode, "debugModeOn", false);
            Scribe_Values.Look(ref mapCompOn, "mapCompOn", true);
            Scribe_Values.Look(ref textEnabled, "textEnabled", true);
            Scribe_Collections.Look<string, bool>(ref enabledRaces, "enabledRaces", LookMode.Undefined, LookMode.Undefined);
            base.ExposeData();
        }
    }

    public class RimValiMod : Mod
    {
        RimValiModSettings settings;
        public ModContentPack mod;

        public RimValiMod(ModContentPack content) : base(content)
        {
            this.mod = content;
            this.settings = GetSettings<RimValiModSettings>();
            RimValiUtility.dir = this.mod.RootDir.ToString();
            Log.Message(RimValiUtility.dir);
            if (settings.packMultiThreading)
            {
                Log.Message("!---RIMVALI PACK MULTITHREADING IS ACTIVE.---!");
            }

        }
        private Vector2 vector = Vector2.up;
        Rect rect = new Rect();
        private bool saidDebugWasEnabled;
        private int settingsAreStable = 0;
        private bool resetSettings;

        private void LogDebugOn()
        {
            if (!saidDebugWasEnabled & settings.enableDebugMode)
            {
                Log.Error("This is not an error, but RimVali debugging has been enabled. This may affect some functions.");
                saidDebugWasEnabled = true;
            }
        }
        private void ShowRaces()
        {
            /*if (!hasRun & settings.enableDebugMode)
            {
                string racesFound = "Races found: ";
                foreach (ThingDef race in RimvaliPotentialPackRaces.potentialRaces)
                {
                    racesFound = racesFound + ", " + race.defName;
                }
                hasRun = false;
                Log.Message(racesFound);
            }*/
        }

        public override void DoSettingsWindowContents(Rect rect)
        {

            bool threaded = settings.packMultiThreading;
            if (this.settings.enabledRaces == null)
            {
                this.settings.enabledRaces = new Dictionary<string, bool>();
            }
            if (!(settings.enableAirdrops))
            {
                AvaliDefs.AeroWeaveResearch.requiredResearchBuilding = AvaliDefs.AvaliResearchBench;
                AvaliDefs.AvaliAdvancedGuns.requiredResearchBuilding = AvaliDefs.AvaliResearchBench;
                AvaliDefs.AvaliAdvancedMelee.requiredResearchBuilding = AvaliDefs.AvaliResearchBench;
                AvaliDefs.AvaliAeroTungsten.requiredResearchBuilding = AvaliDefs.AvaliResearchBench;
            }
            else
            {
                AvaliDefs.AeroWeaveResearch.requiredResearchBuilding = AvaliDefs.AvaliNexus;
                AvaliDefs.AvaliAdvancedGuns.requiredResearchBuilding = AvaliDefs.AvaliNexus;
                AvaliDefs.AvaliAdvancedMelee.requiredResearchBuilding = AvaliDefs.AvaliNexus;
                AvaliDefs.AvaliAeroTungsten.requiredResearchBuilding = AvaliDefs.AvaliNexus;
            }
            Rect TopHalf = rect.TopHalf();
            Rect TopLeft = TopHalf.LeftHalf();
            Rect TopRight = TopHalf.RightHalf();
            Rect BottomHalf = rect.BottomHalf();
            Rect BottomLeft = BottomHalf.LeftHalf();
            Rect BottomRight = BottomHalf.RightHalf();
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(TopLeft);
            listing_Standard.Label("PackLabel".Translate());
            listing_Standard.GapLine(10);
            listing_Standard.CheckboxLabeled("PackLossCheck".Translate(), ref settings.packLossEnabled, "PackLossDesc".Translate());
            listing_Standard.CheckboxLabeled("MultithreadingCheck".Translate(), ref settings.packMultiThreading, "MultiThreadingDesc".Translate());
            listing_Standard.CheckboxLabeled("PacksCheck".Translate(), ref settings.packsEnabled, "PacksDesc".Translate());
            listing_Standard.CheckboxLabeled("Enable other avali", ref settings.checkOtherRaces, "Pull any other potential 'avali' races from other mods, and factor them into the pack system. ");
            
            /* try
             {
                 ShowRaces();
                 foreach (ThingDef race in RimvaliPotentialPackRaces.potentialRaces)
                 {
                     bool checkOn = this.settings.enabledRaces.TryGetValue(race.defName);
                     listing_Standard.CheckboxLabeled(race.label, ref checkOn);
                     this.settings.enabledRaces.SetOrAdd(race.defName, checkOn);
                 }
             }
             catch
             {
                 listing_Standard.GapLine(10);
                 listing_Standard.Label("RimVali '".Colorize(Color.red) + RimValiUtility.build.Colorize(Color.red) + "' was unable to show this item! We're sorry for any inconvience. :(".Colorize(Color.red));
                 listing_Standard.CheckboxLabeled("Enable debug mode".Colorize(Color.red), ref settings.enableDebugMode, "It appears RimVali encountered an error. You can use debug mode to return logs with more information on what RimVali was doing. [WIP]".Colorize(Color.red));
             }*/
            LogDebugOn();
            listing_Standard.Gap(10);
            if ((settings.maxPackSize < 20 & settings.maxPackSize > 10) | settings.maxPackSize < 3)
            {
                settingsAreStable = 1;
            }
            else if (settings.maxPackSize >= 20)
            {
                settingsAreStable = 2;
            }
            else
            {
                settingsAreStable = 0;
            }
            Rect packRacesRect = rect;
            packRacesRect.Set(packRacesRect.x, packRacesRect.y, BottomLeft.width-20, packRacesRect.height + (RimvaliPotentialPackRaces.potentialRaces.Count()*17));
            listing_Standard.Label("MaxPackSize".Translate() + settings.maxPackSize.ToString(), -1, "RimVali was made to play this way.".Colorize(Color.green));
            settings.maxPackSize = (int)listing_Standard.Slider(settings.maxPackSize, 2, 50);
            listing_Standard.End();
            listing_Standard.Begin(TopRight);
            listing_Standard.Label("GameplayLabel".Translate());
            listing_Standard.GapLine(10);
            listing_Standard.CheckboxLabeled("CanHaveEggs".Translate(), ref settings.avaliLayEggs, "EggsDesc".Translate());
            listing_Standard.CheckboxLabeled("ShowText".Translate(), ref settings.textEnabled);
            listing_Standard.CheckboxLabeled("AirdropsText".Translate(), ref settings.enableAirdrops);
            listing_Standard.End();
            listing_Standard.Begin(BottomRight);
            listing_Standard.Label("DebugLabel".Translate());
            listing_Standard.GapLine(10);
            if (this.settings.enableDebugMode)
            {
                listing_Standard.GapLine(10);
                listing_Standard.Label("Debug settings");
                listing_Standard.GapLine(10);
                listing_Standard.Label("RVBuild".Translate() + RimValiUtility.build);
                listing_Standard.CheckboxLabeled("Enable map component", ref settings.mapCompOn);

            }

            if (settingsAreStable == 0)
            {
                resetSettings = listing_Standard.ButtonText("StableSettings".Translate());
            }
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
            bool debugButton = listing_Standard.ButtonText("ToggleDebug".Translate());
            if (debugButton)
            {

                settings.enableDebugMode = !settings.enableDebugMode;
                debugButton = !debugButton;
            }
            if (!threaded == settings.packMultiThreading)
            {
                settings.Write();
                GenCommandLine.Restart();
            }
            listing_Standard.End();
            listing_Standard.BeginScrollView(BottomLeft,ref vector,ref packRacesRect);
            listing_Standard.Gap(50);
            listing_Standard.Label("RacesInPacks".Translate());
            listing_Standard.GapLine(10);

            try
            {
                ShowRaces();
                foreach (ThingDef race in RimvaliPotentialPackRaces.potentialRaces)
                {
                    bool checkOn = this.settings.enabledRaces.TryGetValue(race.defName);
                    listing_Standard.CheckboxLabeled(race.label, ref checkOn);
                    this.settings.enabledRaces.SetOrAdd(race.defName, checkOn);
                }
            }
            catch
            {
                listing_Standard.GapLine(10);
                listing_Standard.Label("RimVali '".Colorize(Color.red) + RimValiUtility.build.Colorize(Color.red) + "' was unable to show this item! We're sorry for any inconvience. :(".Colorize(Color.red));
                listing_Standard.CheckboxLabeled("Enable debug mode".Colorize(Color.red), ref settings.enableDebugMode, "It appears RimVali encountered an error. You can use debug mode to return logs with more information on what RimVali was doing. [WIP]".Colorize(Color.red));
            }
            listing_Standard.Gap(50);
            listing_Standard.EndScrollView(ref BottomLeft);
            base.DoSettingsWindowContents(rect);

        }
        public override string SettingsCategory()
        {
            return "RimValiCore";
        }
    }
}