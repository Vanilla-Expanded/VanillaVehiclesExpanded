using HarmonyLib;
using RimWorld;
using System;
using Vehicles;
using Verse;

namespace VanillaVehiclesExpanded
{
    [DefOf]
    public static class VVE_DefOf
    {
        public static VehicleStatDef AccelerationRate;
        public static SoundDef VVE_TiresScreech;
        public static JobDef VVE_OpenGarage;
        public static JobDef VVE_CloseGarage;
        public static DesignationDef VVE_Open;
        public static DesignationDef VVE_Close;
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
                __result *= comp.currentSpeed / comp.GetDefaultMoveSpeed();
                comp.curPaidPathCost += __result;
            }
        }
    }

    [HarmonyPatch(typeof(Components), "DraftedVehiclesCanMove")]
    public static class Components_DraftedVehiclesCanMove_Patch
    {
        public static void Prefix(Pawn_DraftController __0, bool value)
        {
            if (__0.pawn is VehiclePawn vehicle && !value && vehicle.vPather?.curPath != null)
            {
                var comp = vehicle.GetComp<CompVehicleMovementController>();
                if (comp != null && vehicle.vPather.Moving)
                {
                    float moveSpeed = comp.GetDefaultMoveSpeed();
                    float totalCost = comp.GetPathCost(false).cost;
                    float decelerateMultiplier = 4f;
                    float decelerateInPctOfPath = moveSpeed / (comp.AccelerationRate * decelerateMultiplier) / totalCost;
                    comp.Slowdown(decelerateInPctOfPath, stopImmediately: true);
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }
}
