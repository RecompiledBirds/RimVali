using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Rimvali.Rewrite.Packs;
using RimValiCore;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace AvaliMod
{
    public class AvaliUpdater : WorldComponent
    {
        private readonly bool mapCompOn =
            LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().mapCompOn;


        private int onTick;
        public HashSet<Pawn> pawns = new HashSet<Pawn>();
        private List<Pawn> pawnsAreMissing = new List<Pawn>();
        private List<Pawn> pawnsHaveBeenSold = new List<Pawn>();
        private bool threadRunning;

        public AvaliUpdater(World map)
            : base(map)
        {
        }

        public bool CanStartNextThread
        {
            get
            {
                ThreadPool.GetAvailableThreads(out int wThreads, out _);
                return !threadRunning && wThreads > 0;
            }
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref pawnsHaveBeenSold, "soldPawns", LookMode.Reference);
            Scribe_Collections.Look(ref pawnsAreMissing, "missingPawns", LookMode.Reference);
        }

        public bool CheckIfLost(Pawn pawn)
        {
            TaleManager manager = Find.TaleManager;
            manager.AllTalesListForReading.Any(x => x.DominantPawn == pawn);
            if (pawnsAreMissing.Count > 0 && pawn.IsKidnapped() && !pawnsAreMissing.Contains(pawn))
            {
                pawnsAreMissing.Add(pawn);
                return true;
            }

            return false;
        }

        public void UpdatePawns()
        {
            if (!RimValiUtility.Driver.Packs.EnumerableNullOrEmpty())
            {
                foreach (AvaliPack pack in RimValiUtility.Driver.Packs)
                {
                    foreach (Pawn pawn in pack.GetAllNonNullPawns)
                    {
                        var packComp = pawn.TryGetComp<PackComp>();
                        AvaliPack pawnPack = RimValiUtility.Driver.GetCurrentPack(pawn);
                        GivePackThoughts(pawnPack, packComp);
                    }
                }
            }
        }

        private void GivePackThoughts(AvaliPack pawnPack, PackComp packComp)
        {
            if (pawnPack != null && packComp != null)
            {
                foreach (Pawn packmate in pawnPack.GetAllNonNullPawns.Where(x => x.Alive() && x != pawnPack.leaderPawn))
                {
                    var thought_Memory2 = (Thought_Memory)ThoughtMaker.MakeThought(packComp.Props.togetherThought);
                    if (packmate != null && pawnPack.leaderPawn != null &&
                        !thought_Memory2.TryMergeWithExistingMemory(out bool _))
                    {
                        packmate.needs.mood.thoughts.memories.TryGainMemory(thought_Memory2, pawnPack.leaderPawn);
                    }
                }
            }
        }

        public void UpdateThreaded()
        {
            UpdatePawns();
            threadRunning = false;
        }

        int fails = 0;
        private void UpdateV2()
        {
            //A simple check to stop if we are having repetitive issues.
            if (fails >= 5)
                return;
            try
            {
                PacksV2WorldComponent packsComponent = Find.World.GetComponent<PacksV2WorldComponent>();
                if (packsComponent == null || packsComponent.PacksReadOnly.EnumerableNullOrEmpty())
                    return;

                foreach (Pack pack in packsComponent.PacksReadOnly)
                {
                    if (pack != null && !pack.GetAllPawns.EnumerableNullOrEmpty())
                    {
                        IEnumerable<Pawn> pawns = pack.GetAllPawns.Where(x => x.Alive() && x != pack.Leader);
                        if (!pawns.EnumerableNullOrEmpty())
                        {
                            foreach (Pawn pawn in pawns)
                            {
                                var packComp = pawn.TryGetComp<PackComp>();
                                var thought_Memory2 = (Thought_Memory)ThoughtMaker.MakeThought(packComp.Props.togetherThought);
                                if (pawn != null && pack.Leader != null && !thought_Memory2.TryMergeWithExistingMemory(out bool _))
                                {
                                    pawn.needs.mood.thoughts.memories.TryGainMemory(thought_Memory2, pack.Leader);
                                }
                            }
                        }
                    }
                }
                fails = 0;
            }
            catch(Exception error)
            {
                Log.ErrorOnce($"{error}",1);
                fails++;
            }
        }

        

        public override void WorldComponentTick()
        {
            if (!PacksV2WorldComponent.EnhancedMode)
            {
                if (RimValiMod.settings.packThoughtsEnabled && mapCompOn && !(RimValiUtility.Driver.Packs == null) &&
                    RimValiUtility.Driver.Packs.Count > 0)
                {
                    if (onTick == 120)
                    {
                        pawns = RimValiUtility.PawnsInWorld;
                        if (CanStartNextThread)
                        {
                            threadRunning = true;
                            ThreadStart packThreadRef = UpdateThreaded;
                            var packThread = new Thread(packThreadRef);
                            packThread.Start();
                        }
                        else
                        {
                            UpdatePawns();
                        }

                        onTick = 0;
                    }

                    onTick++;
                }
            }
            else
            {
                UpdateV2();
            }
        }
    }
}
