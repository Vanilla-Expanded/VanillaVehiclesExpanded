using SmashTools;
using System.Collections.Generic;
using System.Linq;
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

    public enum MovementMode { Starting, Accelerate, CurrentSpeed, Decelerate}

    [HotSwappable]
    public class CompVehicleMovementController : VehicleComp
    {
        public VehiclePawn Vehicle => this.parent as VehiclePawn;
        public float currentSpeed;
        public bool wasMoving;
        public MovementMode curMovementMode;
        public List<float> recordedSpeeds = new List<float>();
        public float AccelerationRate => Vehicle.GetStatValue(VVE_DefOf.AccelerationRate);

        public void StartMove()
        {
            if (wasMoving is false)
            {
                currentSpeed = 0;
            }
            curMovementMode = MovementMode.Accelerate;
            recordedSpeeds.Clear();
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
                var averageSpeed = recordedSpeeds.Any() ? recordedSpeeds.Average() : currentSpeed;
                var decelerateMultiplier = ticksToDestination / (averageSpeed / AccelerationRate);
                var decelerateInTicks = ticksToDestination - (averageSpeed / (AccelerationRate));
                //LogMode("Init", ticksToDestination, decelerateMultiplier, decelerateInTicks);
                if ((decelerateInTicks <= 0f) && currentSpeed > 0)
                {
                    Vehicle.Map.debugDrawer.FlashCell(Vehicle.Position, 0.1f, duration: 10000);
                    curMovementMode = MovementMode.Decelerate;
                    var newSpeed = currentSpeed - (AccelerationRate);
                    if (newSpeed <= 0.5f && Vehicle.vPather.nextCellCostLeft > 0)
                    {
                        // do nothing
                    }
                    else
                    {
                        currentSpeed = Mathf.Max(0, newSpeed);
                    }
                    if (decelerateMultiplier <= 0.9f)
                    {
                        LogMode("Handbraking", ticksToDestination, decelerateMultiplier, decelerateInTicks);
                        //Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                    }
                    else
                    { 
                        LogMode("Deceleration", ticksToDestination, decelerateMultiplier, decelerateInTicks); 
                    }
                }
                else if (curMovementMode != MovementMode.Decelerate && GetMoveSpeed() > currentSpeed && decelerateInTicks > 60)
                {
                    curMovementMode = MovementMode.Accelerate;
                    currentSpeed = Mathf.Min(currentSpeed + AccelerationRate, GetMoveSpeed());
                    LogMode("Acceleration", ticksToDestination, decelerateMultiplier, decelerateInTicks);
                    Vehicle.Map.debugDrawer.FlashCell(Vehicle.Position, 0.5f, duration: 10000);
                }
                else
                {
                    Vehicle.Map.debugDrawer.FlashCell(Vehicle.Position, 0.7f, duration: 10000);
                    LogMode("Cur speed intact", ticksToDestination, decelerateMultiplier, decelerateInTicks);
                    curMovementMode = MovementMode.CurrentSpeed;
                }
                recordedSpeeds.Add(currentSpeed);
            }
            Log.ResetMessageCount();
        }

        private void LogMode(string logMode, float ticksToDestination, float decelerateMultiplier, float decelerateInTicks)
        {
            Log.Message(logMode + ": currentSpeed: " + currentSpeed + " average speed: " + (recordedSpeeds.Any() ? recordedSpeeds.Average() : 0) + " move speed: " + GetMoveSpeed() + " ticks left: " + ticksToDestination + " - decelerateInTicks: " + decelerateInTicks + " - decelerateMultiplier: " + decelerateMultiplier + " - next node cost left: " + Vehicle.vPather.nextCellCostLeft + " - Vehicle.vPather.curPath: " + Vehicle.vPather.curPath);
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

            VehicleStatPart_AccelerationRate.modifyValue = true;
            if (c.x == prevCell.x || c.z == prevCell.z)
            {
                num = vehicle.TicksPerMoveCardinal;
            }
            else
            {
                num = vehicle.TicksPerMoveDiagonal;
            }
            VehicleStatPart_AccelerationRate.modifyValue = false;

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
            Scribe_Values.Look(ref wasMoving, "wasMoving");
            Scribe_Values.Look(ref currentSpeed, "currentSpeed");
            Scribe_Values.Look(ref curMovementMode, "curMovementMode");
            Scribe_Collections.Look(ref recordedSpeeds, "recordedSpeeds", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                recordedSpeeds ??= new List<float>();
            }
        }
    }
}
