using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Vehicles;

namespace VanillaVehiclesExpanded
{



    [HarmonyPatch(typeof(VehiclePawn))]
    [HarmonyPatch("WorldSpeedMultiplier", MethodType.Getter)]
    public static class VanillaVehiclesExpanded_VehiclePawn_WorldSpeedMultiplier_Patch
    {

        public static void Postfix(ref float __result)
        {

            if (Current.Game.World.factionManager.OfPlayer.ideos.PrimaryIdeo.GetPrecept(InternalDefOf.VVE_Acceleration_SundayDriver) != null)
            {
                __result = __result *0.8f;
            }

            if (Current.Game.World.factionManager.OfPlayer.ideos.PrimaryIdeo.GetPrecept(InternalDefOf.VVE_Acceleration_High) != null)
            {
                __result = __result * 1.25f;
            }

        }

    }





   







}
