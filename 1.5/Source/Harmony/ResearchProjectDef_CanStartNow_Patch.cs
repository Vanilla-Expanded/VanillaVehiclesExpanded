using HarmonyLib;
using Verse;

namespace VanillaVehiclesExpanded
{
    [HarmonyPatch(typeof(ResearchProjectDef), "CanStartNow", MethodType.Getter)]
    public static class ResearchProjectDef_CanStartNow_Patch
    {
        public static void Postfix(ResearchProjectDef __instance, ref bool __result)
        {
            if (__result && __instance.IsDisabled())
            {
                __result = false;
            }
        }
    }
}
