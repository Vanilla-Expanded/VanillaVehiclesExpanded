using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaVehiclesExpanded
{
    [HarmonyPatch(typeof(MainTabWindow_Research), "DoBeginResearch")]
    public static class MainTabWindow_Research_DoBeginResearch_Patch
    {
        public static bool Prefix(ResearchProjectDef projectToStart)
        {
            if (projectToStart.IsDisabled(out var wreck))
            {
                Messages.Message("VVE_CannotBeResearched".Translate(wreck.LabelCap), MessageTypeDefOf.CautionInput);
                return false;
            }
            return true;
        }
    }
}
