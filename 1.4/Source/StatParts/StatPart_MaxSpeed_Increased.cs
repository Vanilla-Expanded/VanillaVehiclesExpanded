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
    public class StatPart_MaxSpeed_Increased : VehicleStatPart
    {

        public OperationType operation = OperationType.Multiplication;


        public override float TransformValue(VehiclePawn vehicle, float value)
        {
            float modifier = 1;
            if (Applies(vehicle))
            {
                modifier = 1.2f;
            }
            return operation.Apply(value, modifier);
        }



        public override string ExplanationPart(VehiclePawn vehicle)
        {
            if (Applies(vehicle))
            {
                return "VVE_MaxSpeed_Increased".Translate() + ": x" + 1.2f.ToStringPercent();
            }


            return null;
        }

        public static bool Applies(VehiclePawn vehicle)
        {
            return Current.Game.World.factionManager.OfPlayer.ideos.PrimaryIdeo.GetPrecept(InternalDefOf.VVE_MaxSpeed_Increased) != null;
        }
    }

  
}
