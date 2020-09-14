using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
namespace AvaliMod
{
    public class AvaliPackDriver : MapComponent
    {
        private readonly bool enableDebug = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().enableDebugMode;
        private readonly int maxSize = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().maxPackSize;
        private readonly bool packsEnabled = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packsEnabled;
        private readonly bool checkOtherRaces = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().checkOtherRaces;
        private readonly bool allowAllRaces = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().allowAllRaces;
        private bool AddedRaces = false;
        private int onTick = 0;
        private int onOtherTick = 0;
        public Dictionary<Pawn, bool> pawnHadPack;
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

        public override void ExposeData()
        {
            Scribe_Collections.Look<Pawn, bool>(ref pawnHadPack, "pawnsThatHaveHadPacks");
        }
        public void UpdatePacks()
        {
            IEnumerable<Pawn> pawnsOnMap = RimValiUtility.AllPawnsOfRaceOnMap(AvaliDefs.RimVali, map).Where(x => (RimValiUtility.GetPackSize(x, x.TryGetComp<PackComp>().Props.relation) < maxSize) & racesInPacks.Contains(x.def));
            foreach (Pawn pawn in pawnsOnMap)
            {

                PackComp comp = pawn.TryGetComp<PackComp>();
                if (!(comp == null))
                {
                    //Pull the comp info from the pawn
                    PawnRelationDef relationDef = comp.Props.relation;
                    SimpleCurve ageCurve = comp.Props.packGenChanceOverAge;
                    if (RimValiUtility.GetPackSize(pawn, relationDef) == 1)
                    {
                        //Tells us that this pawn has had a pack
                        if (enableDebug)
                        {
                            Log.Message("Attempting to make pack.. [Base pack]");
                        }
                        //Makes the pack.
                        RimValiUtility.KeoBuildMakePack(pawn, relationDef, racesInPacks, maxSize);
                    }
                }
            }
        }

        public override void MapComponentTick()
        {
            if (!AddedRaces)
            {
                AddRaces();
            }
            if (onTick == 120)
            {
                if (packsEnabled)
                { 
                    UpdatePacks();
                }
                onTick = 0;
            }
            else
            {
                onTick += 1;
            }
            if(onOtherTick == 1800)
            {
                onOtherTick = 0;
            }
            else
            {
                onOtherTick += 1;
            }
        }

    }
}