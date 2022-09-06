using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using Vehicles;
using Verse;

namespace VanillaVehiclesExpanded
{
    [DefOf]
    public static class VVE_DefOf
    {
        public static VehicleStatDef AccelerationRate;
    }

    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            new Harmony("VanillaVehiclesExpanded.Mod").PatchAll();
        }
    }

    [HarmonyPatch(typeof(Vehicle_PathFollower), "StartPath")]
    public static class Vehicle_PathFollower_StartPath_Patch
    {
        public static void Postfix(VehiclePawn ___vehicle)
        {
            var comp = ___vehicle.GetComp<CompVehicleMovementController>();
            if (comp != null)
            {
                comp.StartMove();
            }
        }
    }

    [HarmonyPatch(typeof(Vehicle_PathFollower), "CostToPayThisTick")]
    public static class Vehicle_PathFollower_CostToPayThisTick_Patch
    {
        public static void Postfix(ref float __result, VehiclePawn ___vehicle)
        {
            var comp = ___vehicle.GetComp<CompVehicleMovementController>();
            if (comp != null)
            {
                __result *= comp.currentSpeed / comp.GetMoveSpeed();
            }
        }
    }


    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }
}
