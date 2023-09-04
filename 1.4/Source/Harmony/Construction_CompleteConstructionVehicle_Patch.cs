using HarmonyLib;
using RimWorld;

namespace VanillaVehiclesExpanded
{
    [HarmonyPatch("Vehicles.Construction", "CompleteConstructionVehicle")]
    public static class Construction_CompleteConstructionVehicle_Patch
    {
        public static void Prefix(Frame __1)
        {
            if (GameComponent_VehicleUseTracker.Instance.frameWrecks.TryGetValue(__1, out var wreck))
            {
                GameComponent_VehicleUseTracker.Instance.frameWrecks.Remove(__1);
                GameComponent_VehicleUseTracker.Instance.restoredWrecks.Add(wreck);
            }
        }
    }
}
