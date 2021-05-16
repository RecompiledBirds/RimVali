using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System;

namespace AvaliMod
{
    public class AvaliPackDriver : MapComponent
    {
        private RimValiModSettings settings = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>();
        private readonly bool enableDebug = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().enableDebugMode;
        private readonly int maxSize = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().maxPackSize;
        private readonly bool packsEnabled = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packsEnabled;
        private readonly bool checkOtherRaces = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().checkOtherRaces;
        private readonly bool allowAllRaces = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().allowAllRaces;
        private readonly bool multiThreaded = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packMultiThreading;
        private readonly Dictionary<string, bool> otherRaces = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().enabledRaces;
        private bool HasStarted = false;
        public static List<AvaliPack> packs = new List<AvaliPack>();
        private int onTick = 0;
        private int onOtherTick = 0;
        public Dictionary<Pawn, bool> pawnsHaveHadPacks = new Dictionary<Pawn, bool>(new PawnEqaulityComparer());
        public AvaliPackDriver(Map map)
            : base(map)
        {

        }
        List<ThingDef> racesInPacks = new List<ThingDef>();
        public void LoadAll()
        {
            if (!HasStarted)
            {
                racesInPacks.Add(AvaliDefs.RimVali);
                if (checkOtherRaces)
                {
                    foreach(ThingDef race in RimvaliPotentialPackRaces.potentialPackRaces)
                    {
                        racesInPacks.Add(race);
                    }
                }
                if (allowAllRaces)
                {
                    foreach(ThingDef race in RimvaliPotentialPackRaces.potentialRaces)
                    {
                        if(otherRaces.TryGetValue(race.defName) == true)
                        {
                            racesInPacks.Add(race);
                        }
                    }
                }
                HasStarted = true;
            }
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look<AvaliPack>(ref packs, "packs", LookMode.Deep, Array.Empty<object>());
            base.ExposeData();
        }

        public void UpdatePacks()
        {
            if (enableDebug && multiThreaded)
            {
                Log.Message("Thread started.");
            }
            IEnumerable<Pawn> pawnsOnMap = RimValiUtility.AllPawnsOfRaceOnMap(AvaliDefs.RimVali, map).Where<Pawn>(x => RimValiUtility.GetPackSize(x, x.TryGetComp<PackComp>().Props.relation) < maxSize);
            foreach (Pawn pawn in pawnsOnMap)
            {
                //Log.Message(pawn.Name.ToString() + " updatePacks()");
                PackComp comp = pawn.TryGetComp<PackComp>();
                if (!(comp == null))
                {
                    //Pull the comp info from the pawn
                    SimpleCurve ageCurve = comp.Props.packGenChanceOverAge;
                    //Tells us that this pawn has had a pack
                    if (enableDebug)
                    {
                        Log.Message("Attempting to make pack.. [Base pack]");
 
                    }
                    //Makes the pack.
                    packs = RimValiUtility.EiPackHandler(packs, pawn, racesInPacks, maxSize);
                }
            }
        }

        public override void MapComponentTick()
        {
            if (!HasStarted)
            {
                LoadAll();
            }
            if (onTick == 0)
            {
                if (packsEnabled)
                {
                    if (multiThreaded)
                    {
                        if (enableDebug)
                        {
                            Log.Message("Attempting to make new thread.");
                        }
                        ThreadStart packThreadRef = new ThreadStart(UpdatePacks);
                        Thread packThread = new Thread(packThreadRef);
                        packThread.Start();
                    }
                    else
                    {
                        UpdatePacks();
                    }
                   /* Log.Message(packs.Count.ToString());
                    if (packs.Count > 0)
                    {
                        Log.Message(packs[0].name);
                    }*/
                }
                onTick = 120;
            }
            else
            {
                onTick--;
            }
        }

    }
}