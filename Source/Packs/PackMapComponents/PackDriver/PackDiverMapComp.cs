using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public List<AvaliPack> packs;
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
            Scribe_Values.Look<List<AvaliPack>>(ref packs, "packs", new List<AvaliPack>(), true);
            base.ExposeData();
        }


        public void MakeNewPacks()
        {
            if (enableDebug && multiThreaded)
            {
                Log.Message("Thread started.");
            }
            IEnumerable<Pawn> pawnsOnMap = RimValiUtility.AllPawnsOfRaceOnMap(AvaliDefs.RimVali, map).Where<Pawn>(x => RimValiUtility.GetPackSize(x, x.TryGetComp<PackComp>().Props.relation) < maxSize);
            foreach (Pawn pawn in pawnsOnMap)
            {
                PackComp comp = pawn.TryGetComp<PackComp>();
                if (!(comp == null))
                {
                    //Pull the comp info from the pawn
                    PawnRelationDef relationDef = comp.Props.relation;
                    SimpleCurve ageCurve = comp.Props.packGenChanceOverAge;
                    //Tells us that this pawn has had a pack
                    if (enableDebug)
                    {
                        Log.Message("Attempting to make pack.. [New/added pack]");
                    }
                    //Makes the pack.
                    foreach (Pawn packmate in pawnsOnMap)
                    {
                        //RimValiUtility.KeoBuildMakeBasePack(pawn, relationDef, racesInPacks, maxSize);
                        RimValiUtility.EiBuildMakeBasePack(pawn, relationDef, racesInPacks, maxSize, packs);
                        if (RimValiUtility.GetPackSize(pawn, relationDef) <= 0)
                        {
                            RimValiUtility.EiBuildMakeBasePack(pawn, relationDef, racesInPacks, maxSize, packs);
                        }
                        else
                        {
                            RimValiUtility.EiBuildMakeBasePack(pawn, relationDef, racesInPacks, maxSize, packs);
                        }
        if (RimValiUtility.GetPackSize(pawn, relationDef) == maxSize)
        {
            break;
        }
    }
}
}
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
               
                PackComp comp = pawn.TryGetComp<PackComp>();
                if (!(comp == null))
                {
                    //Pull the comp info from the pawn
                    PawnRelationDef relationDef = comp.Props.relation;
                    SimpleCurve ageCurve = comp.Props.packGenChanceOverAge;
                    //Tells us that this pawn has had a pack
                    if (enableDebug)
                    {
                        Log.Message("Attempting to make pack.. [Base pack]");
 
                    }
                    //Makes the pack.
                    RimValiUtility.EiBuildMakeBasePack(pawn, relationDef, racesInPacks, maxSize, packs);
                }
            }
        }

        public override void MapComponentTick()
        {
            if (!HasStarted)
            {
                LoadAll();
            }
            if (onTick == 120)
            {
                if (packsEnabled)
                {
                    if (multiThreaded)
                    {
                        if (enableDebug)
                        {
                            Log.Message("Attempting to make new thread.");
                        }
                        ThreadStart packThreadRef = new ThreadStart(MakeNewPacks);
                        Thread packThread = new Thread(packThreadRef);
                        packThread.Start();
                    }
                    else
                    {
                        MakeNewPacks();
                    }
                }
                onTick = 0;
            }
            else
            {
                onTick += 1;
            }
            if(onOtherTick == 180)
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
                }
                onOtherTick = 0;
            }
            else
            {
                onOtherTick += 1;
            }
        }

    }
}