using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Verse;
namespace AvaliMod
{
    public class AvaliPackDriver : WorldComponent//MapComponent//
    {


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

        public HashSet<AvaliPack> Packs => packs;

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
                    {
                        pawnHasHadXPacks[pawn]--;
                    }
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
            foreach (Pawn pawn in pack.pawns.Where(p => PawnHasHadPack(p)))
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
            if (packs.EnumerableNullOrEmpty())
            {
                return false;
            }

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


            if (pawnHasHadXPacks == null) { pawnHasHadXPacks = new Dictionary<Pawn, int>(); }
            if (packs == null) { packs = new HashSet<AvaliPack>(); }
            if (pawns == null) { pawns = new List<Pawn>(); }
            if (count == null) { count = new List<int>(); }
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

                ThreadPool.GetAvailableThreads(out int wThreads, out _);
                return !ThreadIsActive && wThreads > 0;
            }
        }

        private bool hasKickedBack;

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
        }
    }


}