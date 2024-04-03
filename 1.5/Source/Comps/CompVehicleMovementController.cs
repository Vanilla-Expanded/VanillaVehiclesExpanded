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
    [HotSwappable]
    public class CompVehicleMovementController : VehicleComp
    {
        private float DecelerationMultiplier = 4;
        private float MaxSpeedToDecelerateSmoothly = 2.5f;

        public float currentSpeed;
        public bool wasMoving;
        public MovementMode curMovementMode;
        public float curPaidPathCost;
        public int curPaidPathCostTickChecked;
        public bool isScreeching;
        public bool handbrakeApplied;
        private Sustainer screechingSustainer;
        private float prevPctOfPathPassed;
        private float curPctOfPathPassed;

        private Dictionary<StartAndDestCells, PawnPath> savedPaths = new();

        public float AccelerationRate => Vehicle.GetStatValue(VVE_DefOf.AccelerationRate);

        public float StatMoveSpeed => Vehicle.GetStatValue(VehicleStatDefOf.MoveSpeed);

        public void StartMove()
        {
            wasMoving = Vehicle.vehiclePather.Moving;
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

        public override void CompTick()
        {
            base.CompTick();

            if (!Vehicle.Spawned) return; //WorldPawns can tick.  If curPath was not cleared before ticking, exception will be thrown
            
            wasMoving = Vehicle.vehiclePather.Moving;
            if (wasMoving && Vehicle.vehiclePather.curPath != null)
            {
                float moveSpeed = StatMoveSpeed;
                float totalPathCost = GetPathCost(false).cost;
                float remainingArrivalTicks = GetPathCost(true).cost;
                float decelerateInPctOfPath = moveSpeed / (AccelerationRate * DecelerationMultiplier) / totalPathCost;
                curPctOfPathPassed = curPaidPathCost / totalPathCost;
                if (decelerateInPctOfPath > 1f)
                {
                    if (prevPctOfPathPassed > curPctOfPathPassed)
                    {
                        Slowdown(remainingArrivalTicks, decelerateInPctOfPath);
                    }
                    else
                    {
                        if (curPctOfPathPassed <= 0.5f && curMovementMode != MovementMode.Decelerate)
                        {
                            SpeedUp(moveSpeed);
                        }
                        else
                        {
                            Slowdown(remainingArrivalTicks, decelerateInPctOfPath);
                        }
                    }
                }
                else
                {
                    if (curMovementMode != MovementMode.Decelerate && ShouldSpeedUp(remainingArrivalTicks))
                    {
                        SpeedUp(moveSpeed);
                    }
                    else
                    {
                        Slowdown(remainingArrivalTicks, decelerateInPctOfPath);
                    }
                }
                prevPctOfPathPassed = curPctOfPathPassed;
                if (isScreeching && screechingSustainer != null && !screechingSustainer.Ended)
                {
                    screechingSustainer.Maintain();
                }
            }
            else
            {
                ResetValues();
            }
        }

        private bool ShouldSpeedUp(float remainingArrivalTicks)
        {
            var tempSpeed = currentSpeed - MaxSpeedToDecelerateSmoothly;
            for (var i = 0; i < remainingArrivalTicks; i++)
            {
                float decelerationRate = AccelerationRate * DecelerationMultiplier;
                var newSpeed = tempSpeed - decelerationRate;
                if (tempSpeed > newSpeed)
                {
                    tempSpeed = newSpeed;
                }
                var slowdownMultiplier = tempSpeed / (AccelerationRate * DecelerationMultiplier) / (remainingArrivalTicks - i);
                if (tempSpeed <= 0)
                {
                    if (slowdownMultiplier >= 1.5f)
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        private void ResetValues()
        {
            if (wasMoving is false)
            {
                prevPctOfPathPassed = 0;
                currentSpeed = 0;
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

        public void Slowdown(float remainingArrivalTicks, float deceleratePct, bool stopImmediately = false)
        {
            remainingArrivalTicks += 30;
            float decelerationRate = AccelerationRate * DecelerationMultiplier;
            float newSpeed = currentSpeed - decelerationRate;
            float slowdownMultiplier = currentSpeed / (AccelerationRate * DecelerationMultiplier) / remainingArrivalTicks;
            bool shouldApplyHandbrake = handbrakeApplied is false && (stopImmediately
                            || (slowdownMultiplier >= 2f && currentSpeed > MaxSpeedToDecelerateSmoothly
                            || deceleratePct > 1f && Vehicle.vehiclePather.curPath.NodesConsumedCount < 2));


            if (shouldApplyHandbrake)
            {   
                newSpeed /= slowdownMultiplier;
                isScreeching = true;
                screechingSustainer = VVE_DefOf.VVE_TiresScreech.TrySpawnSustainer(SoundInfo.InMap(parent, MaintenanceType.PerTick));
                handbrakeApplied = true;
                if (VanillaVehiclesExpandedSettings.handbrakeDealsDamage)
                {
                    Messages.Message("VVE_HandbrakeWarning".Translate(Vehicle.Named("VEHICLE")), MessageTypeDefOf.NegativeHealthEvent);
                    float damageAmount = currentSpeed - (AccelerationRate * 10);
                    var components = Vehicle.statHandler.components.Where(x => x.props.tags != null && x.props.tags.Contains("Wheel"));
                    foreach (var component in components)
                    {
                        component.TakeDamage(Vehicle, new DamageInfo(DamageDefOf.Scratch, damageAmount), ignoreArmor: true);
                    }
                }
            }

            newSpeed = Mathf.Max(MaxSpeedToDecelerateSmoothly, newSpeed);
            if (currentSpeed > newSpeed)
            {
                currentSpeed = newSpeed;
            }
            if (newSpeed <= MaxSpeedToDecelerateSmoothly)
			{
                handbrakeApplied = false;
                //Vehicle.vPather.PatherFailed(); //Stop vehicle upon reaching max move ticks
			}
            curMovementMode = MovementMode.Decelerate;
            //Log.Message("currentSpeed: " + currentSpeed + " - curMovementMode: " + curMovementMode 
            //    + " - decelerationRate: " + decelerationRate + " - decelerationRateNeeded: " + decelerationRateNeeded
            //    + " - remainingArrivalTicks: " + remainingArrivalTicks + " - curPctOfPathPassed: " + curPctOfPathPassed
            //    + " - deceleratePct: " + deceleratePct);
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
            var path = Vehicle.vehiclePather.curPath;
            var result = GetPathCost(path, ignorePassedCells);
            result.cost += Vehicle.vehiclePather.nextCellCostTotal - Vehicle.vehiclePather.nextCellCostLeft;
            var otherResult = GetPathCostsFromQueuedJobs(path.LastNode);
            result.cost += otherResult.cost;
            result.cells += otherResult.cells;
            if (ignorePassedCells)
            {
                //var tempSpeed = this.currentSpeed;
                //var adjustedValue = 0f;
                //
                //float moveSpeed = StatMoveSpeed;
                //float totalCost = result.cost;
                //float decelerateInPctOfPath = moveSpeed / (AccelerationRate * DecelerationMultiplier) / totalCost;
                //curPctOfPathPassed = curPaidPathCost / totalCost;
                //
                //for (var i = 0; i < result.cells; i++)
                //{
                //    for (var j = 0; j < 60; j++)
                //    {
                //        if (curPctOfPathPassed <= 1f - decelerateInPctOfPath && curMovementMode != MovementMode.Decelerate)
                //        {
                //            if (moveSpeed > tempSpeed)
                //            {
                //                tempSpeed = Mathf.Min(tempSpeed + AccelerationRate, moveSpeed);
                //            }
                //        }
                //        else
                //        {
                //            float decelerationRate = AccelerationRate * DecelerationMultiplier;
                //            float decelerationRateNeeded = (MaxSpeedToDecelerateSmoothly + tempSpeed) / totalCost;
                //            decelerationRate = Mathf.Min(decelerationRate, decelerationRateNeeded);
                //            var newSpeed = tempSpeed - decelerationRate;
                //            newSpeed = Mathf.Max(MaxSpeedToDecelerateSmoothly, newSpeed);
                //            if (tempSpeed > newSpeed)
                //            {
                //                tempSpeed = newSpeed;
                //            }
                //        }
                //        var costToPayThisTick = (result.cost / result.cells) / 60f;
                //        costToPayThisTick *= tempSpeed / moveSpeed;
                //        adjustedValue += costToPayThisTick;
                //    }
                //}
                //Log.Message("result: " + result.cost + " - adjustedValue: " + adjustedValue);
                //result.cost = adjustedValue;
            }
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
                        var otherResult = GetPathCost(path, ignorePassedCells: false);
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
                        result.cost += Vehicle_PathFollower.CostToMoveIntoCell(Vehicle, prevCell, cell);
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

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);

            Vehicle.AddEvent(VehicleEventDefOf.MoveStart, StartMove); //Starts ticking from this event
            Vehicle.AddEvent(VehicleEventDefOf.Braking, TrySlowdown); //Hooks onto event trigger right before PathFollower is notified to stop
        }

        private void TrySlowdown()
		{
            if (Vehicle.vehiclePather.Moving && Vehicle.vehiclePather.curPath != null)
            {
                float totalCost = GetPathCost(false).cost;
                float decelerateMultiplier = 4f;
                float decelerateInPctOfPath = StatMoveSpeed / (AccelerationRate * decelerateMultiplier) / totalCost;
                Slowdown(GetPathCost(true).cost, decelerateInPctOfPath, stopImmediately: true);
            }
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
    }
}
