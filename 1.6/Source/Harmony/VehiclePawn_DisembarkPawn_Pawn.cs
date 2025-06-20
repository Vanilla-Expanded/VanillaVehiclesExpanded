using HarmonyLib;
using RimWorld;
using Vehicles;
using Verse;

namespace VanillaVehiclesExpanded
{
    [HarmonyPatch(typeof(VehiclePawn), "DisembarkPawn")]
    public static class VehiclePawn_DisembarkPawn_Pawn
    {
        public static void Postfix(VehiclePawn __instance, Pawn pawn)
        {
            var tracker = pawn.GetVehicleTracker(__instance.VehicleDef);
            tracker.disembarkedFromTicks = Find.TickManager.TicksGame;
        }
    }
}
