using HarmonyLib;
using UnityEngine;
using Verse;

namespace VanillaVehiclesExpanded
{
    [HarmonyPatch(typeof(GhostDrawer), "DrawGhostThing")]
    public static class GhostDrawer_DrawGhostThing_Patch
    {
        public static void Prefix(ThingDef thingDef, ref Vector3 __state)
        {
            if (typeof(GarageDoor).IsAssignableFrom(thingDef.thingClass))
            {
                __state = thingDef.graphicData.drawSize;
                thingDef.graphicData.drawSize = thingDef.graphic.drawSize = new Vector2(thingDef.size.x, thingDef.size.z);
            }
        }

        public static void Postfix(ThingDef thingDef, Vector3 __state)
        {
            if (typeof(GarageDoor).IsAssignableFrom(thingDef.thingClass))
            {
                thingDef.graphicData.drawSize = __state;
            }
        }
    }
}
