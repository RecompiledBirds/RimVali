using RimWorld;
using UnityEngine;
using Verse;
namespace AvaliMod
{
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
    public class AvaliGraphic : Graphic
    {
        public Color colorThree = Color.white;
        public AvaliGraphicData data;
        public string path;
        private Graphic_Shadow cachedShadowGraphicInt;
        private AvaliGraphic cachedShadowlessGraphicInt;

        public Color ColorThree
        {
            get
            {
                return this.colorThree;
            }
        }

     


        public virtual void Init(AvaliGraphicRequest req)
        {
            Log.ErrorOnce("Cannot init Graphic of class " + this.GetType().ToString(), 658928, false);
        }

        public virtual AvaliGraphic GetColoredVersion(
          Shader newShader,
          Color newColor,
          Color newColorTwo,
          Color newColorThree)
        {
            Log.ErrorOnce("CloneColored not implemented on this subclass of Graphic: " + this.GetType().ToString(), 66300, false);
            return AvaliBaseContent.BadGraphic;
        }

#pragma warning disable CS0114 // Member hides inherited member; missing override keyword
        public virtual AvaliGraphic GetCopy(Vector2 newDrawSize)

        {
            return AvaliGraphicDatabase.Get(this.GetType(),
                                            this.path,
                                            this.Shader,
                                            newDrawSize,
                                            this.color,
                                            this.colorTwo,
                                            this.colorThree);
        }

        public virtual AvaliGraphic GetShadowlessGraphic()
        {
            if (this.data == null || this.data.shadowData == null)
                return this;
            if (this.cachedShadowlessGraphicInt == null)
            {
                AvaliGraphicData graphicData = new AvaliGraphicData();
                graphicData.CopyFrom(this.data);
                graphicData.shadowData = (ShadowData)null;
                this.cachedShadowlessGraphicInt = graphicData.Graphic;
            }
            return this.cachedShadowlessGraphicInt;
        }
    }
}
