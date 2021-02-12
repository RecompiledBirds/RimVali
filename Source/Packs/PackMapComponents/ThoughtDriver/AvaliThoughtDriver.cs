using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

using System.Threading;
namespace AvaliMod
{
    public class AvaliUpdater : MapComponent
    {
        private readonly bool multiThreaded = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packMultiThreading;
        private readonly bool mapCompOn = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().mapCompOn;
        private int onTick;
        private List<Pawn> pawnsHaveBeenSold = new List<Pawn>();
        private List<Pawn> pawnsAreMissing = new List<Pawn>();
        public AvaliUpdater(Map map)
            : base(map)
        {

        }
        public override void ExposeData()
        {
            Scribe_Collections.Look<Pawn>(ref pawnsHaveBeenSold, "soldPawns", LookMode.Reference);
            Scribe_Collections.Look<Pawn>(ref pawnsAreMissing, "missingPawns", LookMode.Reference);
        }

        public bool CheckIfLost(Pawn pawn)
        {
            TaleManager manager = new TaleManager();
          
            if (pawnsAreMissing.Count > 0 && pawn.IsKidnapped() && !pawnsAreMissing.Contains(pawn))
            {
                pawnsAreMissing.Add(pawn);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UpdatePawns(Map map)
        {
            AvaliPackDriver AvaliPackDriver = Current.Game.GetComponent<AvaliPackDriver>();
            IEnumerable<Pawn> pawns = RimValiUtility.CheckAllPawnsInMapAndFaction(map, Faction.OfPlayer).Where(x => x.def == AvaliDefs.RimVali);
            IEnumerable<AvaliPack> packs = AvaliPackDriver.packs;
            foreach (Pawn pawn in pawns)
            {
                AvaliThoughtDriver avaliThoughtDriver = pawn.TryGetComp<AvaliThoughtDriver>();
                PackComp packComp = pawn.TryGetComp<PackComp>();
                if (!(avaliThoughtDriver == null))
                {
                    //Log.Message("Pawn has pack comp, moving to next step...");
                    if (AvaliPackDriver.packs == null || AvaliPackDriver.packs.Count == 0)
                    {
                        //Log.Message("How did we get here? [Pack list was 0 or null]");
                        return;
                    }
                    AvaliPack pawnPack = null;
                    //This errors out when pawns dont have a pack, in rare cases. That is bad. This stops it from doing that.
                    try { pawnPack = RimValiUtility.GetPack(pawn); }
                    catch
                    {
                        return;
                    }
                    //Log.Message("Tried to get packs pack, worked.");
                    if (pawnPack == null)
                    {
                        //Log.Message("How did we get here? [Pack was null.]");
                        break;
                    }
                    foreach (Pawn packmate in pawnPack.pawns)
                    {
                        Thought_Memory thought_Memory2 = (Thought_Memory)ThoughtMaker.MakeThought(packComp.Props.togetherThought);
                        if (!(packmate == pawn) && packmate != null && pawn != null)
                        {
                            bool bubble;
                            if (!thought_Memory2.TryMergeWithExistingMemory(out bubble))
                            {
                                //Log.Message("Adding thought to pawn.");
                                pawn.needs.mood.thoughts.memories.TryGainMemory(thought_Memory2, packmate);
                            }
                        }
                    }
                }
            }
        }

        public void UpdateThreaded()
        {
            Map map = this.map;
            UpdatePawns(map);
        }

        public override void MapComponentTick()
        {
            AvaliPackDriver AvaliPackDriver = Current.Game.GetComponent<AvaliPackDriver>();
            if (mapCompOn && !(AvaliPackDriver.packs == null) && AvaliPackDriver.packs.Count > 0)
            {
                if (onTick == 120)
                {
                    Map map = this.map;
                    if (multiThreaded)
                    {
                        ThreadStart packThreadRef = new ThreadStart(UpdateThreaded);
                        Thread packThread = new Thread(packThreadRef);
                        packThread.Start();
                    }
                    else
                    {
                        
                        UpdatePawns(map);
                    }
                    onTick = 0;
                }
                else
                {
                    onTick += 1;
                }
            }
        }

    }
}