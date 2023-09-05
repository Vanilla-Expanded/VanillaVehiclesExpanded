using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace VanillaVehiclesExpanded
{
    public class JobDriver_RestoreWreck : JobDriver
    {
        public CompVehicleWreck Comp => job.targetA.Thing.TryGetComp<CompVehicleWreck>();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => FailCondition());
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(300).WithProgressBarToilDelay(TargetIndex.A)
                .FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            var finalize = new Toil
            {
                initAction = delegate
                {
                    Comp.RestoreWreck(pawn);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return finalize;
        }

        public bool FailCondition()
        {
            return base.Map.designationManager.DesignationOn(base.TargetThingA, VVE_DefOf.VVE_RestoreWreck) is null;
        }
    }
}
