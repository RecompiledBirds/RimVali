using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Verse;
using static AvaliMod.RimValiUtility;

namespace AvaliMod
{
    public class AvaliPackDriver : WorldComponent
    {
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

        public bool PawnHasHadPack(Pawn pawn) => GetPackCount(pawn) > 0;

        private void RemovePack(AvaliPack pack)
        {
            packs.Remove(pack);
            foreach (Pawn pawn in pack.pawns)
            {
                pawnsThatHavePacks.Remove(pawn);
            }
        }

        public void AddPack(AvaliPack pack)
        {
            if (!packs.Contains(pack))
            {
                Log.Message($"Adding {pack.GetUniqueLoadID()} to packs");
                packs.Add(pack);
                foreach (Pawn pawn in pack.pawns)
                {
                    pawnsThatHavePacks.Add(pawn);
                    AddToPackCount(pawn);
                    SetPawnPack(pawn, pack);
                }
            }
        }

        public AvaliPack GetCurrentPack(Pawn pawn)
        {
            if (currentPack.ContainsKey(pawn) && currentPack[pawn] != -1)
            {
                return packs[currentPack[pawn]];
            }
            Log.Message($"{pawn.Name.ToStringShort} does not have a pack!");
            return null;
        }

        public void AddPawnToPack(Pawn pawn, ref AvaliPack pack)
        {
            Log.Message($"Adding {pawn.Name.ToStringShort} to {pack}");
            pack.pawns.Add(pawn);
            AddToPackCount(pawn);
            pawnsThatHavePacks.Add(pawn);
            SetPawnPack(pawn, pack);
        }

        private void SetPawnPack(Pawn pawn, AvaliPack pack)
        {
            if (!currentPack.ContainsKey(pawn))
                currentPack.Add(pawn, -1);
            currentPack[pawn] = packs.IndexOf(pack);
        }

        private void RemovePawnFromPack(Pawn pawn, ref AvaliPack pack)
        {

            Log.Message($"Removing {pawn.Name.ToStringShort} to {pack}");
            pack.pawns.Remove(pawn);
            if (currentPack.ContainsKey(pawn))
            {
                currentPack.Remove(pawn);
            }
        }

        public bool HasPack(Pawn pawn)
        {
            Log.Message($"{pawn.Name.ToStringShort} has pack:{pawnsThatHavePacks.Contains(pawn)}");
            return pawnsThatHavePacks.Contains(pawn);
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref pawnsThatHavePacks, "pawnsThatHavePacks", LookMode.Reference);
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
