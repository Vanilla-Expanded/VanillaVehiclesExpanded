using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace VanillaVehiclesExpanded
{
    [HarmonyPatch]
    public static class MainTabWindow_MintResearch_SetTargetProject_Patch
    {
        public static bool Prepare() => ModsConfig.IsActive("Dubwise.DubsMintMenus");

        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            var type = AccessTools.TypeByName("DubsMintMenus.MainTabWindow_MintResearch+MysterBox");
            yield return AccessTools.Method(type, "SetTargetProject");
        }

        public static bool Prefix(ResearchProjectDef proj)
        {
            if (proj.IsDisabled(out var wreck))
            {
                Messages.Message("VVE_CannotBeResearched".Translate(wreck.LabelCap), MessageTypeDefOf.CautionInput);
                return false;
            }
            return true;
        }
    }
}
