using RimWorld;
using RimWorld.Planet;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System;
using System.Threading.Tasks;

namespace AvaliMod
{
    public class AvaliPackDriver : GameComponent//WorldComponent//MapComponent//
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
        public List<AvaliPack> packs = new List<AvaliPack>();
        private int onTick = 0;
        public Dictionary<Pawn, bool> pawnsHaveHadPacks = new Dictionary<Pawn, bool>();
        public List<Pawn> pawns = new List<Pawn>();
        public List<bool> bools = new List<bool>();

        //public AvaliPackDriver(Map map) : base(map) { }

        //public AvaliPackDriver(World world) : base(world) { }

        public AvaliPackDriver(Game game) { }//: base(game) { }

        List<ThingDef> racesInPacks = new List<ThingDef>();

        public override void StartedNewGame()
        {
            packs = new List<AvaliPack>();
            racesInPacks.Add(AvaliDefs.RimVali);
            if (checkOtherRaces)
            {
                foreach (ThingDef race in RimValiDefChecks.potentialPackRaces)
                {
                    racesInPacks.Add(race);
                }
            }
            if (allowAllRaces)
            {
                foreach (ThingDef race in RimValiDefChecks.potentialRaces)
                {
                    if (otherRaces.TryGetValue(race.defName) == true)
                    {
                        racesInPacks.Add(race);
                        if (enableDebug)
                        {
                            Log.Message("Adding race: " + race.defName + " to racesInPacks.");
                        }
                    }
                }
            }
        }

        public override void LoadedGame()
        {
           
            racesInPacks.Add(AvaliDefs.RimVali);
            if (checkOtherRaces)
            {
                foreach (ThingDef race in RimValiDefChecks.potentialPackRaces)
                {
                    racesInPacks.Add(race);
                }
            }
            if (allowAllRaces)
            {
                foreach (ThingDef race in RimValiDefChecks.potentialRaces)
                {
                    if (otherRaces.TryGetValue(race.defName) == true)
                    {
                        racesInPacks.Add(race);
                        if (enableDebug)
                        {
                            Log.Message("Adding race: " + race.defName + " to racesInPacks.");
                        }
                    }
                }
            }
            base.LoadedGame();
        }
        public void LoadAll()
        {
            packs = new List<AvaliPack>();
            if (!HasStarted)
            {
                
                racesInPacks.Add(AvaliDefs.RimVali);
                if (checkOtherRaces)
                {
                    foreach(ThingDef race in RimValiDefChecks.potentialPackRaces)
                    {
                        racesInPacks.Add(race);
                    }
                }
                if (allowAllRaces)
                {
                    foreach(ThingDef race in RimValiDefChecks.potentialRaces)
                    {
                        if(otherRaces.TryGetValue(race.defName) == true)
                        {
                            racesInPacks.Add(race);
                            if (enableDebug)
                            {
                                Log.Message("Adding race: " + race.defName + " to racesInPacks.");
                            }
                        }
                    }
                }
                HasStarted = true;
            }
        }
        
        public override void ExposeData()
        {
            if (pawnsHaveHadPacks.EnumerableNullOrEmpty() == false)
            {
                foreach (Pawn pawn in pawnsHaveHadPacks.Keys)
                {
                    pawns.Add(pawn);
                    bools.Add(pawnsHaveHadPacks[pawn]);
                }
            }
            //Scribe_Collections.Look<AvaliPack>(ref packs, "packs", LookMode.Deep);
            //Scribe_Collections.Look<Pawn, bool>(ref pawnsHaveHadPacks, "pawnsHasHadPacks", LookMode.Reference, LookMode.Undefined, ref pawns, ref bools);
            Scribe_Collections.Look<Pawn, bool>(ref pawnsHaveHadPacks, "pawnsHasHadPacks", LookMode.Reference, LookMode.Undefined, ref pawns, ref bools);
            Scribe_Collections.Look<AvaliPack>(ref packs, "packs", LookMode.Deep);


            if (pawnsHaveHadPacks == null)
            {
                pawnsHaveHadPacks = new Dictionary<Pawn, bool>();
            }
            if (packs == null)
            {
                Log.Message("packs was null");
                packs = new List<AvaliPack>();
            }
            if (pawns == null)
            {
                Log.Message("pawns was null");
                pawns = new List<Pawn>();
            }
            if (bools == null)
            {
                Log.Message("bools was null");
                bools = new List<bool>();
            }
            base.ExposeData();
        }
        public void UpdatePacks()
        {
            if (enableDebug && multiThreaded)
            {
                //Log.Message("Thread started.");
            }
            IEnumerable<Pawn> pawnsInWorld = RimValiUtility.AllPawnsOfRaceInWorld(racesInPacks).Where<Pawn>(x => RimValiUtility.GetPackSize(x) < maxSize);
            foreach (Pawn pawn in pawnsInWorld)
            {
//Log.Message(pawn.Faction.Name);
                //Log.Message(pawn.Name.ToString() + " updatePacks()");
                PackComp comp = pawn.TryGetComp<PackComp>();
                if (!(comp == null))
                {
                    //Pull the comp info from the pawn
                    SimpleCurve ageCurve = comp.Props.packGenChanceOverAge;
                    //Tells us that this pawn has had a pack
                    if (enableDebug)
                    {
                        //Log.Message("Attempting to make pack.. [Base pack]");
 
                    }
                    //Makes the pack.
                    //Log.Message("EiPackHandlerFromPackDriverMapComp started.");
                    packs = RimValiUtility.EiPackHandler(packs, pawn, racesInPacks, maxSize);
                }
            }
        }

        //public override void MapComponentTick()
        //public override void WorldComponentTick()
        public override void GameComponentTick()
        {
            if (!HasStarted)
            {
                //LoadAll();
            }
            if (onTick == 0)
            {
                if (packsEnabled)
                {
                    if (multiThreaded)
                    {
                        if (enableDebug)
                        {
                            //Log.Message("Attempting to make new thread.");
                        }
                        /*ThreadStart packThreadRef = new ThreadStart(UpdatePacks);
                        Thread packThread = new Thread(packThreadRef);
                        packThread.Start();
                        */
                        Task packTask = new Task(UpdatePacks);
                        packTask.Start();
                        packTask.Wait();
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