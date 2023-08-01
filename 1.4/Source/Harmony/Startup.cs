using HarmonyLib;
using RimWorld;
using System;
using Vehicles;
using Verse;
using SmashTools;

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

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }
}
