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
    public class StatPart_VehicleRepairs_Slow : VehicleStatPart
    {

        public OperationType operation = OperationType.Multiplication;


        public override float TransformValue(VehiclePawn vehicle, float value)
        {
            float modifier = 1;
            if (Applies(vehicle))
            {
                modifier = 0.75f;
            }
            return operation.Apply(value, modifier);
        }



        public override string ExplanationPart(VehiclePawn vehicle)
        {
            if (Applies(vehicle))
            {
                return "VVE_VehicleRepairs_Slow".Translate() + ": x" + 0.75f.ToStringPercent();
            }


            return null;
        }

        public static bool Applies(VehiclePawn vehicle)
        {
            return Current.Game.World.factionManager.OfPlayer.ideos.PrimaryIdeo.GetPrecept(InternalDefOf.VVE_VehicleRepairs_Slow) != null;
        }
    }

    public class StatPart_VehicleRepairs_Fast : VehicleStatPart
    {
        public OperationType operation = OperationType.Multiplication;


        public override float TransformValue(VehiclePawn vehicle, float value)
        {
            float modifier = 1;
            if (Applies(vehicle))
            {
                modifier = 1.25f;
            }
            return operation.Apply(value, modifier);
        }

        public override string ExplanationPart(VehiclePawn vehicle)
        {
            if (Applies(vehicle))
            {
                return "VVE_VehicleRepairs_Fast".Translate() + ": x" + 1.25f.ToStringPercent();
            }


            return null;
        }

        public static bool Applies(VehiclePawn vehicle)
        {
            return Current.Game.World.factionManager.OfPlayer.ideos.PrimaryIdeo.GetPrecept(InternalDefOf.VVE_VehicleRepairs_Fast) != null;
        }
    }
}
