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
    [HarmonyPatch("PawnCollisionMultiplier", MethodType.Getter)]
    public static class VanillaVehiclesExpanded_VehiclePawn_PawnCollisionMultiplier_Patch
    {

        public static void Postfix(ref float __result)
        {

            if (Current.Game.World.factionManager.OfPlayer.ideos.PrimaryIdeo.GetPrecept(InternalDefOf.VVE_ImpactDamage_High) != null)
            {
                __result = __result * 1.25f;
            }

         
        }

    }













}
