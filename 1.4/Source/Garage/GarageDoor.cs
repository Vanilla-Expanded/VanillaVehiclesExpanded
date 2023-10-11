using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.Sound;

namespace VanillaVehiclesExpanded
{
    [HotSwappable]
    public class GarageDoor : Building
    {
        public CompPowerTrader compPower;
        public bool opened;
        public int tickOpening;
        public int tickClosing;
        public static List<GarageDoor> garageDoors = new();

        public int WorkSpeedTicks => 120;

        public override Color DrawColor
        {
            get
            {
                var color = base.DrawColor;
                float openProgress = OpenProgress();
                color.a /= openProgress + 1;
                return color;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            garageDoors.Add(this);
            compPower = this.TryGetComp<CompPowerTrader>();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            garageDoors.Remove(this);
        }

        public override void Draw()
        {
            var oldDrawSize = def.graphicData.drawSize;
            var drawPos = DrawPos;
            drawPos.y += 5f;
            var size = def.graphicData.drawSize = new Vector2(def.size.x, def.size.z);
            float openProgress = OpenProgress();
            size.y += openProgress * 0.7f;
            if (Rotation == Rot4.South)
            {
                drawPos.z += openProgress;
            }
            else if (Rotation == Rot4.North)
            {
                drawPos.z -= openProgress;
            }
            else if (Rotation == Rot4.West)
            {
                drawPos.x += openProgress;
            }
            else if (Rotation == Rot4.East)
            {
                drawPos.x -= openProgress;
            }

            Graphics.DrawMesh(MeshPool.GridPlane(size), drawPos, Rotation.AsQuat, def.graphicData
                .GraphicColoredFor(this).MatAt(base.Rotation, this), 0);
            Graphic.ShadowGraphic?.DrawWorker(drawPos, base.Rotation, def, this, 0f);
            Comps_PostDraw();
            def.graphicData.drawSize = oldDrawSize;
        }

        public float OpenProgress()
        {
            if (tickOpening > 0)
            {
                return 1f - ((tickOpening - Find.TickManager.TicksGame) / (float)WorkSpeedTicks);
            }
            else if (tickClosing > 0)
            {
                return ((tickClosing - Find.TickManager.TicksGame) / (float)WorkSpeedTicks);
            }
            if (opened)
            {
                return 1f;
            }
            return 0f;
        }

        public override void Tick()
        {
            base.Tick();
            if (tickOpening > 0 && Find.TickManager.TicksGame >= tickOpening)
            {
                var openGarage = ThingMaker.MakeThing(ThingDef.Named(def.defName + "Opened"), Stuff) as GarageDoor;
                openGarage.opened = true;
                SpawnGarage(openGarage);
            }
            else if (tickClosing > 0 && Find.TickManager.TicksGame >= tickClosing)
            {
                var closedGarage = ThingMaker.MakeThing(ThingDef.Named(def.defName.Replace("Opened", "")), Stuff) as GarageDoor;
                closedGarage.opened = false;
                SpawnGarage(closedGarage);
            }
        }

        private void SpawnGarage(GarageDoor newGarage)
        {
            bool wasSelected = Find.Selector.IsSelected(this);
            newGarage.HitPoints = HitPoints;
            newGarage.SetFaction(Faction);
            var pos = Position;
            var map = Map;
            Destroy();
            GenSpawn.Spawn(newGarage, pos, map, Rotation);
            if (wasSelected)
            {
                Find.Selector.Select(newGarage);
            }
            if (newGarage.compPower != null)
            {
                newGarage.compPower.PowerOn = true;
                newGarage.compPower.SetUpPowerVars();
                PowerConnectionMaker.TryConnectToAnyPowerNet(newGarage.compPower);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            foreach (var doorGizmo in GetDoorGizmos())
            {
                yield return doorGizmo;
            }
        }

        protected virtual IEnumerable<Gizmo> GetDoorGizmos() 
        {
            var openDesignation = base.Map.designationManager.DesignationOn(this, VVE_DefOf.VVE_Open);
            var closeDesignation = base.Map.designationManager.DesignationOn(this, VVE_DefOf.VVE_Close);
            bool openDesignationOn = openDesignation != null;
            bool closeDesignationOn = closeDesignation != null;
            if (openDesignationOn is false && !opened)
            {
                var openButton = new Command_Action
                {
                    defaultLabel = "VVE_Open".Translate(),
                    defaultDesc = "VVE_OpenDescription".Translate(),
                    icon = ContentFinder<Texture2D>.Get("Things/Building/Structure/GarageDoor_Open"),
                    action = delegate
                    {
                        var designation = Map.designationManager.DesignationOn(this, VVE_DefOf.VVE_Open);
                        if (designation == null)
                        {
                            Map.designationManager.AddDesignation(new Designation(this, VVE_DefOf.VVE_Open));
                        }
                        base.Map.designationManager.DesignationOn(this, VVE_DefOf.VVE_Close)?.Delete();
                    }
                };
                yield return openButton;
            }
            else if (openDesignationOn && !opened)
            {
                var cancelButton = new Command_Action
                {
                    defaultLabel = "Cancel".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel"),
                    action = delegate
                    {
                        base.Map.designationManager.RemoveDesignation(openDesignation);
                    }
                };
                yield return cancelButton;
            }
            else if (closeDesignationOn is false && opened)
            {
                var closeButton = new Command_Action
                {
                    defaultLabel = "VVE_Close".Translate(),
                    defaultDesc = "VVE_CloseDescription".Translate(),
                    icon = ContentFinder<Texture2D>.Get("Things/Building/Structure/GarageDoor_Close"),
                    action = delegate
                    {
                        var designation = Map.designationManager.DesignationOn(this, VVE_DefOf.VVE_Close);
                        if (designation == null)
                        {
                            Map.designationManager.AddDesignation(new Designation(this, VVE_DefOf.VVE_Close));
                        }
                        base.Map.designationManager.DesignationOn(this, VVE_DefOf.VVE_Open)?.Delete();
                    }
                };
                yield return closeButton;
                CheckVehicleObstructingClosingGarage(closeButton);
            }
            else if (closeDesignationOn && opened)
            {
                var cancelButton = new Command_Action
                {
                    defaultLabel = "Cancel".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel"),
                    action = delegate
                    {
                        base.Map.designationManager.RemoveDesignation(closeDesignation);
                    }
                };
                yield return cancelButton;
            }
        }

        public void CheckVehicleObstructingClosingGarage(Command_Action command)
        {
            var cellRect = this.OccupiedRect();
            if (cellRect.Cells.Any(x => x.GetFirstThing<VehiclePawn>(this.Map) != null))
            {
                command.Disable("VVE_VehicleObstructingClosingGarage".Translate());
            }
        }

        public void StartOpening()
        {
            base.Map.designationManager.DesignationOn(this, VVE_DefOf.VVE_Open)?.Delete();
            tickOpening = Find.TickManager.TicksGame + WorkSpeedTicks;
            def.building.soundDoorOpenPowered.PlayOneShot(new TargetInfo(base.Position, base.Map));
        }

        public void StartClosing()
        {
            base.Map.designationManager.DesignationOn(this, VVE_DefOf.VVE_Close)?.Delete();
            def.building.soundDoorClosePowered.PlayOneShot(new TargetInfo(base.Position, base.Map));
            tickClosing = Find.TickManager.TicksGame + WorkSpeedTicks;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref opened, "opened");
            Scribe_Values.Look(ref tickOpening, "tickOpening");
            Scribe_Values.Look(ref tickClosing, "tickClosing");
        }
    }
}
