using System.Collections.Generic;
using System.Threading.Tasks;
using RimValiCore;
using RimWorld;
using Verse;

namespace AvaliMod
{
    public class BarrackThoughtHandler : MapComponent
    {
        public IEnumerable<Pawn> pawns;

        private int tick;

        public BarrackThoughtHandler(Map map) : base(map)
        {
        }

        private void UpdateAllPawnThoughts()
        {
            foreach (Pawn pawn in pawns)
            {
                bool sharedRoom = pawn.SharedBedroom();
                if (pawn.Awake())
                {
                    continue;
                }

                if (sharedRoom &&
                    !pawn.needs.mood.thoughts.memories.Memories.Any(x => x.def == AvaliDefs.AvaliSharedBedRoom))
                {
                    ThoughtDef thoughtDef = AvaliDefs.AvaliSharedBedRoom;
                    Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef,
                        RoomStatDefOf.Impressiveness.GetScoreStageIndex(pawn.CurrentBed().GetRoom()
                            .GetStat(RoomStatDefOf.Impressiveness)));
                    pawn.needs.mood.thoughts.memories.TryGainMemory(mem);
                }
                else if (!sharedRoom &&
                         !pawn.needs.mood.thoughts.memories.Memories.Any(x => x.def == AvaliDefs.AvaliSleptAlone))
                {
                    ThoughtDef thoughtDef = AvaliDefs.AvaliSleptAlone;
                    Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef,
                        RoomStatDefOf.Impressiveness.GetScoreStageIndex(pawn.CurrentBed().GetRoom()
                            .GetStat(RoomStatDefOf.Impressiveness)));
                    pawn.needs.mood.thoughts.memories.TryGainMemory(mem);
                }
            }
        }

        public override void MapComponentTick()
        {
            if (tick == 240)
            {
                pawns = RimValiCore.RimValiUtility.AllPawnsOfRaceOnMap(AvaliDefs.AvaliRaces, map);
                var task = new Task(UpdateAllPawnThoughts);
                task.Start();
                //UpdateAllPawnThoughts();
                tick = 0;
            }

            tick++;
            base.MapComponentTick();
        }
    }
}
