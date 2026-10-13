using HarmonyLib;
using RimWorld;
using Vehicles;
using Verse;

namespace VanillaVehiclesExpanded
{
    [HarmonyPatch(typeof(CompRottable), "Active", MethodType.Getter)]
    public static class CompRottable_Active_Patch
    {
        public static bool Prefix(CompRottable __instance, ref bool __result)
        {
            if (__instance.parent.ParentHolder is Pawn_InventoryTracker tracker && tracker.pawn is VehiclePawn vehicle 
                && vehicle.GetCachedComp<CompRefrigerator>() is CompRefrigerator comp
                    && comp.enabled && vehicle.CompFueledTravel.Fuel > 0)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
