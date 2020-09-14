using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public Dictionary<string, bool> enabledRaces;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref packLossEnabled, "packLossEnabled", true);
            Scribe_Values.Look(ref packsEnabled, "packsEnabled", true);
            Scribe_Values.Look(ref checkOtherRaces, "checkOtherRaces", true);
            Scribe_Values.Look(ref allowAllRaces, "allowAllRaces", false);
            Scribe_Values.Look(ref maxPackSize, "maxPackSize", 5);
            Scribe_Values.Look(ref avaliLayEggs, "avaliLayEggs", false);
            Scribe_Collections.Look<string, bool>(ref enabledRaces, "enabledRaces");
            Scribe_Values.Look(ref enableDebugMode, "debugModeOn", false);
            Scribe_Values.Look(ref mapCompOn, "mapCompOn", true);
            Scribe_Values.Look(ref textEnabled, "textEnabled", true);
            base.ExposeData();
        }
    }
    public class RimValiMod : Mod
    {
        RimValiModSettings settings;
        public RimValiMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<RimValiModSettings>();
        }
        private Vector2 vector = Vector2.left;
        Rect rect = new Rect();
        private bool hasRun = false;
        private bool saidDebugWasEnabled;
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

            Rect WidgetRect = rect.LeftHalf().BottomPart(0.1f);
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.BeginScrollView(inRect, ref vector, ref rect);
            listing_Standard.Gap(50);
            listing_Standard.Label("        Pack settings");
            listing_Standard.Gap(10);
            listing_Standard.CheckboxLabeled("Pack loss enabled", ref settings.packLossEnabled, "Enable/disable pack loss.");
            listing_Standard.CheckboxLabeled("Packs enabled", ref settings.packsEnabled, "Enable/disable packs");
            listing_Standard.CheckboxLabeled("Enable other avali", ref settings.checkOtherRaces, "Pull any other potential 'avali' races from other mods, and factor them into the pack system. ");
            listing_Standard.CheckboxLabeled("Enable all races", ref settings.allowAllRaces, "Allow all races to join packs.");
            try
            {
                ShowRaces();
                foreach (ThingDef race in RimvaliPotentialPackRaces.potentialRaces)
                {
                    bool checkOn;
                    bool isEnabled = this.settings.enabledRaces.TryGetValue(race.defName, out checkOn);
                    listing_Standard.CheckboxLabeled(race.label, ref isEnabled);
                    this.settings.enabledRaces.SetOrAdd<string, bool>(race.defName, isEnabled);
                }
            }
            catch
            {
                listing_Standard.Label("RimVali '".Colorize(Color.red) + RimValiUtility.build.Colorize(Color.red) + "' was unable to show this item! We're sorry for any inconvience. :(".Colorize(Color.red));
                listing_Standard.CheckboxLabeled("Enable debug mode".Colorize(Color.red), ref settings.enableDebugMode, "It appears RimVali encountered an error. You can use debug mode to return logs with more information on what RimVali was doing. [WIP]".Colorize(Color.red));
            }
            LogDebugOn();
            listing_Standard.Gap(10);
            if ((settings.maxPackSize < 20 & settings.maxPackSize >10) | settings.maxPackSize < 3)
            {
                listing_Standard.Label("Maximum pack size: ".Colorize(Color.yellow) + settings.maxPackSize.ToString().Colorize(Color.yellow), -1,"This size may cause issues.".Colorize(Color.yellow));
            }
            else if(settings.maxPackSize >= 20)
            {
                listing_Standard.Label("Maximum pack size: ".Colorize(Color.red) + settings.maxPackSize.ToString().Colorize(Color.red), -1, "RimVali can and will have issues with this setting. It is not recommended. USE AT YOUR OWN RISK.".Colorize(Color.red));
            }
            else
            {
                listing_Standard.Label("Maximum pack size: " + settings.maxPackSize.ToString(), -1, "RimVali was made to play this way.".Colorize(Color.green));
            }
            settings.maxPackSize = (int)listing_Standard.Slider(settings.maxPackSize, 2, 50);
            listing_Standard.Gap(10);
            listing_Standard.Label("        Avali settings");
            listing_Standard.Gap(10);
            listing_Standard.CheckboxLabeled("Avali can have eggs", ref settings.avaliLayEggs, "Enable/disable eggs");
            listing_Standard.CheckboxLabeled("Display text (chirp, peep, etc)", ref settings.textEnabled);
            if (this.settings.enableDebugMode)
            {
                listing_Standard.Gap(10);
                listing_Standard.Label("        Debug settings");
                listing_Standard.Gap(10);
                listing_Standard.CheckboxLabeled("Enable map component", ref settings.mapCompOn);
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