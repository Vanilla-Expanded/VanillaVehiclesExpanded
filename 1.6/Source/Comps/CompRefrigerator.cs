using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Vehicles;
using Verse;

namespace VanillaVehiclesExpanded
{
    public class CompProperties_Refrigerator : VehicleCompProperties
    {
        public string label;
        public string description;
        public float fuelConsumptionPerDay;
        public string iconPath;

        public CompProperties_Refrigerator()
        {
            this.compClass = typeof(CompRefrigerator);
        }
    }

    public class CompRefrigerator : VehicleComp
    {
        public bool enabled;
        public CompProperties_Refrigerator Props => base.props as CompProperties_Refrigerator;

        public override void CompTick()
        {
            base.CompTick();
            if (enabled)
            {
                this.Vehicle.CompFueledTravel.ConsumeFuel(Props.fuelConsumptionPerDay / (float)GenDate.TicksPerDay);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Toggle
            {
                defaultLabel = Props.label,
                defaultDesc = Props.description,
                icon = ContentFinder<Texture2D>.Get(Props.iconPath),
                isActive = () => enabled,
                toggleAction = () => { enabled = !enabled; },
            };
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref enabled, "enabled", false);
        }
    }
}
