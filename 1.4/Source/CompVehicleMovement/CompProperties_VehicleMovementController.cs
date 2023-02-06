using System;
using System.Collections.Generic;
using System.Linq;
using Vehicles;

namespace VanillaVehiclesExpanded
{
    public class CompProperties_VehicleMovementController : VehicleCompProperties
    {
        public CompProperties_VehicleMovementController()
        {
            compClass = typeof(CompVehicleMovementController);
        }
    }
}
