using System.Collections.Generic;
using Verse;
namespace AvaliMod
{
    public class TraitsOverTimeProps : HediffCompProperties
    {
        public List<TraitsOverTimeClass> traits;

        public TraitsOverTimeProps()
        {
            this.compClass = typeof(TraitsOverTime);
        }
    }
}