using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VanillaVehiclesExpanded
{
    [DefOf]
    public static class InternalDefOf
    {
        static InternalDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(InternalDefOf));
        }

       

        [MayRequireIdeology]
        public static PreceptDef VVE_SmallSpaces_Horrible;

        [MayRequireIdeology]
        public static PreceptDef VVE_Driving_Required;
        [MayRequireIdeology]
        public static PreceptDef VVE_Flying_Required;
        [MayRequireIdeology]
        public static PreceptDef VVE_Sailing_Required;

        [MayRequireIdeology]
        public static PreceptDef VVE_Acceleration_SundayDriver;
        [MayRequireIdeology]
        public static PreceptDef VVE_Acceleration_High;

        [MayRequireIdeology]
        public static PreceptDef VVE_VehicleRepairs_Slow;
        [MayRequireIdeology]
        public static PreceptDef VVE_VehicleRepairs_Fast;

        [MayRequireIdeology]
        public static PreceptDef VVE_FuelEfficiency_Inefficient;
        [MayRequireIdeology]
        public static PreceptDef VVE_FuelEfficiency_Efficient;

        [MayRequireIdeology]
        public static PreceptDef VVE_ImpactDamage_High;

        [MayRequireIdeology]
        public static PreceptDef VVE_MaxSpeed_Increased;
    }
}