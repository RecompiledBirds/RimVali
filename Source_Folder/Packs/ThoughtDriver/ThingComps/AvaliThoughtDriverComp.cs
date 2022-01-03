using RimWorld;
using Verse;
namespace AvaliMod
{
    public class AvaliThoughtDriver : ThingComp
    {
        public AvaliDriverThoughtProps Props
        {
            get
            {
                return (AvaliDriverThoughtProps) this.props;
            }
        }
    }
}