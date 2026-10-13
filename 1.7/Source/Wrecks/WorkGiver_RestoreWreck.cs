using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace VanillaVehiclesExpanded
{
    public class WorkGiver_RestoreWreck : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            foreach (var comp in CompVehicleWreck.compVehicleWrecks.Where(x => x?.parent?.Map == pawn.Map))
            {
                yield return comp.parent;
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!pawn.CanReserveAndReach(t, PathEndMode, MaxPathDanger(pawn), 1, -1, null, forced))
            {
                return false;
            }
            if (t.Map.designationManager.DesignationOn(t, VVE_DefOf.VVE_RestoreWreck) is null)
            {
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(VVE_DefOf.VVE_RestoreWreckJob, t);
        }
    }
}
