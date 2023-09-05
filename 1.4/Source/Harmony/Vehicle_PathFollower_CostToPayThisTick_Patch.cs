using HarmonyLib;
using Vehicles;
using Verse;

namespace VanillaVehiclesExpanded
{
    [HarmonyPatch(typeof(Vehicle_PathFollower), "CostToPayThisTick")]
    public static class Vehicle_PathFollower_CostToPayThisTick_Patch
    {
        public static void Postfix(ref float __result, VehiclePawn ___vehicle)
        {
            var comp = ___vehicle.GetCachedComp<CompVehicleMovementController>();
            if (comp != null)
            {
                if (comp.currentSpeed > 0)
                {
                    __result *= comp.currentSpeed / comp.StatMoveSpeed;
                    if (comp.curPaidPathCostTickChecked != Find.TickManager.TicksGame)
                    {
                        comp.curPaidPathCost += __result;
                    }
                }
            }
        }
    }
}
