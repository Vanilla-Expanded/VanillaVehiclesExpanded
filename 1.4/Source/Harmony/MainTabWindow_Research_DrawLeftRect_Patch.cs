using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace VanillaVehiclesExpanded
{
    [HarmonyPatch(typeof(MainTabWindow_Research), "DrawLeftRect")]
    public static class MainTabWindow_Research_DrawLeftRect_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            var selectedProjectInfo = AccessTools.Field(typeof(MainTabWindow_Research), "selectedProject");
            bool patched = false;
            int match = 0;
            for (var i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                yield return code;
                if (!patched && code.opcode == OpCodes.Stloc_S && code.operand is LocalBuilder lb 
                    && lb.LocalIndex == 19 && codes[i - 1].opcode == OpCodes.Ldstr && codes[i - 1].OperandIs(""))
                {
                    match++;
                    if (match == 2)
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 19);
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, selectedProjectInfo);
                        yield return new CodeInstruction(OpCodes.Call,
                            AccessTools.Method(typeof(MainTabWindow_Research_DrawLeftRect_Patch), nameof(ChangeLockedReason)));
                        yield return new CodeInstruction(OpCodes.Stloc_S, 19);
                        patched = true;
                    }
                }
            }
        }

        public static string ChangeLockedReason(string text, ResearchProjectDef researchProject)
        {
            if (researchProject.IsDisabled(out var wreckNotRestored))
            {
                return text += "\n  " + "VVE_WreckNotRestored".Translate(wreckNotRestored.LabelCap);
            }
            return text;
        }
    }
}
