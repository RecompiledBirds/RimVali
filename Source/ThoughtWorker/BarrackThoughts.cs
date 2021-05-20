using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
namespace AvaliMod
{
    public class RoomThoughtsHandler : MapComponent
    {
        public RoomThoughtsHandler(Map map) : base(map)
        {
        }
        public List<Pawn> pawns = new List<Pawn>();
        void UpdateAllPawnThoughts()
        {
            foreach(Pawn pawn in pawns)
            {
                float qual = pawn.GetRoomQuality();
                bool sharedRoom = pawn.SharedBedroom();

                if (sharedRoom && !pawn.needs.mood.thoughts.memories.Memories.Any(x => x.def == AvaliDefs.AvaliSharedBedRoom))
                {
                    ThoughtDef thoughtDef = AvaliDefs.AvaliSharedBedRoom;
                    if(qual >= 0 && qual < 20)
                    {
                        Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, 0);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(mem);
                    }else if(qual >= 20 && qual < 30)
                    {
                        Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, 1);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(mem);
                    }
                    else if (qual >= 30 && qual <40)
                    {
                        Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, 2);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(mem);
                    }
                    else if (qual >= 40 && qual <50)
                    {
                        Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, 3);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(mem);
                    }
                    else if (qual >= 50 && qual < 65)
                    {
                        Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, 4);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(mem);
                    }
                    else if (qual >= 65 && qual < 85)
                    {
                        Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, 5);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(mem);
                    }
                    else if (qual >= 85 && qual < 120)
                    {
                        Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, 6);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(mem);
                    }
                    else if (qual >= 120 && qual < 170)
                    {
                        Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, 7);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(mem);
                    }
                    else if (qual >= 170 && qual < 240)
                    {
                        Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, 8);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(mem);
                    }
                    else if (qual >= 240)
                    {
                        Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, 9);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(mem);
                    }
                }
                else if( !sharedRoom&& !pawn.needs.mood.thoughts.memories.Memories.Any(x => x.def == AvaliDefs.AvaliSleptAlone))
                {
                    ThoughtDef thoughtDef = AvaliDefs.AvaliSleptAlone;
                    if (qual >= 0 && qual < 20)
                    {
                        Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, 0);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(mem);
                    }
                    else if (qual >= 20 && qual < 30)
                    {
                        Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, 1);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(mem);
                    }
                    else if (qual >= 30 && qual < 40)
                    {
                        Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, 2);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(mem);
                    }
                    else if (qual >= 40 && qual < 50)
                    {
                        Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, 3);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(mem);
                    }
                    else if (qual >= 50 && qual < 65)
                    {
                        Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, 4);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(mem);
                    }
                    else if (qual >= 65 && qual < 85)
                    {
                        Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, 5);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(mem);
                    }
                    else if (qual >= 85 && qual < 120)
                    {
                        Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, 6);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(mem);
                    }
                    else if (qual >= 85 && qual < 120)
                    {
                        Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, 7);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(mem);
                    }
                    else if (qual >= 170 && qual < 240)
                    {
                        Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, 8);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(mem);
                    }
                    else if (qual >= 240)
                    {
                        Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, 9);
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
                /*pawns = map.mapPawns.AllPawns.Where(x => (x.def == AvaliDefs.RimVali || x.def.defName == "IWAvaliRace") && !x.Awake()).ToList();
                Task task = new Task(UpdateAllPawnThoughts);
                task.Start();*/
                UpdateAllPawnThoughts();
                tick = 0;
            }
            tick++;
            base.MapComponentTick();

        }
    }
}
