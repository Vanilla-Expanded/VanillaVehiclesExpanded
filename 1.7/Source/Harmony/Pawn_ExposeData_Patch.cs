using HarmonyLib;
using System.Collections.Generic;
using Vehicles;
using Verse;

namespace VanillaVehiclesExpanded
{
    [HarmonyPatch(typeof(Pawn), "ExposeData")]
    public static class Pawn_ExposeData_Patch
    {
        public static void Postfix(Pawn __instance)
        {
            var vehicleTrackers = __instance.GetVehicleTrackers();
            Scribe_Collections.Look(ref vehicleTrackers, "vehicleTrackers", LookMode.Def, LookMode.Deep);
            if (vehicleTrackers != null)
            {
                pawnVehicleTrackers[__instance] = vehicleTrackers;
            }
        }

        private static Dictionary<Pawn, Dictionary<VehicleDef, VehicleTracker>> pawnVehicleTrackers = new();
        public static Dictionary<VehicleDef, VehicleTracker> GetVehicleTrackers(this Pawn pawn)
        {
            if (!pawnVehicleTrackers.TryGetValue(pawn, out var data))
            {
                pawnVehicleTrackers[pawn] = data = new();
            }
            return data;
        }

        public static VehicleTracker GetVehicleTracker(this Pawn pawn, VehicleDef vehicleDef)
        {
            var trackers = pawn.GetVehicleTrackers();
            if (!trackers.TryGetValue(vehicleDef, out var data))
            {
                trackers[vehicleDef] = data = new();
            }
            return data;
        }
    }
}
