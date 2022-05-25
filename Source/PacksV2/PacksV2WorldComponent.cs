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
        private List<Pawn> workingPawnList = new List<Pawn>();
        private List<int> workingIDList = new List<int>();
        private Dictionary<Pawn, int> pawnPacks = new Dictionary<Pawn, int>();
        private int nextID;
        private static bool enabled = false;
        private static bool ranWarn = false;


        public List<Pack> PacksReadOnly
        {
            get
            {
                return packs;
            }
        }
        /// <summary>
        /// Determine wether or not we should enable the new system.
        /// </summary>
        public static bool EnhancedMode
        {
            get
            {
                if (!ranWarn && (!(Find.GameInfo.RealPlayTimeInteracting < 10 ||enabled)))
                {
                    ranWarn = true;
                    ChoiceLetter choiceLetter = LetterMaker.MakeLetter("EnhancedModeTitle".Translate(),
                       "EnhancedModeDesc".Translate(), AvaliDefs.IlluminateAirdrop);
                    Find.LetterStack.ReceiveLetter(choiceLetter);
                }
                return (Find.GameInfo.RealPlayTimeInteracting < 10 || enabled) && RimValiMod.settings.unstable;
            }
        }

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
            RimValiUtility.LogAnaylitics("Initalizing PACKSV2.");
            enabled = false;
            ranWarn = false;
            bool dateHasPassed = DateTime.Today.Day >= 1 && DateTime.Today.Month >= 5 && DateTime.Today.Year >= 2021;
            if (Find.GameInfo.RealPlayTimeInteracting < 10 || dateHasPassed)
                enabled = true;
            RimValiUtility.LogAnaylitics("Packs V2 is running!",enabled);
        }

        
        /// <summary>
        /// Clear a pawn's pack value.
        /// </summary>
        /// <param name="pawn"></param>
        public void ClearPawnPack(Pawn pawn)
        {
            RimValiUtility.LogAnaylitics($"Clearing {pawn.Name.ToStringShort}'s pack.");
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
            bool hasPackWithMembers = PawnHasPack(pawn) && packs[pawnPacks[pawn]].GetAllPawns.Count > 1;
            RimValiUtility.LogAnaylitics($"{pawn.Name.ToStringShort} has a pack with members.", hasPackWithMembers);
            RimValiUtility.LogAnaylitics($"{pawn.Name.ToStringShort} does not have a pack members.", !hasPackWithMembers);
            return hasPackWithMembers;
        }

        /// <summary>
        /// Does the pawn have a pack?
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public bool PawnHasPack(Pawn pawn)
        {
            bool hasPack = pawnPacks.ContainsKey(pawn);
            RimValiUtility.LogAnaylitics($"{pawn.Name.ToStringShort} has a pack.", hasPack);
            RimValiUtility.LogAnaylitics($"{pawn.Name.ToStringShort} does not have a pack.", !hasPack);
            return hasPack;
        }

        public Pack GetPack(Pawn pawn) => pawnPacks.ContainsKey(pawn) ? packs[pawnPacks[pawn]] : null;


        /// <summary>
        /// Add a pack to the system.
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="pawn"></param>
        public void AddPack(Pack pack, Pawn pawn = null)
        {
            RimValiUtility.LogAnaylitics($"Adding pack {pack.Name} to packs list.");
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
            if (EnhancedMode)
            {
                if (ticks == 0 || reevaluateSpeed)
                {
                    //Start the timer if told to reevaluate function speed.
                    if (reevaluateSpeed)
                        packTimeWatcher.Start();
                    Dictionary<Faction, List<Pack>> cachedFactionPacks = new Dictionary<Faction, List<Pack>>();
                    IEnumerable<Pawn> pawns = RimValiUtility.PawnsInWorld;
                    if (!pawns.EnumerableNullOrEmpty())
                    {
                        foreach (Pawn pawn in pawns)
                        {
                            if (pawn != null && pawn.Faction != null)
                            {
                                if (!cachedFactionPacks.ContainsKey(pawn.Faction))
                                    cachedFactionPacks[pawn.Faction] = packs.Where(x => x.Faction == pawn.Faction).ToList();

                                bool anyFactionPacksAcceptable = (!cachedFactionPacks[pawn.Faction].NullOrEmpty() && cachedFactionPacks[pawn.Faction].Any(x => x.GetAllPawns.Count < RimValiMod.settings.maxPackSize));

                                RimValiUtility.LogAnaylitics($"{pawn.Name.ToStringShort} has a acceptable pack in their faction: {anyFactionPacksAcceptable}");

                                bool hasPack = PawnHasPack(pawn);
                                if (!hasPack && (packs.EnumerableNullOrEmpty() || !anyFactionPacksAcceptable) || random.Next(0, 12) == 2)
                                {
                                    RimValiUtility.LogAnaylitics($"Generating pack for {pawn}");
                                    Pack pack = new Pack(pawn.Faction, pawn, GetID);
                                    AddPack(pack, pawn);
                                    cachedFactionPacks[pawn.Faction].Add(pack);
                                }
                            }
                        }
                    }
                    //If told to reevaluate speed, stop the stopwatch, calculate the ticks between each run, and reset the stopwatch.
                    if (reevaluateSpeed)
                    {
                        RimValiUtility.LogAnaylitics("Re-evaluating pack system speed..");
                        packTimeWatcher.Stop();
                        calculatedTicks = (int)(packTimeWatcher.ElapsedTicks/5);
                        RimValiUtility.LogAnaylitics($"Time elapsed: {packTimeWatcher.ElapsedMilliseconds}ms \nCalculated ticks: {calculatedTicks}");
                        packTimeWatcher.Reset();
                        reevaluateSpeed = false;
                        
                    }
                    ticks = calculatedTicks;
                }
                ticks--;

                //Ask for speed evaluation every 1000 ticks
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
            RimValiUtility.LogAnaylitics($"Joining pawn {pawn.Name.ToStringShort} to pack {pack.Name}");
            pack.AddPawn(pawn);
            pawnPacks[pawn] = packs.IndexOf(pack);
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref packs, "packs",LookMode.Deep);
            Scribe_Values.Look(ref nextID, "nextID");
            Scribe_Collections.Look(ref pawnPacks, "pawnPacks", LookMode.Reference, LookMode.Value, ref workingPawnList, ref workingIDList);
            Scribe_Values.Look(ref enabled, "enabled");
            bool dateHasPassed = DateTime.Today.Day >= 1 && DateTime.Today.Month >= 5 && DateTime.Today.Year >= 2021;
            if(dateHasPassed)
                enabled= true;
        }

    }
}
