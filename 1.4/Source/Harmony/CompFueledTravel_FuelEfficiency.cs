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



    [HarmonyPatch(typeof(CompFueledTravel))]
    [HarmonyPatch("FuelEfficiency", MethodType.Getter)]
    public static class VanillaVehiclesExpanded_CompFueledTravel_FuelEfficiency_Patch
    {

        public static void Postfix(ref float __result)
        {

            if (Current.Game.World.factionManager.OfPlayer.ideos.PrimaryIdeo.GetPrecept(InternalDefOf.VVE_FuelEfficiency_Inefficient) != null)
            {
                __result = __result * 0.5f;
            }

            if (Current.Game.World.factionManager.OfPlayer.ideos.PrimaryIdeo.GetPrecept(InternalDefOf.VVE_FuelEfficiency_Efficient) != null)
            {
                __result = __result * 1.5f;
            }

        }

    }













}
