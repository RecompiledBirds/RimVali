using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RimWorld.Planet;
using Verse;

namespace AvaliMod
{
    public class AvaliPackDriver : WorldComponent
    {
        public static int tickTime = RimValiMod.settings.ticksBetweenPackUpdate;

        private readonly Dictionary<Pawn, int> currentPack = new Dictionary<Pawn, int>();

        private readonly bool multiThreaded =
            LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packMultiThreading;

        private readonly bool packsEnabled =
            LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packsEnabled;

        private bool hasKickedBack;

        private int onTick;

        private Dictionary<Pawn, int> packCounter = new Dictionary<Pawn, int>();

        private List<AvaliPack> packs = new List<AvaliPack>();

        private List<Pawn> pawnsThatHavePacks = new List<Pawn>();
        private bool ThreadIsActive;

        public HashSet<Pawn> workingPawnHashset = new HashSet<Pawn>();

        public AvaliPackDriver(World world) : base(world)
        {
            ResetPacks();
        }

        public List<AvaliPack> Packs => packs;

        public bool CanStartNextThread
        {
            get
            {
                ThreadPool.GetAvailableThreads(out int wThreads, out _);
                return !ThreadIsActive && wThreads > 0;
            }
        }

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

        public bool PawnHasPack(Pawn pawn)
        {
            return pawnsThatHavePacks.Contains(pawn);
        }

        public bool PawnsHasHadPack(Pawn pawn)
        {
            return GetPackCount(pawn) > 0;
        }

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

            return null;
        }

        public void AddPawnToPack(Pawn pawn, ref AvaliPack pack)
        {
            pack.pawns.Add(pawn);
            AddToPackCount(pawn);
            pawnsThatHavePacks.Add(pawn);
            SetPawnPack(pawn, pack);
        }

        private void SetPawnPack(Pawn pawn, AvaliPack pack)
        {
            if (!currentPack.ContainsKey(pawn))
            {
                currentPack.Add(pawn, -1);
            }

            currentPack[pawn] = packs.IndexOf(pack);
            Log.Message($"Set packID for {pawn.Name.ToStringShort}: {packs.IndexOf(pack)}");
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
            var pawns = new List<Pawn>();
            var ints = new List<int>();
            Scribe_Collections.Look(ref pawnsThatHavePacks, "pawnsThatHavePacks", LookMode.Reference);
            Scribe_Collections.Look(ref packCounter, "packCounter", LookMode.Reference, LookMode.Value, ref pawns,
                ref ints);
            Scribe_Collections.Look(ref packs, "packs", LookMode.Deep);

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

        public void UpdatePacks()
        {
            lock (packs)
            {
                if (!workingPawnHashset.EnumerableNullOrEmpty())
                {
                    foreach (Pawn pawn in workingPawnHashset)
                    {
                        RimValiUtility.KioPackHandler(pawn);
                    }
                }

                CleanupPacks();
            }

            ThreadIsActive = false;
        }

        public void CleanupPacks()
        {
            IEnumerable<AvaliPack> packsToRemove = packs.Where(p => p.pawns.EnumerableNullOrEmpty());
            foreach (AvaliPack pack in packsToRemove)
            {
                packs.Remove(pack);
            }
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
                        var packTask = new Task(UpdatePacks);
                        packTask.Start();
                    }
                    else
                    {
                        UpdatePacks();
                    }

                    onTick = tickTime;
                }
                else
                {
                    onTick--;
                }
            }
            catch (Exception e)
            {
                if (!hasKickedBack)
                {
                    hasKickedBack = true;
                    tickTime += 60000;
                    Log.Warning(
                        "Kio pack handler has encountered an error, kicking back ticks between update by 60000");
                }

                Log.Error(e.ToString());
            }

            base.WorldComponentTick();
        }
    }
}
