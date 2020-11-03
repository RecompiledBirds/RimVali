using System.Collections.Generic;
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
        public bool textEnabled;
        public bool mapCompOn;
        public bool avaliLayEggs;
        public Dictionary<string, bool> enabledRaces = new Dictionary<string, bool>();
        public override void ExposeData()
        {
            Scribe_Values.Look(ref packLossEnabled, "packLossEnabled", true);
            Scribe_Values.Look(ref packsEnabled, "packsEnabled", true);
            Scribe_Values.Look(ref checkOtherRaces, "checkOtherRaces", true);
            Scribe_Values.Look(ref allowAllRaces, "allowAllRaces", false);
            Scribe_Values.Look(ref maxPackSize, "maxPackSize", 5);
            Scribe_Values.Look(ref avaliLayEggs, "avaliLayEggs", false);
            Scribe_Values.Look(ref enableDebugMode, "debugModeOn", false);
            Scribe_Values.Look(ref mapCompOn, "mapCompOn", true);
            Scribe_Values.Look(ref textEnabled, "textEnabled", true);
            Scribe_Collections.Look<string, bool>(ref enabledRaces, "enabledRaces",LookMode.Undefined, LookMode.Undefined);
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
            

        }
        
        private Vector2 vector = Vector2.left;
        Rect rect = new Rect();
        private bool hasRun = false;
        private bool saidDebugWasEnabled;
        private int settingsAreStable = 0;
        private bool resetSettings;
        private void LogDebugOn()
        {
            if(!saidDebugWasEnabled & settings.enableDebugMode)
            {
                Log.Error("This is not an error, but RimVali debugging has been enabled. This may affect some functions.");
                saidDebugWasEnabled = true;
            }
        }
        private void ShowRaces()
        {
            if (!hasRun & settings.enableDebugMode)
            {
                string racesFound = "Races found: ";
                foreach (ThingDef race in RimvaliPotentialPackRaces.potentialRaces)
                {
                    racesFound = racesFound + ", " + race.defName;
                }
                hasRun = false;
                Log.Message(racesFound);
            }
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            rect.Set(inRect.x, inRect.y - 2, 500, 5000);
            if (this.settings.enabledRaces == null)
            {
                this.settings.enabledRaces = new Dictionary<string, bool>();
            }
            Rect WidgetRect = rect.LeftHalf().BottomPart(0.1f);
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.BeginScrollView(inRect, ref vector, ref rect);
            listing_Standard.Gap(50);
            listing_Standard.Label("        Pack settings");
            listing_Standard.GapLine(10);
            listing_Standard.CheckboxLabeled("Pack loss enabled", ref settings.packLossEnabled, "Enable/disable pack loss.");
            listing_Standard.CheckboxLabeled("Packs enabled", ref settings.packsEnabled, "Enable/disable packs");
            listing_Standard.CheckboxLabeled("Enable other avali", ref settings.checkOtherRaces, "Pull any other potential 'avali' races from other mods, and factor them into the pack system. ");
            listing_Standard.CheckboxLabeled("Enable all races", ref settings.allowAllRaces, "Allow all races to join packs.");
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
            listing_Standard.Label("Maximum pack size: " + settings.maxPackSize.ToString(), -1, "RimVali was made to play this way.".Colorize(Color.green));
            settings.maxPackSize = (int)listing_Standard.Slider(settings.maxPackSize, 2, 50);
            listing_Standard.GapLine(10);
            listing_Standard.Label("        Avali settings");
            listing_Standard.GapLine(10);
            listing_Standard.CheckboxLabeled("Avali can have eggs", ref settings.avaliLayEggs, "Enable/disable eggs");
            listing_Standard.CheckboxLabeled("Display text (chirp, peep, etc)", ref settings.textEnabled);
            if (this.settings.enableDebugMode)
            {
                listing_Standard.GapLine(10);
                listing_Standard.Label("        Debug settings");
                listing_Standard.GapLine(10);
                listing_Standard.CheckboxLabeled("Enable map component", ref settings.mapCompOn);
            }

            if (settingsAreStable == 0)
            {
               resetSettings = listing_Standard.ButtonText("Stable");
            }
            else if (settingsAreStable == 1) { 
               resetSettings= listing_Standard.ButtonText("Potential issues");
            }
            else
            {
                resetSettings = listing_Standard.ButtonText("Unstable; Use at your own risk.");
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
            bool debugButton = listing_Standard.ButtonText("Toggle debug mode");
            if (debugButton)
            {
                settings.enableDebugMode = !settings.enableDebugMode;
                debugButton = !debugButton;
            }
            listing_Standard.EndScrollView(ref inRect);
            base.DoSettingsWindowContents(inRect);

        }
        public override string SettingsCategory()
        {
            return "RimVali";
        }
    }
}