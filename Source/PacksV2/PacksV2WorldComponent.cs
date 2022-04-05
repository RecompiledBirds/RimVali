using AvaliMod;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Rimvali.Rewrite.Packs
{
    public class PacksV2WorldComponent : WorldComponent
    {
        private List<Pack> packs = new List<Pack>();
        private Dictionary<Pawn, int> pawnPacks = new Dictionary<Pawn, int>();
        private int nextID;

        /// <summary>
        /// Get an ID for a pack.
        /// </summary>
        public int GetID
        {
            get
            {
                return nextID++;
            }
        }

        public PacksV2WorldComponent(World world) : base(world)
        {
        }

        
        /// <summary>
        /// Clear a pawn's pack value.
        /// </summary>
        /// <param name="pawn"></param>
        public void ClearPawnPack(Pawn pawn)
        {
            if (pawnPacks.ContainsKey(pawn))
                pawnPacks.Remove(pawn);
        }

        /// <summary>
        /// Does the pawn have a pack with at least one other avali?
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public bool PawnHasPackWithMembers(Pawn pawn)
        {
            return PawnHasPack(pawn) && packs[pawnPacks[pawn]].GetAllPawns.Count>1;
        }

        /// <summary>
        /// Does the pawn have a pack?
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public bool PawnHasPack(Pawn pawn)
        {
            return pawnPacks.ContainsKey(pawn);
        }

        public Pack GetPack(Pawn pawn) => pawnPacks.ContainsKey(pawn) ? packs[pawnPacks[pawn]] : null;


        /// <summary>
        /// Add a pack to the system.
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="pawn"></param>
        public void AddPack(Pack pack, Pawn pawn = null)
        {
            packs.Add(pack);
            pawnPacks[pawn] = packs.IndexOf(pack);
        }
        public override void FinalizeInit()
        {
            reevaluateSpeed = true;
            base.FinalizeInit();
        }

        int ticks = 1;
        int calculatedTicks =1000;
        int evalTicks = 1000;

        bool reevaluateSpeed = false;

        public float CalculatedTickTime
        {
            get
            {
                return calculatedTicks;
            }
        }
        Stopwatch packTimeWatcher = new Stopwatch();

        Random random = new Random();
        public override void WorldComponentUpdate()
        {
            if (RimValiMod.settings.unstable)
            {
                if (ticks == 0 || reevaluateSpeed)
                {
                    if (reevaluateSpeed)
                        packTimeWatcher.Start();
                    Dictionary<Faction, List<Pack>> cachedFactionPacks = new Dictionary<Faction, List<Pack>>();
                    foreach (Pawn pawn in RimValiUtility.PawnsInWorld)
                    {
                        if (!cachedFactionPacks.ContainsKey(pawn.Faction))
                            cachedFactionPacks[pawn.Faction] = packs.Where(x => x.Faction == pawn.Faction).ToList();


                        if (packs.EnumerableNullOrEmpty() || cachedFactionPacks[pawn.Faction].NullOrEmpty())
                        {
                            Pack pack = new Pack(pawn.Faction, pawn, GetID);
                            AddPack(pack, pawn);
                            cachedFactionPacks[pawn.Faction].Add(pack);
                        }
                        else
                        {
                            if(!PawnHasPack(pawn) && !cachedFactionPacks[pawn.Faction].Any(x=>x.GetAllPawns.Count<RimValiMod.settings.maxPackSize) || random.Next(0, 12) == 2)
                            {
                                Pack pack = new Pack(pawn.Faction, pawn, GetID);
                                AddPack(pack, pawn);
                                cachedFactionPacks[pawn.Faction].Add(pack);
                            }
                        }
                    }
                    if (reevaluateSpeed)
                    {
                        packTimeWatcher.Stop();
                        Log.Message($"Evaluated stopwatch time: {packTimeWatcher.ElapsedTicks}ms");
                        calculatedTicks = (int)(packTimeWatcher.ElapsedTicks/5);
                        Log.Message($"Pack updates will be every: {calculatedTicks} ticks");
                        packTimeWatcher.Reset();
                        reevaluateSpeed = false;
                    }
                    ticks = calculatedTicks;
                }
                ticks--;

                if (evalTicks == 0)
                {
                    evalTicks = 1000;
                    reevaluateSpeed = true;
                }
                evalTicks--;
            }

            base.WorldComponentUpdate();
        }

        /// <summary>
        /// Include a pawn to a pack.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="pack"></param>
        public void JoinPawnToPack(Pawn pawn, ref Pack pack)
        {
            pack.AddPawn(pawn);
            pawnPacks[pawn] = packs.IndexOf(pack);
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref packs, "packs",LookMode.Deep);
            Scribe_Values.Look(ref nextID, "nextID");
            Scribe_Collections.Look(ref pawnPacks, "pawnPacks", LookMode.Reference, LookMode.Value);
        }

    }
    public class PackTimer : Alert
    {
        public override AlertReport GetReport()
        {
            // $"{"AirdropInStart".Translate()} {AirDropHandler.timeToDrop / AirDropHandler.ticksInAnHour} {"AirdropInEnd".Translate()}";

            defaultLabel =Find.World.GetComponent<PacksV2WorldComponent>().CalculatedTickTime.ToString();

            return  AlertReport.Active;
        }
    }
}
