using System.Collections.Generic;
using Verse;
namespace AvaliMod
{
    public class ShowTextPropsV3 : HediffCompProperties
    {
        public List<showTextClass> showText;

        public ShowTextPropsV3()
        {
            this.compClass = typeof(ShowTextComp);
        }
    }
}