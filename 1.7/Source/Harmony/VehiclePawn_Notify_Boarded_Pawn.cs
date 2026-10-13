using HarmonyLib;
using Vehicles;
using Verse;

namespace VanillaVehiclesExpanded
{
  [HarmonyPatch(typeof(VehiclePawn), "TryAddPawn", typeof(Pawn), typeof(VehicleRoleHandler))]
  public static class VehiclePawn_Notify_Boarded_Pawn
  {
    public static void Postfix(VehiclePawn __instance, bool __result, Pawn pawn)
    {
      if (__result)
      {
        var tracker = pawn.GetVehicleTracker(__instance.VehicleDef);
        tracker.boardedToTicks = Find.TickManager.TicksGame;
      }
    }
  }
}