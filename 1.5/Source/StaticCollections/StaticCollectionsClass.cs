
using Verse;
using System;
using RimWorld;
using System.Collections.Generic;
using System.Linq;


namespace VanillaVehiclesExpanded
{

    public static class StaticCollectionsClass
    {

        //This static class stores lists for different things.


        public static Dictionary<Pawn, int> colonist_landvehicle_tracker = new Dictionary<Pawn, int>();
        public static Dictionary<Pawn, int> colonist_airvehicle_tracker = new Dictionary<Pawn, int>();
        public static Dictionary<Pawn, int> colonist_seavehicle_tracker = new Dictionary<Pawn, int>();

        public static void AddOrSetColonistToLandVehicleList(Pawn pawn, int days)
        {
            if (pawn != null)
            {
                if (!colonist_landvehicle_tracker.ContainsKey(pawn)||days==0)
                {
                    colonist_landvehicle_tracker[pawn] = 0;
                }              
                else
                colonist_landvehicle_tracker[pawn] += days;
            } 
        }
        public static void RemoveColonistFromLandVehicleList(Pawn pawn)
        {
            if (pawn != null && colonist_landvehicle_tracker.ContainsKey(pawn))
            {
                colonist_landvehicle_tracker.Remove(pawn);
            }
        }

        public static void AddOrSetColonistToAirVehicleList(Pawn pawn, int days)
        {
            if (pawn != null)
            {
                if (!colonist_airvehicle_tracker.ContainsKey(pawn) || days == 0)
                {
                    colonist_airvehicle_tracker[pawn] = 0;
                }
                else
                    colonist_airvehicle_tracker[pawn] += days;
            }
        }
        public static void RemoveColonistFromAirVehicleList(Pawn pawn)
        {
            if (pawn != null && colonist_airvehicle_tracker.ContainsKey(pawn))
            {
                colonist_airvehicle_tracker.Remove(pawn);
            }
        }

        public static void AddOrSetColonistToSeaVehicleList(Pawn pawn, int days)
        {
            if (pawn != null)
            {
                if (!colonist_seavehicle_tracker.ContainsKey(pawn) || days == 0)
                {
                    colonist_seavehicle_tracker[pawn] = 0;
                }
                else
                    colonist_seavehicle_tracker[pawn] += days;
            }
        }
        public static void RemoveColonistFromSeaVehicleList(Pawn pawn)
        {
            if (pawn != null && colonist_seavehicle_tracker.ContainsKey(pawn))
            {
                colonist_seavehicle_tracker.Remove(pawn);
            }
        }





    }
}
