using RimWorld;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;

namespace VanillaVehiclesExpanded
{
    public class CompProperties_RefuelingPump : VehicleCompProperties
    {
        public float refuelAmountPerTick;

        public CompProperties_RefuelingPump()
        {
            this.compClass = typeof(CompRefuelingPump);
        }
    }

    public class CompRefuelingPump : VehicleComp
    {
        public CompProperties_RefuelingPump Props => base.props as CompProperties_RefuelingPump;

        public CompRefuelable compRefuelable;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compRefuelable = this.parent.GetComp<CompRefuelable>();
        }
        public override void CompTick()
        {
            base.CompTick();
            if (parent.Spawned)
            {
                var vehicle = this.parent.InteractionCell.GetThingList(this.parent.Map).OfType<VehiclePawn>().FirstOrDefault();
                if (vehicle != null && compRefuelable.HasFuel)
                {
                    var compFuel = vehicle.CompFueledTravel;
                    if (compFuel != null && compFuel.fuel < compFuel.FuelCapacity) 
                    {
                        var fuelAmount = Mathf.Min(compFuel.FuelCapacity - compFuel.fuel, 
                            Mathf.Min(compFuel.fuel, Props.refuelAmountPerTick));
                        compFuel.Refuel(fuelAmount);
                        compRefuelable.ConsumeFuel(fuelAmount);
                    }
                }
            }
        }
    }
}
