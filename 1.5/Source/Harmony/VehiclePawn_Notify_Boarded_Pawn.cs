using HarmonyLib;
using Vehicles;
using Verse;

namespace VanillaVehiclesExpanded
{
    [HarmonyPatch(typeof(VehiclePawn), "Notify_Boarded")]
    public static class VehiclePawn_Notify_Boarded_Pawn
    {
        public static void Postfix(VehiclePawn __instance, bool __result, Pawn pawnToBoard)
        {
            if (__result)
            {
                var tracker = pawnToBoard.GetVehicleTracker(__instance.VehicleDef);
                tracker.boardedToTicks = Find.TickManager.TicksGame;
            }
        }
    }
}
