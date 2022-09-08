using RimWorld;
using System.Collections.Generic;
using UnityEngine;
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
        public static List<GarageDoor> garageDoors = new List<GarageDoor>();
        public int WorkSpeedTicks => 120;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compPower = this.TryGetComp<CompPowerTrader>();
            garageDoors.Add(this);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            garageDoors.Remove(this);
        }
        public override void Draw()
        {
            Vector3 drawPos = DrawPos;
            drawPos.y = AltitudeLayer.DoorMoveable.AltitudeFor();
            var size = def.graphicData.drawSize;
            var openProgress = OpenProgress();
            size.y += (openProgress * 0.7f);
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

            Graphics.DrawMesh(MeshPool.GridPlane(size), drawPos, this.Rotation.AsQuat, def.graphicData
                .GraphicColoredFor(this).MatAt(base.Rotation, this), 0);
            Graphic.ShadowGraphic?.DrawWorker(drawPos, base.Rotation, def, this, 0f);
            Comps_PostDraw();
        }

        public override Color DrawColor
        {
            get
            {
                var color = base.DrawColor;
                var openProgress = OpenProgress();
                color.a /= openProgress + 1;
                return color;
            }
        }

        public float OpenProgress()
        {
            if (this.opened)
            {
                return 1f;
            }
            if (tickOpening > 0)
            {
                return 1f - ((tickOpening - Find.TickManager.TicksGame) / (float)WorkSpeedTicks);
            }
            else if (tickClosing > 0)
            {
                return 1f - ((tickClosing - Find.TickManager.TicksGame) / (float)WorkSpeedTicks);
            }
            return 0f;
        }
        public override void Tick()
        {
            base.Tick();
            if (tickOpening > 0 && Find.TickManager.TicksGame >= tickOpening)
            {
                var openGarage = ThingMaker.MakeThing(ThingDef.Named(this.def.defName + "Opened"), this.Stuff) as GarageDoor;
                openGarage.opened = true;
                SpawnGarage(openGarage);
            }
            else if (tickClosing > 0 && Find.TickManager.TicksGame >= tickClosing)
            {
                var closedGarage = ThingMaker.MakeThing(ThingDef.Named(this.def.defName.Replace("Opened", "")), this.Stuff) as GarageDoor;
                closedGarage.opened = false;
                SpawnGarage(closedGarage);
            }
        }

        private void SpawnGarage(GarageDoor newGarage)
        {
            var wasSelected = Find.Selector.IsSelected(this);
            newGarage.HitPoints = this.HitPoints;
            newGarage.SetFaction(this.Faction);
            var pos = this.Position;
            var map = this.Map;
            this.Destroy();
            GenSpawn.Spawn(newGarage, pos, map, this.Rotation);
            newGarage.compPower.PowerOn = true;
            newGarage.compPower.SetUpPowerVars();
            PowerConnectionMaker.TryConnectToAnyPowerNet(newGarage.compPower);
            if (wasSelected)
            {
                Find.Selector.Select(newGarage);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (base.Map.designationManager.DesignationOn(this, VVE_DefOf.VVE_Open) is null && !opened)
            {
                var openButton = new Command_Action
                {
                    defaultLabel = "VVE_Open".Translate(),
                    defaultDesc = "VVE_OpenDescription".Translate(),
                    icon = ContentFinder<Texture2D>.Get("Things/Building/Structure/GarageDoor_Open"),
                    action = delegate
                    {
                        Designation designation = this.Map.designationManager.DesignationOn(this, VVE_DefOf.VVE_Open);
                        if (designation == null)
                        {
                            this.Map.designationManager.AddDesignation(new Designation(this, VVE_DefOf.VVE_Open));
                        }
                        base.Map.designationManager.DesignationOn(this, VVE_DefOf.VVE_Close)?.Delete();
                    }
                };
                if (compPower.PowerOn is false)
                {
                    openButton.Disable("NoPower".Translate());
                }
                yield return openButton;
            }
            else
            {
                var closeButton = new Command_Action
                {
                    defaultLabel = "VVE_Close".Translate(),
                    defaultDesc = "VVE_CloseDescription".Translate(),
                    icon = ContentFinder<Texture2D>.Get("Things/Building/Structure/GarageDoor_Close"),
                    action = delegate
                    {
                        Designation designation = this.Map.designationManager.DesignationOn(this, VVE_DefOf.VVE_Close);
                        if (designation == null)
                        {
                            this.Map.designationManager.AddDesignation(new Designation(this, VVE_DefOf.VVE_Close));
                        }
                        base.Map.designationManager.DesignationOn(this, VVE_DefOf.VVE_Open)?.Delete();
                    }
                };
                if (compPower.PowerOn is false)
                {
                    closeButton.Disable("NoPower".Translate());
                }
                yield return closeButton;
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
