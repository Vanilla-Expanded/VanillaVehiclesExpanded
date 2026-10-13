using HarmonyLib;
using Verse;

namespace VanillaVehiclesExpanded
{
    [HarmonyPatch(typeof(ResearchProjectDef), "CanStartNow", MethodType.Getter)]
    public static class ResearchProjectDef_CanStartNow_Patch
    {
        public static bool shouldSkip;
        public static void Postfix(ResearchProjectDef __instance, ref bool __result)
        {
            if (shouldSkip is false && __result && __instance.IsDisabled())
            {
                __result = false;
            }
        }
    }
}
