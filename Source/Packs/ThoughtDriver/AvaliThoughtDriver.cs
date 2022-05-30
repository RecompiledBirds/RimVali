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
            UpdateV2();
        }
    }
}
