using System;
using RimWorld;
using System.Reflection;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using Vehicles;
using SmashTools;


namespace VanillaVehiclesExpanded
{
    public class StatPart_AccelerationPrecept_SundayDriver : VehicleStatPart
    {

        public OperationType operation = OperationType.Multiplication;


        public override float TransformValue(VehiclePawn vehicle, float value)
        {
            float modifier = 1;
            if (Applies(vehicle))
            {
                modifier = 0.5f;
            }
            return operation.Apply(value, modifier);
        }



        public override string ExplanationPart(VehiclePawn vehicle)
        {
            if (Applies(vehicle))
            {
                return "VVE_AccelerationSundayDriver".Translate() + ": x" + 0.5f.ToStringPercent();
            }


            return null;
        }

        public static bool Applies(VehiclePawn vehicle)
        {
            return Current.Game.World.factionManager.OfPlayer.ideos.PrimaryIdeo.GetPrecept(InternalDefOf.VVE_Acceleration_SundayDriver) != null;
        }
    }

    public class StatPart_AccelerationPrecept_High : VehicleStatPart
    {
        public OperationType operation = OperationType.Multiplication;


        public override float TransformValue(VehiclePawn vehicle, float value)
        {
            float modifier = 1;
            if (Applies(vehicle))
            {
                modifier = 2f;
            }
            return operation.Apply(value, modifier);
        }

        public override string ExplanationPart(VehiclePawn vehicle)
        {
            if (Applies(vehicle))
            {
                return "VVE_AccelerationHigh".Translate() + ": x" + 0.5f.ToStringPercent();
            }


            return null;
        }

        public static bool Applies(VehiclePawn vehicle)
        {
            return Current.Game.World.factionManager.OfPlayer.ideos.PrimaryIdeo.GetPrecept(InternalDefOf.VVE_Acceleration_High) != null;
        }
    }
}
