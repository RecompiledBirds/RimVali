using System.Collections.Generic;
using Verse;

namespace AvaliMod
{
    public class RimValiModSettings : ModSettings
    {
        public int AERIALShellCap;
        public bool allowAllRaces;
        public bool avaliLayEggs;
        public int avaliRequiredForDrop;
        public bool canGetPackBroken;
        public bool checkOtherRaces;
        public bool enableAirdrops;
        public bool enableDebugMode;
        public Dictionary<string, bool> enabledRaces;
        public int hackChance;
        public float healthScale;
        public int IlluminateAvaliMaxTemp;
        public int IlluminateAvaliMinTemp;
        public int IWAvaliMaxTemp;
        public int IWAvaliMinTemp;
        public bool mapCompOn;
        public int maxPackSize;
        public int packBrokenChance;
        public bool packLossEnabled;
        public int packOpReq;
        public bool packsEnabled;
        public bool packThoughtsEnabled;
        public int stageOneDaysPackloss;
        public int stageThreeDaysPackloss;
        public int stageTwoDaysPackloss;
        public bool textEnabled;
        public int ticksBetweenPackUpdate;
        public bool unstable;

        public RimValiModSettings()
        {
            ticksBetweenPackUpdate = 120;
            packThoughtsEnabled = true;
            enabledRaces = new Dictionary<string, bool>();
            healthScale = 1.3f;
            packLossEnabled = true;
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
            unstable = true;
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
            Scribe_Collections.Look(ref enabledRaces, "enabledRaces");
            Scribe_Values.Look(ref packBrokenChance, "packBrokenChance", 5, true);
            Scribe_Values.Look(ref ticksBetweenPackUpdate, "ticksBetweenPackUpdate", 120, true);
            Scribe_Values.Look(ref canGetPackBroken, "canGetPackBroken", true, true);
            Scribe_Values.Look(ref unstable, "unstable", false, true);
            base.ExposeData();
        }
    }
}
