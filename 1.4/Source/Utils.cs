using HarmonyLib;
using System;
using Verse;
using SmashTools;
using System.Collections.Generic;
using System.Linq;

namespace VanillaVehiclesExpanded
{

    [StaticConstructorOnStartup]
    public static class Utils
    {
        public static List<ThingDef> wreckDefs = new List<ThingDef>();
        static Utils()
        {
            new Harmony("VanillaVehiclesExpanded.Mod").PatchAll();
            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.GetCompProperties<CompProperties_VehicleWreck>() != null)
                {
                    wreckDefs.Add(def);
                }
            }
        }

        public static bool IsDisabled(this ResearchProjectDef __instance)
        {
            return IsDisabled(__instance, out _);
        }

        public static bool IsDisabled(this ResearchProjectDef __instance, out ThingDef wreckNotRestored)
        {
            wreckNotRestored = null;
            foreach (var restoredWreck in GameComponent_VehicleUseTracker.Instance.restoredWrecks)
            {
                if (restoredWreck.GetCompProperties<CompProperties_VehicleWreck>().researchDef == __instance)
                {
                    return false;
                }
            }
            foreach (var wreck in wreckDefs.Where(x => GameComponent_VehicleUseTracker.Instance.restoredWrecks.Contains(x) is false))
            {
                if (wreck.GetCompProperties<CompProperties_VehicleWreck>().researchDef == __instance)
                {
                    wreckNotRestored = wreck;
                    return true;
                }
            }
            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }
}
