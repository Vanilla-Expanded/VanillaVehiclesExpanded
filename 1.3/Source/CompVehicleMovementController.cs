using RimWorld;
using SmashTools;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.AI;
using Verse.Sound;

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

    public struct StartAndDestCells
    {
        public IntVec3 start;
        public IntVec3 dest;
        public override bool Equals(object obj)
        {
            if (obj is StartAndDestCells other)
            {
                return this == other;
            }
            return false;
        }
        public static bool operator ==(StartAndDestCells a, StartAndDestCells b)
        {
            if (a.start == b.start && a.dest == b.dest)
            {
                return true;
            }
            return false;
        }

        public static bool operator !=(StartAndDestCells a, StartAndDestCells b)
        {
            if (a.start != b.start || a.dest != b.dest)
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (start + dest).GetHashCode();
        }
    }

    public struct PathCostResult
    {
        public float cost;
        public int cells;

        public override string ToString()
        {
            return "Cost: " + cost + " - cells: " + cells;
        }
    }
    [HotSwappable]
    public class CompVehicleMovementController : VehicleComp
    {
        public VehiclePawn Vehicle => this.parent as VehiclePawn;
        public float currentSpeed;
        public bool wasMoving;
        public MovementMode curMovementMode;
        public float curPaidPathCost;
        public bool isScreeching;
        public bool handbrakeApplied;
        private Sustainer screechingSustainer;
        private Dictionary<StartAndDestCells, PawnPath> savedPaths = new Dictionary<StartAndDestCells, PawnPath>();
        private List<float> speedRecords = new List<float>();
        public float AccelerationRate => Vehicle.GetStatValue(VVE_DefOf.AccelerationRate);
        public void StartMove()
        {
            if (wasMoving is false)
            {
                currentSpeed = 0;
            }
            curMovementMode = MovementMode.Accelerate;
            curPaidPathCost = 0;
            handbrakeApplied = false;
            savedPaths.Clear();
            speedRecords.Clear();
            Log.Message(Vehicle + " started move");
        }

        public override void CompTick()
        {
            base.CompTick();
            wasMoving = Vehicle.vPather.Moving;
            if (wasMoving && Vehicle.vPather.curPath != null)
            {
                var moveSpeed = GetDefaultMoveSpeed();
                var totalCost = GetPathCost(false).cost;
                var decelerateMultiplier = 4f;
                //var decelerateInPctOfPath = ((speedRecords.Any() ? speedRecords.Average() : currentSpeed) 
                //    / (AccelerationRate * decelerateMultiplier)) / totalCost;
                var decelerateInPctOfPath = (moveSpeed / (AccelerationRate * decelerateMultiplier)) / totalCost;

                var pctOfPathPassed = (curPaidPathCost / totalCost);
                if (decelerateInPctOfPath > 1f)
                {
                    if (pctOfPathPassed <= 0.5f && curMovementMode != MovementMode.Decelerate)
                    {
                        SpeedUp(moveSpeed, decelerateInPctOfPath, pctOfPathPassed);
                    }
                    else
                    {
                        Slowdown(decelerateInPctOfPath, pctOfPathPassed);
                    }
                }
                else
                {
                    if (pctOfPathPassed <= 1f - decelerateInPctOfPath && curMovementMode != MovementMode.Decelerate)
                    {
                        SpeedUp(moveSpeed, decelerateInPctOfPath, pctOfPathPassed);
                    }
                    else
                    {
                        Slowdown(decelerateInPctOfPath, pctOfPathPassed);
                    }
                }

                if (isScreeching)
                {
                    if (screechingSustainer == null || screechingSustainer.Ended)
                    {
                        screechingSustainer = VVE_DefOf.VVE_TiresScreech.TrySpawnSustainer(SoundInfo.InMap(parent));
                    }
                    screechingSustainer.Maintain();
                }
                speedRecords.Add(currentSpeed);
            }
            else if (isScreeching)
            {
                isScreeching = false;
                if (screechingSustainer != null && !screechingSustainer.Ended)
                {
                    screechingSustainer.End();
                }
            }
            Log.ResetMessageCount();
        }

        private void Slowdown(float deceleratePct, float pctPassed)
        {
            curMovementMode = MovementMode.Decelerate;
            var decelerateMultiplier = 4f;
            var result = GetPathCost(true);
            var remainingArrivalTicks = result.cost;
            var accelerateRateAdjusted = AccelerationRate * decelerateMultiplier;
            var targetSpeed = 1f;
            var diff = currentSpeed - targetSpeed;
            var check = currentSpeed / remainingArrivalTicks;
            accelerateRateAdjusted = Mathf.Min(accelerateRateAdjusted, check);
            var newSpeed = currentSpeed - accelerateRateAdjusted;
            var sbLog = "accelerateRateAdjusted: " + accelerateRateAdjusted + " - AccelerationRate * decelerateMultiplier: " 
                + (AccelerationRate * decelerateMultiplier) + " - check: " + (check);
            var slowdownMultiplier = (currentSpeed / (AccelerationRate * decelerateMultiplier)) / remainingArrivalTicks;
            if (handbrakeApplied is false && slowdownMultiplier >= 2f && deceleratePct > 1f)
            {
                newSpeed /= slowdownMultiplier;
                isScreeching = true;
                Messages.Message("VVE_HandbrakeWarning".Translate(Vehicle.Named("VEHICLE")), MessageTypeDefOf.NegativeHealthEvent);
                var damageAmount = Mathf.CeilToInt(slowdownMultiplier);
                Log.Message("Damage: " + damageAmount);
                sbLog += ": slowdownMultiplier: " + slowdownMultiplier + " - (currentSpeed / AccelerationRate): " + (currentSpeed / AccelerationRate);
                LogMode("Handbrake: " + sbLog, pctPassed, deceleratePct);
                Vehicle.Map.debugDrawer.FlashCell(Vehicle.Position, 0.1f, duration: 10000);
                //VehicleComponent component = Vehicle.statHandler.components.Where(x => x.props.tags
                handbrakeApplied = true;
            }
            else
            {
                LogMode("Deceleration: " + sbLog, pctPassed, deceleratePct);
                Vehicle.Map.debugDrawer.FlashCell(Vehicle.Position, 0.1f, duration: 10000);
            }
            newSpeed = Mathf.Max(1f, newSpeed);
            if (currentSpeed > newSpeed)
            {
                currentSpeed = newSpeed;
            }
        }

        private void SpeedUp(float moveSpeed, float deceleratePct, float pctPassed)
        {
            if (moveSpeed > currentSpeed)
            {
                curMovementMode = MovementMode.Accelerate;
                currentSpeed = Mathf.Min(currentSpeed + AccelerationRate, moveSpeed);
                LogMode("Acceleration", pctPassed, deceleratePct);
                Vehicle.Map.debugDrawer.FlashCell(Vehicle.Position, 0.5f, duration: 10000);
            }
            else
            {
                LogMode("Cur speed", pctPassed, deceleratePct);
                Vehicle.Map.debugDrawer.FlashCell(Vehicle.Position, 0.7f, duration: 10000);
                curMovementMode = MovementMode.CurrentSpeed;
            }
        }

        private void LogMode(string logMode, float pctPassed, float deceleratePct)
        {
            Log.Message(logMode + " cur position: " + Vehicle.Position +  " - currentSpeed: " + currentSpeed
                + " - curPaidPathCost: " + curPaidPathCost + " - pctOfPathPassed: "
                + pctPassed + " - decelerateInPctOfPath: " + deceleratePct + " - GetTicksToDestination: " + GetPathCost(true) +
                " - TotalCost: " + GetPathCost(false));
        }
        private PathCostResult GetPathCost(bool ignorePassedCells)
        {
            var path = Vehicle.vPather.curPath;
            var result = GetPathCost(path, ignorePassedCells);
            result.cost += Vehicle.vPather.nextCellCostTotal - Vehicle.vPather.nextCellCostLeft;
            var otherResult = GetPathCostsFromQueuedJobs(path.LastNode);
            result.cost += otherResult.cost;
            result.cells += otherResult.cells;
            return result;
        }

        private PathCostResult GetPathCostsFromQueuedJobs(IntVec3 startPos)
        {
            var result = default(PathCostResult);
            foreach (var queueJob in Vehicle.jobs.jobQueue)
            {
                if (queueJob.job.def == JobDefOf.Goto)
                {
                    var path = GetPawnPath(startPos, queueJob.job.targetA.Cell);
                    if (path != null)
                    {
                        startPos = path.LastNode;
                        var otherResult = GetPathCost(path, ignorePassedCells: false); ;
                        result.cost += otherResult.cost;
                        result.cells += otherResult.cells;
                    }
                }
                else
                {
                    break;
                }
            }
            return result;
        }

        private PawnPath GetPawnPath(IntVec3 start, IntVec3 dest)
        {
            var key = new StartAndDestCells { start = start, dest = dest };
            if (!savedPaths.TryGetValue(key, out var path))
            {
                savedPaths[key] = path = Vehicle.Map.pathFinder.FindPath(start, dest, Vehicle, PathEndMode.OnCell);
            }
            return path;
        }

        public PathCostResult GetPathCost(PawnPath path, bool ignorePassedCells)
        {
            var result = default(PathCostResult);
            if (path != null)
            {
                var prevCell = Vehicle.Position;
                bool startCalculation = false;
                var nodes = path.NodesReversed.ListFullCopy();
                nodes.Reverse();
                foreach (var cell in nodes)
                {
                    if (startCalculation || !ignorePassedCells)
                    {
                        result.cost += CostToMoveIntoCell(Vehicle, prevCell, cell);
                        result.cells++;
                    }
                    if (cell == Vehicle.Position)
                    {
                        startCalculation = true;
                    }
                    prevCell = cell;
                }
            }
            return result;
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
        public float GetDefaultMoveSpeed()
        {
            return Vehicle.GetStatValue(VehicleStatDefOf.MoveSpeed);
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref curPaidPathCost, "curPaidPathCost");
            Scribe_Values.Look(ref wasMoving, "wasMoving");
            Scribe_Values.Look(ref currentSpeed, "currentSpeed");
            Scribe_Values.Look(ref curMovementMode, "curMovementMode");
            Scribe_Values.Look(ref isScreeching, "isScreeching");
            Scribe_Values.Look(ref handbrakeApplied, "handbrakeApplied");
            Scribe_Collections.Look(ref speedRecords, "speedRecords");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                speedRecords ??= new List<float>();
            }
        }
    }
}
