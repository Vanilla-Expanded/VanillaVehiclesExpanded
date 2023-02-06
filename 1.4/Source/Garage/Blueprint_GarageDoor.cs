using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaVehiclesExpanded
{
    public class Blueprint_GarageDoor : Blueprint_Build
    {
        public override Graphic Graphic
        {
            get
            {
                var graphic = base.Graphic;
                var thingDef = this.def.entityDefToBuild as ThingDef;
                graphic.data.drawSize = graphic.drawSize = thingDef.graphic.drawSize = thingDef.graphicData.drawSize = new Vector2(thingDef.size.x, thingDef.size.z);
                return graphic;
            }
        }
        public override void Draw()
        {
            var oldDrawSize = def.graphicData.drawSize;
            var thingDef = this.def.entityDefToBuild as ThingDef;
            def.graphicData.drawSize = thingDef.graphicData.drawSize = new Vector2(thingDef.size.x, thingDef.size.z);
            base.Draw();
            def.graphicData.drawSize = thingDef.graphicData.drawSize = oldDrawSize;
        }
    }
}
