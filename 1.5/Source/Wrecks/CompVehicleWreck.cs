using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Vehicles;
using Verse;

namespace VanillaVehiclesExpanded
{

    public class CompProperties_VehicleWreck : CompProperties
    {
        public VehicleDef vehicleDef;
        public ResearchProjectDef researchDef;
        public Rot4? spawnRotation;

        public CompProperties_VehicleWreck()
        {
            this.compClass = typeof(CompVehicleWreck);
        }
    }

    public class CompVehicleWreck : ThingComp
    {
        public CompProperties_VehicleWreck Props => base.props as CompProperties_VehicleWreck;

        public static HashSet<CompVehicleWreck> compVehicleWrecks = new HashSet<CompVehicleWreck>();
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compVehicleWrecks.Add(this);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            compVehicleWrecks.Remove(this);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            var restoreWrecksDesignation = parent.Map.designationManager.DesignationOn(parent, VVE_DefOf.VVE_RestoreWreck);
            if (restoreWrecksDesignation == null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "VVE_RestoreWreck".Translate(),
                    defaultDesc = "VVE_RestoreWreckDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/RestoreWreck"),
                    action = delegate
                    {
                        parent.Map.designationManager.AddDesignation(new Designation(parent, VVE_DefOf.VVE_RestoreWreck));
                    }
                };
            }
            else
            {
                var cancelButton = new Command_Action
                {
                    defaultLabel = "Cancel".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel"),
                    action = delegate
                    {
                        parent.Map.designationManager.RemoveDesignation(restoreWrecksDesignation);
                    }
                };
                yield return cancelButton;
            }
        }

        public void RestoreWreck(Pawn worker)
        {
            var vehicleDef = Props.vehicleDef;
            var pos = this.parent.Position;
            var map = this.parent.Map;
            var frame = (Frame)ThingMaker.MakeThing(vehicleDef.buildDef.frameDef, null);
            foreach (var thingCost in vehicleDef.buildDef.CostList)
            {
                var stackCount = (int)(thingCost.count * 0.2f);
                while (stackCount > 0)
                {
                    var thing = ThingMaker.MakeThing(thingCost.thingDef);
                    thing.stackCount = Mathf.Min(thing.def.stackLimit, stackCount);
                    stackCount -= thing.stackCount;
                    frame.GetDirectlyHeldThings().TryAdd(thing);
                }
            }
            frame.SetFactionDirect(worker.Faction);
            var existingRect = GenAdj.OccupiedRect(this.parent);
            this.parent.Destroy();
            var rot = Props.spawnRotation.HasValue ? Props.spawnRotation.Value : this.parent.Rotation;
            GenSpawn.Spawn(frame, GetMatchingPos(existingRect, pos, rot, vehicleDef), map, rot, WipeMode.VanishOrMoveAside);
            GameComponent_VehicleUseTracker.Instance.frameWrecks[frame] = this.parent.def;
        }

        public static IntVec3 GetMatchingPos(CellRect existingRect, IntVec3 cell, Rot4 rot, ThingDef vehicle)
        {
            foreach (var pos in existingRect.Cells)
            {
                var rect = GenAdj.OccupiedRect(pos, rot, vehicle.Size);
                if (rect.minX == existingRect.minX && rect.minZ == existingRect.minZ)
                {
                    return pos;
                }
            }
            return cell;
        }
    }
}
