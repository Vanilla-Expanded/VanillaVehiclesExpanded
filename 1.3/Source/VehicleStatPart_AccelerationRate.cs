using Vehicles;
using Verse;

namespace VanillaVehiclesExpanded
{
    [HotSwappable]
    public class VehicleStatPart_AccelerationRate : VehicleStatPart
    {
        public static bool modifyValue;
        public override bool Disabled(VehiclePawn vehicle)
        {
            return base.Disabled(vehicle) && vehicle.GetComp<CompVehicleMovementController>() is null;
        }
        public override string ExplanationPart(VehiclePawn vehicle)
        {
            var comp = vehicle.GetComp<CompVehicleMovementController>();
            return "VVE_StatsReport_AccelerationRate".Translate(comp.AccelerationRate, comp.currentSpeed);
        }

        public override float TransformValue(VehiclePawn vehicle, float value)
        {
            if (modifyValue)
            {
                var comp = vehicle.GetComp<CompVehicleMovementController>();
                //Log.Message("Returning " + comp.currentSpeed + " instead of " + value);
                return comp.currentSpeed;
            }
            return value;
        }
    }
}
