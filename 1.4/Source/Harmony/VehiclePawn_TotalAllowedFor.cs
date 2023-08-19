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
    [HarmonyPatch("TotalAllowedFor")]
    public static class VanillaVehiclesExpanded_VehiclePawn_TotalAllowedFor_Patch
    {

        public static void Postfix(JobDef jobDef,ref int __result)
        {

            if(VehicleMod.settings.main.multiplePawnsPerJob)
            {
                
                    if (jobDef.defName == JobDefOf_Vehicles.RepairVehicle.defName)
                    {
                        if(Current.Game.World.factionManager.OfPlayer.ideos.PrimaryIdeo.GetPrecept(InternalDefOf.VVE_VehicleRepairs_Fast) != null) {
                            __result = __result + 1;
                        }
                       
                    }
                


            }

        }

    }





}
