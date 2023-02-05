using RimWorld;
using SmashTools;
using System.Collections.Generic;
using System.Linq;
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
            compClass = typeof(CompVehicleMovementController);
        }
    }

    public enum MovementMode { Starting, Accelerate, CurrentSpeed, Decelerate }

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
        public VehiclePawn Vehicle => parent as VehiclePawn;
        public float currentSpeed;
        public bool wasMoving;
        public MovementMode curMovementMode;
        public float curPaidPathCost;
        public bool isScreeching;
        public bool handbrakeApplied;
        private Sustainer screechingSustainer;
        private float prevPctOfPathPassed;
        private float curPctOfPathPassed;
        private Dictionary<StartAndDestCells, PawnPath> savedPaths = new();

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
            curPctOfPathPassed = 0;
        }

        //public string slowdownMultiplierStr;
        //public string slowdownCheckStr;
        //public string accelerateRateAdjustedStr;
        //public string prevPctOfPathPassedStr;
        //public string curPctOfPathPassedStr;
        //public string decelerateInPctOfPathStr;
        //public override string CompInspectStringExtra()
        //{
        //    var sb = new StringBuilder(base.CompInspectStringExtra());
        //    if (Prefs.DevMode)
        //    {
        //        sb.AppendLine("Default speed: " + GetDefaultMoveSpeed());
        //        sb.AppendLine("Current speed: " + currentSpeed);
        //        sb.AppendLine("Current movement mode: " + curMovementMode);
        //        sb.AppendLine("curPctOfPathPassedStr: " + curPctOfPathPassedStr);
        //        sb.AppendLine("prevPctOfPathPassedStr: " + prevPctOfPathPassedStr);
        //        sb.AppendLine("accelerateRateAdjustedStr: " + accelerateRateAdjustedStr);
        //        sb.AppendLine("slowdownCheckStr: " + slowdownCheckStr);
        //        sb.AppendLine("decelerateInPctOfPathStr: " + decelerateInPctOfPathStr);
        //        sb.AppendLine("Slowdown multiplier: " + slowdownMultiplierStr);
        //        sb.AppendLine("Handbrake applied: " + handbrakeApplied);
        //    }
        //    return sb.ToString().TrimEndNewlines();
        //}

        public override void CompTick()
        {
            base.CompTick();

            if (!Vehicle.Spawned) return; //WorldPawns can tick.  If curPath was not cleared before ticking, exception will be thrown

            wasMoving = Vehicle.vPather.Moving;
            if (wasMoving && Vehicle.vPather.curPath != null)
            {
                float moveSpeed = GetDefaultMoveSpeed();
                float totalCost = GetPathCost(false).cost;
                float decelerateMultiplier = 4f;
                float decelerateInPctOfPath = moveSpeed / (AccelerationRate * decelerateMultiplier) / totalCost;
                curPctOfPathPassed = curPaidPathCost / totalCost;
                //curPctOfPathPassedStr = curPctOfPathPassed.ToString();
                //prevPctOfPathPassedStr = prevPctOfPathPassed.ToString();
                //decelerateInPctOfPathStr = decelerateInPctOfPath.ToString();
                if (decelerateInPctOfPath > 1f)
                {
                    if (prevPctOfPathPassed > curPctOfPathPassed)
                    {
                        Slowdown(decelerateInPctOfPath);
                    }
                    else
                    {
                        if (curPctOfPathPassed <= 0.5f && curMovementMode != MovementMode.Decelerate)
                        {
                            SpeedUp(moveSpeed);
                        }
                        else
                        {
                            Slowdown(decelerateInPctOfPath);
                        }
                    }
                }
                else
                {
                    if (curPctOfPathPassed <= 1f - decelerateInPctOfPath && curMovementMode != MovementMode.Decelerate)
                    {
                        SpeedUp(moveSpeed);
                    }
                    else
                    {
                        Slowdown(decelerateInPctOfPath);
                    }
                }

                prevPctOfPathPassed = curPctOfPathPassed;
                if (isScreeching)
                {
                    if (screechingSustainer == null || screechingSustainer.Ended)
                    {
                        screechingSustainer = VVE_DefOf.VVE_TiresScreech.TrySpawnSustainer(SoundInfo.InMap(parent));
                    }
                    screechingSustainer.Maintain();
                }
            }
            else
            {
                if (wasMoving is false)
                {
                    prevPctOfPathPassed = 0;
                }

                if (isScreeching)
                {
                    isScreeching = false;
                    if (screechingSustainer != null && !screechingSustainer.Ended)
                    {
                        screechingSustainer.End();
                    }
                }
            }
        }

        public void Slowdown(float deceleratePct, bool stopImmediately = false)
        {
            float decelerateMultiplier = 4f;
            float remainingArrivalTicks = stopImmediately ? 10 : GetPathCost(true).cost;
            float accelerateRateAdjusted = AccelerationRate * decelerateMultiplier;
            float targetSpeed = 1f;
            float diff = currentSpeed - targetSpeed;
            float check = currentSpeed / remainingArrivalTicks;
            //accelerateRateAdjustedStr = accelerateRateAdjusted.ToString();
            accelerateRateAdjusted = Mathf.Min(accelerateRateAdjusted, check);
            //slowdownCheckStr = check.ToString();
            float newSpeed = currentSpeed - accelerateRateAdjusted;
            float slowdownMultiplier = currentSpeed / (AccelerationRate * decelerateMultiplier) / remainingArrivalTicks;
            //slowdownMultiplierStr = slowdownMultiplier.ToString();
            //Log.Message(handbrakeApplied + " - " + slowdownMultiplier + " - " + deceleratePct + " - " + currentSpeed + " - " + Vehicle.vPather.curPath.NodesConsumedCount);
            if (handbrakeApplied is false && slowdownMultiplier >= 2f && currentSpeed >= 3f && (stopImmediately || (deceleratePct > 1f && Vehicle.vPather.curPath.NodesConsumedCount < 2)))
            {
                newSpeed /= slowdownMultiplier;
                isScreeching = true;
                Messages.Message("VVE_HandbrakeWarning".Translate(Vehicle.Named("VEHICLE")), MessageTypeDefOf.NegativeHealthEvent);
                int damageAmount = Mathf.CeilToInt(slowdownMultiplier);
                var components = Vehicle.statHandler.components.Where(x => x.props.tags != null && x.props.tags.Contains("Wheel"));
                foreach (var component in components)
                {
                    component.TakeDamage(Vehicle, new DamageInfo(DamageDefOf.Scratch, damageAmount));
                }
                handbrakeApplied = true;
            }

            newSpeed = Mathf.Max(1f, newSpeed);
            if (currentSpeed > newSpeed)
            {
                currentSpeed = newSpeed;
            }
            curMovementMode = MovementMode.Decelerate;
        }

        private void SpeedUp(float moveSpeed)
        {
            if (moveSpeed > currentSpeed)
            {
                curMovementMode = MovementMode.Accelerate;
                currentSpeed = Mathf.Min(currentSpeed + AccelerationRate, moveSpeed);
            }
            else
            {
                curMovementMode = MovementMode.CurrentSpeed;
            }
        }
        public PathCostResult GetPathCost(bool ignorePassedCells)
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
            var edifice = c.GetEdifice(vehicle.Map);
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
                var locomotionUrgencySameAs = vehicle.jobs.curDriver.locomotionUrgencySameAs;
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
            Scribe_Values.Look(ref curPctOfPathPassed, "curPctOfPathPassed");
            Scribe_Values.Look(ref prevPctOfPathPassed, "prevPctOfPathPassed");
        }
    }
}
