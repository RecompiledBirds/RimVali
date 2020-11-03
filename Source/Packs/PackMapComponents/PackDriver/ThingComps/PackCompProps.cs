using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
namespace AvaliMod
{
    public class PackComp : ThingComp
    {
        public PackProps Props
        {
            get
            {
                return (PackProps)this.props;
            }
        }
        private List<ThingDef> RacesInPacks
        {
            get
            {
                return Props.racesInPacks;
            }
        }

        //can they have a thought for being alone? (in xml)
        private bool CanHaveAloneThought
        {
            get
            {
                return Props.canHaveAloneThought;
            }
        }
        //what is the thought referred to as? (xml)
        private ThoughtDef AloneThought
        {
            get
            {
                return Props.aloneThought;
            }
        }

        //the "pack" relation we want. (xml)
        private PawnRelationDef RelationDef
        {
            get
            {
                return Props.relation;
            }
        }

        //A curve of age gen over time. (xml)
        private SimpleCurve AgeCurve
        {
            get
            {
                return Props.packGenChanceOverAge;
            }
        }

        //Pull a list from the races rimvali found during loading
        private List<ThingDef> OtherRaces
        {
            get
            {
                return RimvaliPotentialPackRaces.potentialPackRaces.ToList<ThingDef>();
            }
        }

    }
}
