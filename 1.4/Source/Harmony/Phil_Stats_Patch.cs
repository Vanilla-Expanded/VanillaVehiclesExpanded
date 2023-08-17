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



    [HarmonyPatch(typeof(VehicleInfoCard))]
    [HarmonyPatch("StatsToDraw")]
    public static class VanillaVehiclesExpanded_VehicleInfoCard_StatsToDraw_Patch
    {

        public static void Postfix(VehicleDef ___vehicleDef, ref IEnumerable<VehicleStatDrawEntry> __result)
        {

            if (___vehicleDef.GetModExtension<StatExtension>() != null)
            {
                StatExtension extension = ___vehicleDef.GetModExtension<StatExtension>();
                if (extension.statToAdd != null)
                {
                    foreach (string stat in extension.statToAdd)
                    {
                        __result = __result.AddItem(new VehicleStatDrawEntry(StatCategoryDefOf.BasicsPawn, stat.Translate(), extension.statValues[extension.statToAdd.IndexOf(stat)].Translate(), extension.statDescriptions[extension.statToAdd.IndexOf(stat)].Translate()
                        , 1));
                    }
                }

            }

        }

    }





    [HarmonyPatch(typeof(VehicleInfoCard), "DrawStatsWorker")]
    public static class VanillaVehiclesExpanded_VehicleInfoCard_DrawStatsWorker_Patch
    {
        public static StatExtension extension;


        public static void Prefix(Rect rect, VehicleDef ___vehicleDef)
        {



            if (___vehicleDef.GetModExtension<StatExtension>() != null)
            {

                extension = ___vehicleDef.GetModExtension<StatExtension>();
                if (extension.showImageInInfoCard)
                {


                    Texture2D texture = ContentFinder<Texture2D>.Get(extension.ImageToShowInInfoCard, false);
                    Rect position = rect.AtZero();
                    position.width = 384f;
                    position.height = 576f;
                    position.x = rect.width * 0.75f - position.width / 2f + 18f;
                    position.y = rect.center.y - position.height / 2f + 130;
                    GUI.DrawTexture(position, texture, ScaleMode.ScaleToFit, true);


                }

            }

        }


    }








}
