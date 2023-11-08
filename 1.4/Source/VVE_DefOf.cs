using RimWorld;
using Vehicles;
using Verse;

namespace VanillaVehiclesExpanded
{
    [DefOf]
    public static class VVE_DefOf
    {
        public static VehicleStatDef AccelerationRate;
        public static SoundDef VVE_TiresScreech;
        public static JobDef VVE_OpenGarage;
        public static JobDef VVE_CloseGarage;
        public static DesignationDef VVE_Open;
        public static DesignationDef VVE_RestoreWreck;
        [DefAlias("VVE_RestoreWreck")] public static JobDef VVE_RestoreWreckJob;
        public static DesignationDef VVE_Close;
        public static FleckDef VVE_Scytheman_RocketSmoke, RocketExhaust_Short, VVE_Scytheman_RocketExhaust;

    }
}
