using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
namespace AvaliMod
{
    public class AvaliPackDriver : MapComponent
    {
        private bool enableDebug = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().enableDebugMode;
        private int maxSize = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().maxPackSize;
        private readonly bool packsEnabled = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packsEnabled;
        private readonly bool packLossEnabled = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packLossEnabled;
        private readonly bool checkOtherRaces = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().checkOtherRaces;
        private readonly bool allowAllRaces = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().allowAllRaces;
        private bool AddedRaces = false;
        private int onTick = 0;
        public AvaliPackDriver(Map map)
            : base(map)
        {

        }
        List<ThingDef> racesInPacks = new List<ThingDef>();
        public void AddRaces()
        {
            if (!AddedRaces)
            {
                racesInPacks.Add(AvaliDefs.RimVali);
                if (checkOtherRaces)
                {
                    foreach(ThingDef race in RimvaliPotentialPackRaces.potentialPackRaces)
                    {
                        racesInPacks.Add(race);
                    }
                }
                if (allowAllRaces)
                {
                    foreach(ThingDef race in RimvaliPotentialPackRaces.potentialRaces)
                    {
                        racesInPacks.Add(race);
                    }
                }
                AddedRaces = true;
            }
        }

        public void UpdatePawns()
        {
            IEnumerable<Pawn> pawnsOnMap = map.mapPawns.AllPawns;
            foreach (Pawn pawn in pawnsOnMap)
            {
                if (pawn.IsHashIntervalTick(120))
                {
                    PackComp comp = pawn.TryGetComp<PackComp>();
                    if (racesInPacks.Contains(pawn.def))
                    {
                        UpdateThought(pawn, comp.Props.relation, comp.Props.aloneThought);
                    }

                }
            }
        }


        public void UpdatePacks()
        {
            IEnumerable<Pawn> pawnsOnMap = map.mapPawns.AllPawns;
            foreach (Pawn pawn in pawnsOnMap)
            {
                PackComp comp = pawn.TryGetComp<PackComp>();
                if (!(comp == null))
                {
                    if (RimValiUtility.GetPackSize(pawn, comp.Props.relation) == 1)
                    {
                        if (enableDebug)
                        {
                            Log.Message("Attempting to make pack..");
                        }
                        RimValiUtility.MakePack(pawn, comp.Props.relation, comp.Props.racesInPacks, maxSize);
                        UpdateThought(pawn, comp.Props.relation, comp.Props.aloneThought);
                    }
                    else
                    {
                        RimValiUtility.RemovePackRelationIfDead(pawn, RimValiUtility.GetPackPawns(pawn, comp.Props.relation).ToList(), comp.Props.relation);
                        UpdateThought(pawn, comp.Props.relation, comp.Props.aloneThought);

                        if (enableDebug)
                        {
                            Log.Message("Pawn has pack. No need to generate or add to one.");
                        }
                    }
                }
            }
        }

        public void RemoveThought(ThoughtDef thought, PawnRelationDef relationDef, Pawn pawn)
        {
            if(RimValiUtility.GetPackSize(pawn, relationDef) > 1 && packLossEnabled)
            {
                RimValiUtility.RemoveThought(pawn, thought);
            }
        }
        
        public void AddThought(ThoughtDef thought, PawnRelationDef relationDef, Pawn pawn)
        {
            if (RimValiUtility.GetPackSize(pawn, relationDef) == 1 && packLossEnabled)
            {
                RimValiUtility.AddThought(pawn, thought);
            }
        }

        public void UpdateThought(Pawn pawn, PawnRelationDef relationDef, ThoughtDef thought)
        {
            AddThought(thought, relationDef, pawn);
            RemoveThought(thought, relationDef, pawn);
        }

        public override void MapComponentTick()
        {
            if (onTick == 120)
            {
                if (packsEnabled)
                {
                    AddRaces();
                    UpdatePacks();
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