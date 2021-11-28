using RimWorld;                    
using RimWorld.Planet;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System;
using System.Threading.Tasks;
using static AvaliMod.RimValiUtility;

namespace AvaliMod
{
    public class AvaliPackDriver : WorldComponent//MapComponent//
    {

        /*
        private readonly bool packsEnabled = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packsEnabled;
      
        private readonly bool multiThreaded = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packMultiThreading;
        public static int tickTime = RimValiMod.settings.ticksBetweenPackUpdate;
      
        private bool ThreadIsActive;
        private HashSet<AvaliPack> packs = new HashSet<AvaliPack>();
        private int onTick = 0;
        private Dictionary<Pawn, int> pawnHasHadXPacks = new Dictionary<Pawn, int>();
        public List<Pawn> pawns = new List<Pawn>();
        public List<int> count = new List<int>();
        public HashSet<Pawn> workingPawnHashset = new HashSet<Pawn>();




        //public AvaliPackDriver(Map map) : base(map) { }

        //public AvaliPackDriver(World world) : base(world) { }



        public List<ThingDef> racesInPacks = new List<ThingDef>();

        public HashSet<AvaliPack> Packs
        {
            get
            {
                return packs;
            }
        }

        public void MakePawnLeavePack(Pawn pawn, AvaliPack pack)
        {
            pack.pawns.Remove(pawn);
        }

        public void TransferPawnToPack(Pawn pawn, AvaliPack pack)
        {
            AvaliPack pawnPack = pawn.GetPack();
            if (pawnPack != null)
            {
                pawnPack.pawns.Remove(pawn);
            }
            pack.pawns.Add(pawn);
        }

        public int GetPacksPawnHasHad(Pawn pawn)
        {
            return pawnHasHadXPacks.ContainsKey(pawn) ? pawnHasHadXPacks[pawn] : 0;
        }

        public void CleanupBadPacks()
        {
            int packInt = 0;
            while (packs.Where(bP => bP.GetAllNonNullPawns.EnumerableNullOrEmpty()).Count() > 0)
            {
                AvaliPack pack = packs.Where(bP => bP.GetAllNonNullPawns.EnumerableNullOrEmpty()).ToList()[packInt];
                packs.Remove(pack);
                packInt++;
            }
            
        }

        public void CleanupPawnPacks(Pawn pawn)
        {
            while (packs.Where(p => p.GetAllNonNullPawns.Contains(pawn)).Count() > 1) { packs.Remove(packs.Where(p => p.GetAllNonNullPawns.Contains(pawn)).Last()); }
            AvaliPack pawnPack = pawn.GetPack();
            if (pawnPack != null && pawnPack.GetAllNonNullPawns.Count > 1)
            {
                foreach (Pawn p in pawnPack.pawns)
                {
                    if (pawnHasHadXPacks.ContainsKey(pawn))
                        pawnHasHadXPacks[pawn]--;
                }
                pawnPack.CleanupPack(pawn);
                pawnPack.UpdateHediffForAllMembers();
            }
            
        }
        
        public void MakePawnHavePack(Pawn pawn)
        {
            if (!pawnHasHadXPacks.ContainsKey(pawn))
            {
                pawnHasHadXPacks[pawn] = 0;
            }
            pawnHasHadXPacks[pawn]++;
        }

        public bool PawnHasHadMoreThenOnePack(Pawn pawn)
        {
            if (!pawnHasHadXPacks.ContainsKey(pawn))
            {
                return false;
            }
            return pawnHasHadXPacks[pawn] > 1;
        }

        public bool PawnHasHadPack(Pawn pawn)
        {
            if (!pawnHasHadXPacks.ContainsKey(pawn))
            {
                return false;
            }
            return pawnHasHadXPacks[pawn] > 0;
        }

        public bool ContainsPack(AvaliPack pack)
        {
            return packs.Contains(pack);
        }
        public void AddPack(AvaliPack pack)
        {
            packs.Add(pack);
            foreach(Pawn pawn in pack.pawns.Where(p => PawnHasHadPack(p)))
            {
                MakePawnHavePack(pawn);
            }
        }
        
        public void ResetPacks()
        {
            pawnHasHadXPacks = new Dictionary<Pawn, int>();
            packs = new HashSet<AvaliPack>();
        }

        public bool HasPack(Pawn pawn)
        {
            if(packs.EnumerableNullOrEmpty())
                return false;
            return packs.Any(pack => pack.GetAllNonNullPawns.Contains(pawn));
        }



        

        
        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                pawnHasHadXPacks = new Dictionary<Pawn, int>();
                packs = new HashSet<AvaliPack>();
            }

            //Scribe_Collections.Look<AvaliPack>(ref packs, "packs", LookMode.Deep);
            //Scribe_Collections.Look<Pawn, bool>(ref pawnsHaveHadPacks, "pawnsHasHadPacks", LookMode.Reference, LookMode.Undefined, ref pawns, ref bools);
            Scribe_Collections.Look(ref pawnHasHadXPacks, "pawnHasHadXPacks", LookMode.Reference, LookMode.Undefined, ref pawns, ref count);
            Scribe_Collections.Look(ref packs, "packs", LookMode.Deep);


            if (pawnHasHadXPacks == null){pawnHasHadXPacks = new Dictionary<Pawn, int>();}
            if (packs == null){packs = new HashSet<AvaliPack>();}
            if (pawns == null){pawns = new List<Pawn>();}
            if (count == null){count = new List<int>();}
            base.ExposeData();
        }
        public void UpdatePacks()
        {
            lock (packs)
            {
                
                if (!RimValiUtility.PawnsInWorld.EnumerableNullOrEmpty())
                {
                    foreach (Pawn pawn in workingPawnHashset)
                    {
                       RimValiUtility.KioPackHandler(pawn);
                    }
                    if (!packs.EnumerableNullOrEmpty())
                    {
                        CleanupBadPacks();
                    }
                }
            }
            ThreadIsActive = false;
        }
        

        public bool CanStartNextThread
        {
            get
            {
                
                ThreadPool.GetAvailableThreads(out int wThreads, out int cThreads);
                return !ThreadIsActive && wThreads > 0; 
            }
        }
        bool hasKickedBack;

        public AvaliPackDriver(World world) : base(world)
        {
        }
        
        public override void WorldComponentTick()
        
        {
            try
            {
                workingPawnHashset = RimValiUtility.PawnsInWorld;
                if (onTick == 0 && packsEnabled && Find.CurrentMap != null)
                {
                    
                    if (multiThreaded && CanStartNextThread)
                    {

                        ThreadIsActive = true;
                        Task packTask = new Task(UpdatePacks);
                        packTask.Start();
                    }
                    else{UpdatePacks();}
                    onTick = tickTime;
                }
                else{onTick--;}
            }
            catch(Exception e)
            {
                if (!hasKickedBack)
                {
                    hasKickedBack = true;
                    tickTime += 60000;
                    Log.Warning("Kio pack handler has encountered an error, kicking back ticks between update by 60000");
                }
                Log.Error($"{e}");

            }
        }
        */
        public AvaliPackDriver(World world) : base(world)
        {
            ResetPacks();
        }

        private List<Pawn> pawnsThatHavePacks = new List<Pawn>();

        private List<AvaliPack> packs = new List<AvaliPack>();

        private Dictionary<Pawn, int> packCounter = new Dictionary<Pawn, int>();

        private Dictionary<Pawn, int> currentPack = new Dictionary<Pawn, int>();

        public int GetPackCount(Pawn pawn)
        {
            if (!packCounter.ContainsKey(pawn))
            {
                return 0;
            }
            return packCounter[pawn];
        }

        public void AddToPackCount(Pawn pawn)
        {
            if (!packCounter.ContainsKey(pawn))
            {
                packCounter.Add(pawn, 0);
            }
            packCounter[pawn]++;
        }

        public List<AvaliPack> Packs
        {
            get
            {
                return packs;
            }
        }

        public bool PawnHasPack(Pawn pawn) => pawnsThatHavePacks.Contains(pawn);
        public bool PawnsHasHadPack(Pawn pawn) => GetPackCount(pawn) > 0;

        private void RemovePack(AvaliPack pack)
        {
            packs.Remove(pack);
            foreach(Pawn pawn in pack.pawns)
            {
                pawnsThatHavePacks.Remove(pawn);
            }
        }

        public void AddPack(AvaliPack pack)
        {
            if (!packs.Contains(pack))
            {
                Log.Message($"Adding {pack} to packs");
                int ID = packs.Count;
                packs.Add(pack);
                foreach (Pawn pawn in pack.pawns)
                {
                    pawnsThatHavePacks.Add(pawn);
                    AddToPackCount(pawn);
                    currentPack[pawn] = ID;
                }
            }
        }

        public AvaliPack GetCurrentPack(Pawn pawn)
        {
            if (currentPack.ContainsKey(pawn))
            {
                return packs[currentPack[pawn]];
            }
            return null;
        }

        private void AddPawnToPack(Pawn pawn,ref AvaliPack pack)
        {
            Log.Message($"Adding {pawn.Name.ToStringShort} to {pack}");
            pack.pawns.Add(pawn);
            AddToPackCount(pawn);
            currentPack[pawn] = packs.IndexOf(pack);
        }

        private void RemovePawnFromPack(Pawn pawn, ref AvaliPack pack)
        {
            pack.pawns.Remove(pawn);
            if (currentPack.ContainsKey(pawn))
            {
                currentPack.Remove(pawn);
            }
        }

        public bool HasPack(Pawn pawn)
        {
            return pawnsThatHavePacks.Contains(pawn);
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref pawnsThatHavePacks, "pawnsThatHavePacks" ,LookMode.Reference);
            Scribe_Collections.Look(ref packCounter, "packCounter", LookMode.Reference);
            Scribe_Collections.Look(ref packs, "packs");

            base.ExposeData();
        }


        public void ResetPacks()
        {
            pawnsThatHavePacks = new List<Pawn>();
            packs = new List<AvaliPack>();
            packCounter = new Dictionary<Pawn, int>();
        }

        public void SwitchPack(Pawn pawn, AvaliPack newPack)
        {
            AvaliPack oldPack = GetCurrentPack(pawn);
            if (oldPack != null)
            {
                RemovePawnFromPack(pawn, ref oldPack);
            }
            AddPawnToPack(pawn, ref newPack);
        }

        int onTick = 0;
        private readonly bool packsEnabled = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packsEnabled;

        private readonly bool multiThreaded = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packMultiThreading;

        public HashSet<Pawn> workingPawnHashset = new HashSet<Pawn>();
        public static int tickTime = RimValiMod.settings.ticksBetweenPackUpdate;
        private bool hasKickedBack;
        private bool ThreadIsActive;

        public bool CanStartNextThread
        {
            get
            {

                ThreadPool.GetAvailableThreads(out int wThreads, out int cThreads);
                return !ThreadIsActive && wThreads > 0;
            }
        }

        public void UpdatePacks()
        {
            lock (packs)
            {
                if (!PawnsInWorld.EnumerableNullOrEmpty())
                {
                    Log.Message("Running update");
                    foreach (Pawn pawn in workingPawnHashset)
                    {
                        KioPackHandler(pawn);
                    }
                }
            }
            ThreadIsActive = false;
        }


        public override void WorldComponentTick()
        {
            try
            {
               
                workingPawnHashset = PawnsInWorld;
                if (onTick == 0 && packsEnabled && Find.CurrentMap != null)
                {
                    Log.Message("Running tick");
                    if (multiThreaded && CanStartNextThread)
                    {
                        Log.Message("Running thread");

                        ThreadIsActive = true;
                        Task packTask = new Task(UpdatePacks);
                        packTask.Start();
                    }
                    else { UpdatePacks(); }
                    onTick = tickTime;
                }
                else { onTick--; }
            }
            catch (Exception e)
            {
                if (!hasKickedBack)
                {
                    hasKickedBack = true;
                    tickTime += 60000;
                    Log.Warning("Kio pack handler has encountered an error, kicking back ticks between update by 60000");
                }
                Log.Error($"{e}");

            }
            base.WorldComponentTick();
        }
    }


}