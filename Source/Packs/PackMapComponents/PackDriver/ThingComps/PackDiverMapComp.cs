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
        public bool ThreadIsActive;
        public List<AvaliPack> packs = new List<AvaliPack>();
        private int onTick = 0;
        public Dictionary<Pawn, bool> pawnsHaveHadPacks = new Dictionary<Pawn, bool>();
        public List<Pawn> pawns = new List<Pawn>();
        public List<bool> bools = new List<bool>();
        public List<Pawn> pawnsInWorld= new List<Pawn>();
        //public AvaliPackDriver(Map map) : base(map) { }

        //public AvaliPackDriver(World world) : base(world) { }

        public AvaliPackDriver(Game game) {
            StartedNewGame();
        }//: base(game) { }

        public List<ThingDef> racesInPacks = new List<ThingDef>();

        public override void StartedNewGame()
        {
            packs = new List<AvaliPack>();
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(x => x.HasComp(typeof(PackComp))))
            {
                racesInPacks.Add(def);
            }
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

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(x => x.HasComp(typeof(PackComp))))
            {
                racesInPacks.Add(def);
            }
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
            ConvertAvaliContinued();
            base.LoadedGame();
        }
        public Pawn ConPawn(Pawn pawn)
        {

            Thing newSpawn = GenSpawn.Spawn(AvaliDefs.RimVali, pawn.Position, pawn.Map);
            Pawn newPawn = newSpawn as Pawn;
            newPawn.skills = pawn.skills;
            newPawn.relations = pawn.relations;
            newPawn.Name = pawn.Name;
            newPawn.needs = pawn.needs;
            newPawn.health = pawn.health;
            newPawn.inventory = pawn.inventory;
            newPawn.interactions = pawn.interactions;
            newPawn.kindDef = pawn.kindDef;
            newPawn.story = pawn.story;
            pawn.Destroy();
            return newPawn;
        }
        public void ConvertAvaliContinued()
        {
            try
            {
                foreach (Pawn pawn in RimValiUtility.FetchAllAliveOrDeadPawns().Where(p => p.def.defName == "Avali"))
                {
                    Pawn nPawn = ConPawn(pawn);
                    RimValiRaceDef def = nPawn.def as RimValiRaceDef;
                    def.GenGraphics(nPawn);
                    
                }
            }
            catch
            {

            }
        }

        public void LoadAll()
        {
            packs = new List<AvaliPack>();
            if (!HasStarted)
            {
                foreach(ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(x => x.HasComp(typeof(PackComp))))
                {
                    racesInPacks.Add(def);
                }
                //racesInPacks.Add(AvaliDefs.RimVali);
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
            if (!pawnsHaveHadPacks.EnumerableNullOrEmpty())
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
                packs = new List<AvaliPack>();
            }
            if (pawns == null)
            {
                pawns = new List<Pawn>();
            }
            if (bools == null)
            {
                bools = new List<bool>();
            }
            base.ExposeData();
        }
        public void UpdatePacks()
        {
            lock (packs)
            {
                
                if (!pawnsInWorld.EnumerableNullOrEmpty())
                {
                    foreach (Pawn pawn in pawnsInWorld)
                    {
                        PackComp comp = pawn.TryGetComp<PackComp>();
                        if (!(comp == null))
                        {
                            packs = RimValiUtility.EiPackHandler(packs, pawn, maxSize);
                        }
                    }
                }
            }
            ThreadIsActive = false;
        }

        public override void GameComponentTick()
        {

            if (onTick == 0 && packsEnabled && Find.CurrentMap != null)
            {
                //pawnsInWorld = RimVali.RimValiMapComponent.GetRimValiPawnTracker(Find.CurrentMap).AllPawnsOfRaceInWorld(racesInPacks).ToList();
                pawnsInWorld = RimValiUtility.AllPawnsOfRaceInWorld(racesInPacks).ToList();
                if (multiThreaded && !ThreadIsActive)
                {
                    
                    ThreadIsActive = true;
                    Task packTask = new Task(UpdatePacks);
                    packTask.Start();
                }
                else
                {
                    UpdatePacks();
                }
                onTick = 120;
            }
            else
            {
                onTick--;
            }
        }
    }

    public class Converter : WorldComponent
    {
        public Converter(World world) : base(world)
        {
        }
        public void ConPawn(Pawn pawn, ThingDef def)
        {
            Pawn newPawn = PawnGenerator.GeneratePawn(pawn.kindDef, pawn.Faction);
            newPawn.def = def;
            newPawn.skills = pawn.skills;
            newPawn.relations = pawn.relations;
            newPawn.Name = pawn.Name;
            newPawn.needs = pawn.needs;
            newPawn.health = pawn.health;
            newPawn.inventory = pawn.inventory;
            newPawn.interactions = pawn.interactions;
            newPawn.story = pawn.story;
            try
            {
                
                GenSpawn.Spawn(newPawn, pawn.Position, pawn.Map);
                RimValiRaceDef d = newPawn.def as RimValiRaceDef;
                d.GenGraphics(newPawn);
                //pawn.Destroy();
            }
            catch(Exception e)
            {
                Log.Error(e.Message);
            }
            
        }
        public void ConvertAvaliContinued()
        {
            try
            {
                foreach (Pawn pawn in RimValiUtility.FetchPawnsOnAllMaps().Where(p => p.def.defName == "Avali"))
                {
                    Log.Message("test world comp");
                    if (pawn != null)
                    {
                        ConPawn(pawn, AvaliMod.AvaliDefs.RimVali);
                    }
                    Log.Message("test2");
                    

                }
            }
            catch(Exception e)
            {
                Log.Message(e.Message);
            }
        }
        private bool started;
        public override void WorldComponentTick()
        {
            if (!started)
            {
                started = true;
                ConvertAvaliContinued();
            }
            base.WorldComponentTick();
        }
    }
}