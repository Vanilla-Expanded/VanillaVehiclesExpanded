using SmashTools;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.AI;

namespace VanillaVehiclesExpanded
{
    public class CompProperties_VehicleMovementController : VehicleCompProperties
    {
        public CompProperties_VehicleMovementController()
        {
            this.compClass = typeof(CompVehicleMovementController);
        }
    }

    public enum MovementMode { Accelerate, MaxSpeed, Decelerate}
    [HotSwappable]
    public class CompVehicleMovementController : VehicleComp
    {
        public VehiclePawn Vehicle => this.parent as VehiclePawn;
        public float currentSpeed;
        public bool wasMoving;
        private float AccelerationRate => Vehicle.GetStatValue(VVE_DefOf.AccelerationRate);

        public MovementMode curMovementMode;
        public void StartMove()
        {
            if (wasMoving is false)
            {
                currentSpeed = 0;
            }
            curMovementMode = MovementMode.Accelerate;
            Log.Message("Starting move: " + currentSpeed + " - " + wasMoving);
        }

        public override void CompTick()
        {
            base.CompTick();
            wasMoving = Vehicle.vPather.Moving;
            if (wasMoving)
            {
                var ticksToDestination = GetTicksToDestination();
                if (Vehicle.vPather.curPath != null)
                {
                    var firstNodeCost = CostToMoveIntoCell(Vehicle, Vehicle.Position, Vehicle.vPather.curPath.FirstNode);
                    ticksToDestination -= firstNodeCost - Vehicle.vPather.nextCellCostLeft;
                }
                var decelerateInTicks = ticksToDestination - (currentSpeed / AccelerationRate);
                var decelerateMultiplier = ticksToDestination / (currentSpeed / AccelerationRate);
                if (decelerateInTicks <= 10f && currentSpeed > 0)
                {
                    curMovementMode = MovementMode.Decelerate;
                    if (decelerateMultiplier <= 0.9f)
                    {
                        Log.Message("Handbraking");
                        Vehicle.Map.debugDrawer.FlashCell(Vehicle.Position);
                        Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                    }
                    currentSpeed = Mathf.Max(0, currentSpeed - (AccelerationRate / decelerateMultiplier));
                    Log.Message("Deceleration: - currentSpeed: " + currentSpeed + " ticks left: " + ticksToDestination + " - decelerateInTicks: " + decelerateInTicks + " - decelerateMultiplier: " + decelerateMultiplier + " - next node cost left: " + Vehicle.vPather.nextCellCostLeft);
                }
                else if (curMovementMode != MovementMode.Decelerate && GetMoveSpeed() > currentSpeed)
                {
                    curMovementMode = MovementMode.Accelerate;
                    currentSpeed = Mathf.Min(currentSpeed + AccelerationRate, GetMoveSpeed());
                    Log.Message("Accelerate: - currentSpeed: " + currentSpeed + " ticks left: " + ticksToDestination + " - decelerateInTicks: " + decelerateInTicks + " - decelerateMultiplier: " + decelerateMultiplier + " - next node cost left: " + Vehicle.vPather.nextCellCostLeft);
                }
                else
                {
                    curMovementMode = MovementMode.MaxSpeed;
                    Log.Message("Max speed: - currentSpeed: " + currentSpeed + " ticks left: " + ticksToDestination + " - decelerateInTicks: " + decelerateInTicks + " - decelerateMultiplier: " + decelerateMultiplier + " - next node cost left: " + Vehicle.vPather.nextCellCostLeft);
                }
            }
        }
        public float GetTicksToDestination()
        {
            var cost = 0f;
            if (Vehicle.vPather.curPath != null)
            {
                var prevCell = Vehicle.Position;
                bool startCalculation = false;
                var nodes = Vehicle.vPather.curPath.NodesReversed.ListFullCopy();
                nodes.Reverse();
                foreach (var cell in nodes)
                {
                    if (startCalculation)
                    {
                        cost += CostToMoveIntoCell(Vehicle, prevCell, cell);
                    }
                    if (cell == Vehicle.Position)
                    {
                        startCalculation = true;
                    }
                    prevCell = cell;
                }
            }
            return cost;
        }

        private static int CostToMoveIntoCell(VehiclePawn vehicle, IntVec3 prevCell, IntVec3 c)
        {
            int num;
            if (c.x == prevCell.x || c.z == prevCell.z)
            {
                num = vehicle.TicksPerMoveCardinal;
            }
            else
            {
                num = vehicle.TicksPerMoveDiagonal;
            }
            num += vehicle.Map.GetCachedMapComponent<VehicleMapping>()[vehicle.VehicleDef].VehiclePathGrid.CalculatedCostAt(c);
            Building edifice = c.GetEdifice(vehicle.Map);
            if (edifice != null)
            {
                num += edifice.PathWalkCostFor(vehicle);
            }
            if (num > Vehicle_PathFollower.MaxMoveTicks)
            {
                num = Vehicle_PathFollower.MaxMoveTicks;
            }
            if (vehicle.CurJob != null)
            {
                Pawn locomotionUrgencySameAs = vehicle.jobs.curDriver.locomotionUrgencySameAs;
                if (locomotionUrgencySameAs is VehiclePawn locomotionVehicle && locomotionUrgencySameAs != vehicle && locomotionUrgencySameAs.Spawned)
                {
                    int num2 = CostToMoveIntoCell(locomotionVehicle, prevCell, c);
                    if (num < num2)
                    {
                        num = num2;
                    }
                }
                else
                {
                    switch (vehicle.jobs.curJob.locomotionUrgency)
                    {
                        case LocomotionUrgency.Amble:
                            num *= 3;
                            if (num < Vehicle_PathFollower.MinCostAmble)
                            {
                                num = Vehicle_PathFollower.MinCostAmble;
                            }
                            break;
                        case LocomotionUrgency.Walk:
                            num *= 2;
                            if (num < Vehicle_PathFollower.MinCostWalk)
                            {
                                num = Vehicle_PathFollower.MinCostWalk;
                            }
                            break;
                        case LocomotionUrgency.Jog:
                            break;
                        case LocomotionUrgency.Sprint:
                            num = Mathf.RoundToInt(num * 0.75f);
                            break;
                    }
                }
            }
            return Mathf.Max(num, 1);
        }
        public float GetMoveSpeed()
        {
            return Vehicle.GetStatValue(VehicleStatDefOf.MoveSpeed);
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentSpeed, "currentSpeed");
            Scribe_Values.Look(ref curMovementMode, "curMovementMode");
        }
    }
}
