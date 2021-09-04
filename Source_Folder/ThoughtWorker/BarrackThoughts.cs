using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using RimValiCore;
namespace AvaliMod
{
    public class RoomThoughtsHandler : MapComponent
    {
        public RoomThoughtsHandler(Map map) : base(map)
        {
        }
        public IEnumerable<Pawn> pawns;
        void UpdateAllPawnThoughts()
        {
            foreach (Pawn pawn in pawns)
            {

                bool sharedRoom = pawn.SharedBedroom();
                if (!pawn.Awake())
                {
                    if (sharedRoom && !pawn.needs.mood.thoughts.memories.Memories.Any(x => x.def == AvaliDefs.AvaliSharedBedRoom))
                    {
                        ThoughtDef thoughtDef = AvaliDefs.AvaliSharedBedRoom;
                        Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, RoomStatDefOf.Impressiveness.GetScoreStageIndex(pawn.CurrentBed().GetRoom().GetStat(RoomStatDefOf.Impressiveness)));
                        pawn.needs.mood.thoughts.memories.TryGainMemory(mem);
                    }
                    else if (!sharedRoom && !pawn.needs.mood.thoughts.memories.Memories.Any(x => x.def == AvaliDefs.AvaliSleptAlone))
                    {
                        ThoughtDef thoughtDef = AvaliDefs.AvaliSleptAlone;
                        Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, RoomStatDefOf.Impressiveness.GetScoreStageIndex(pawn.CurrentBed().GetRoom().GetStat(RoomStatDefOf.Impressiveness)));
                        pawn.needs.mood.thoughts.memories.TryGainMemory(mem);

                    }
                }
            }
        }

        int tick = 0;
        public override void MapComponentTick()
        {
            if (tick == 240)
            {
                pawns = RimValiCore.RimValiUtility.AllPawnsOfRaceOnMap(new List<ThingDef> { AvaliDefs.RimVali, AvaliDefs.IWAvaliRace }, map);
                Task task = new Task(UpdateAllPawnThoughts);
                task.Start();
               //UpdateAllPawnThoughts();
                tick = 0;
            }
            tick++;
            base.MapComponentTick();

        }
    }
}
