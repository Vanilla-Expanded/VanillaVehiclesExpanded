using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace VanillaVehiclesExpanded
{
    public abstract class JobDriver_GarageControlBase : JobDriver
    {
        public GarageDoor GarageDoor => job.targetA.Thing as GarageDoor;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => (FailCondition()));
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(15).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            Toil finalize = new Toil();
            finalize.initAction = delegate
            {
                DoWork();
            };
            finalize.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return finalize;
        }

        public abstract bool FailCondition();

        public abstract void DoWork();
    }

    public class JobDriver_OpenGarage : JobDriver_GarageControlBase
    {
        public override void DoWork()
        {
            GarageDoor.StartOpening();
        }

        public override bool FailCondition()
        {
            return base.Map.designationManager.DesignationOn(base.TargetThingA, VVE_DefOf.VVE_Open) is null || GarageDoor.compPower.PowerOn is false;
        }
    }

    public class JobDriver_CloseGarage : JobDriver_GarageControlBase
    {
        public override void DoWork()
        {
            GarageDoor.StartClosing();
        }

        public override bool FailCondition()
        {
            return base.Map.designationManager.DesignationOn(base.TargetThingA, VVE_DefOf.VVE_Close) is null || GarageDoor.compPower.PowerOn is false;
        }
    }
}
