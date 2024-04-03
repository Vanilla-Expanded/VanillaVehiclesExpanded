using System;
using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;

namespace VanillaVehiclesExpanded
{
    public class GameComponent_VehicleUseTracker : GameComponent
    {
        public HashSet<ThingDef> restoredWrecks = new HashSet<ThingDef>();
        public Dictionary<Frame, ThingDef> frameWrecks = new();
        public int tickCounter = 0;
        public int tickInterval = 60000;
        public Dictionary<Pawn, int> colonist_landvehicle_tracker_backup = new Dictionary<Pawn, int>();
        List<Pawn> list2;
        List<int> list3;

        public Dictionary<Pawn, int> colonist_airvehicle_tracker_backup = new Dictionary<Pawn, int>();
        List<Pawn> list4;
        List<int> list5;

        public Dictionary<Pawn, int> colonist_seavehicle_tracker_backup = new Dictionary<Pawn, int>();
        List<Pawn> list6;
        List<int> list7;


        public static GameComponent_VehicleUseTracker Instance;

        public GameComponent_VehicleUseTracker()
        {
            Instance = this;
        }

        public GameComponent_VehicleUseTracker(Game game) : base()
        {
            Instance = this;
        }

        public override void FinalizeInit()
        {
            StaticCollectionsClass.colonist_landvehicle_tracker = colonist_landvehicle_tracker_backup;
            StaticCollectionsClass.colonist_airvehicle_tracker = colonist_airvehicle_tracker_backup;
            StaticCollectionsClass.colonist_seavehicle_tracker = colonist_seavehicle_tracker_backup;

            base.FinalizeInit();

        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref colonist_landvehicle_tracker_backup, "colonist_landvehicle_tracker_backup", LookMode.Reference, LookMode.Value, ref list2, ref list3);
            Scribe_Collections.Look(ref colonist_airvehicle_tracker_backup, "colonist_airvehicle_tracker_backup", LookMode.Reference, LookMode.Value, ref list4, ref list5);
            Scribe_Collections.Look(ref colonist_seavehicle_tracker_backup, "colonist_seavehicle_tracker_backup", LookMode.Reference, LookMode.Value, ref list6, ref list7);
            Scribe_Collections.Look(ref restoredWrecks, "restoredWrecks", LookMode.Def);
            Scribe_Collections.Look(ref frameWrecks, "frameWrecks", LookMode.Reference, LookMode.Def, ref frameKeys, ref thingDefValues);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                restoredWrecks ??= new HashSet<ThingDef>();
                frameWrecks ??= new();
            }
        }

        private List<Frame> frameKeys;
        private List<ThingDef> thingDefValues;

        public override void GameComponentTick()
        {


            tickCounter++;
            if ((tickCounter > tickInterval))
            {
                if (Current.Game.World.factionManager.OfPlayer.ideos.GetPrecept(InternalDefOf.VVE_Driving_Required) != null)
                {
                    colonist_landvehicle_tracker_backup = StaticCollectionsClass.colonist_landvehicle_tracker;
                    List<Pawn> listPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists;
                    foreach (Pawn p in listPawns)
                    {
                        if (p.ideo?.Ideo?.HasPrecept(InternalDefOf.VVE_Driving_Required) == true)
                        {
                            StaticCollectionsClass.AddOrSetColonistToLandVehicleList(p, 1);                           
                        }else StaticCollectionsClass.RemoveColonistFromLandVehicleList(p);
                    }
                }
                if (Current.Game.World.factionManager.OfPlayer.ideos.GetPrecept(InternalDefOf.VVE_Flying_Required) != null)
                {
                    colonist_airvehicle_tracker_backup = StaticCollectionsClass.colonist_airvehicle_tracker;
                    List<Pawn> listPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists;
                    foreach (Pawn p in listPawns)
                    {
                        if (p.ideo?.Ideo?.HasPrecept(InternalDefOf.VVE_Flying_Required) == true)
                        {
                            StaticCollectionsClass.AddOrSetColonistToAirVehicleList(p, 1);
                        }
                        else StaticCollectionsClass.RemoveColonistFromAirVehicleList(p);
                    }
                }
                if (Current.Game.World.factionManager.OfPlayer.ideos.GetPrecept(InternalDefOf.VVE_Sailing_Required) != null)
                {
                    colonist_seavehicle_tracker_backup = StaticCollectionsClass.colonist_seavehicle_tracker;
                    List<Pawn> listPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists;
                    foreach (Pawn p in listPawns)
                    {
                        if (p.ideo?.Ideo?.HasPrecept(InternalDefOf.VVE_Sailing_Required) == true)
                        {
                            StaticCollectionsClass.AddOrSetColonistToSeaVehicleList(p, 1);
                        }
                        else StaticCollectionsClass.RemoveColonistFromSeaVehicleList(p);
                    }
                }



                tickCounter = 0;
            }



        }


    }


}
