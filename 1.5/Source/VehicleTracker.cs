using System.Linq;
using Vehicles;
using Verse;

namespace VanillaVehiclesExpanded
{
    public class VehicleTracker : IExposable
    {
        public int boardedToTicks;

        public int disembarkedFromTicks;

        public void ExposeData()
        {
            Scribe_Values.Look(ref boardedToTicks, "boardedToTicks");
            Scribe_Values.Look(ref disembarkedFromTicks, "disembarkedFromTicks");
        }
    }
}
