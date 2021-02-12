using Verse;
using UnityEngine;
namespace AvaliMod
{
    
    public class AvaliGraphic_LinkedCornerFiller : AvaliGraphic_Linked
    {
        private static readonly float CoverSizeCornerCorner = new Vector2(0.5f, 0.5f).magnitude;
        private static readonly float DistCenterCorner = new Vector2(0.5f, 0.5f).magnitude;
        private static readonly float CoverOffsetDist = AvaliGraphic_LinkedCornerFiller.DistCenterCorner - AvaliGraphic_LinkedCornerFiller.CoverSizeCornerCorner * 0.5f;
        private static readonly Vector2[] CornerFillUVs = new Vector2[4]
        {
      new Vector2(0.5f, 0.6f),
      new Vector2(0.5f, 0.6f),
      new Vector2(0.5f, 0.6f),
      new Vector2(0.5f, 0.6f)
        };
        private const float ShiftUp = 0.09f;
        private const float CoverSize = 0.5f;

        public override LinkDrawerType LinkerType
        {
            get
            {
                return LinkDrawerType.CornerFiller;
            }
        }

        public AvaliGraphic_LinkedCornerFiller(AvaliGraphic subGraphic)
          : base(subGraphic)
        {
        }

        public override AvaliGraphic GetColoredVersion(
          Shader newShader,
          Color newColor,
          Color newColorTwo,
          Color newColorThree)
        {
            AvaliGraphic_LinkedCornerFiller linkedCornerFiller = new AvaliGraphic_LinkedCornerFiller(this.subGraphic.GetColoredVersion(newShader, newColor, newColorTwo, newColorThree));
            linkedCornerFiller.data = this.data;
            return (AvaliGraphic)linkedCornerFiller;
        }
        

    }
    public class AvaliGraphic_LinkedTransmitterOverlay : AvaliGraphic_Linked
    {
        public AvaliGraphic_LinkedTransmitterOverlay()
        {
        }

        public AvaliGraphic_LinkedTransmitterOverlay(AvaliGraphic subGraphic)
          : base(subGraphic)
        {
        }

        public override bool ShouldLinkWith(IntVec3 c, Thing parent)
        {
            return c.InBounds(parent.Map) && parent.Map.powerNetGrid.TransmittedPowerNetAt(c) != null;
        }

    }


    public class AvaliGraphic_LinkedTransmitter : AvaliGraphic_Linked
    {
        public AvaliGraphic_LinkedTransmitter(AvaliGraphic subGraphic)
          : base(subGraphic)
        {
        }

        public override bool ShouldLinkWith(IntVec3 c, Thing parent)
        {
            return c.InBounds(parent.Map) && (base.ShouldLinkWith(c, parent) || parent.Map.powerNetGrid.TransmittedPowerNetAt(c) != null);
        }

    }
}