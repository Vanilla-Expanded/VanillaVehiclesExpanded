using HarmonyLib;
using RimWorld;

namespace VanillaVehiclesExpanded
{
    [HarmonyPatch(typeof(ReadingOutcomeDoerGainResearch), "OnReadingTick")]
    public static class ReadingOutcomeDoerGainResearch_OnReadingTick_Patch
    {
        public static void Prefix() => ResearchProjectDef_CanStartNow_Patch.shouldSkip = false;

        public static void Postfix() => ResearchProjectDef_CanStartNow_Patch.shouldSkip = false;
    }
}
