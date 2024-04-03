using RimWorld;
using HarmonyLib;
using Verse.Profile;

namespace VanillaVehiclesExpanded
{
    [HarmonyPatch(typeof(MemoryUtility), "UnloadUnusedUnityAssets")]
    public static class MemoryUtility_UnloadUnusedUnityAssets_Patch
    {
        public static void Postfix()
        {
            GarageDoor.garageDoors.Clear();
            CompVehicleWreck.compVehicleWrecks.Clear();
        }
    }





   







}
