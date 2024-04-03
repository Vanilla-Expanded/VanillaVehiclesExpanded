using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace VanillaVehiclesExpanded
{
    public abstract class WorkGiver_ManageGarage : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            foreach (var garageDoor in GarageDoor.garageDoors.Where(x => x?.Map == pawn.Map))
            {
                yield return garageDoor;
            }
        }
    }

    public class WorkGiver_OpenGarage : WorkGiver_ManageGarage
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!pawn.CanReserveAndReach(t, PathEndMode, MaxPathDanger(pawn), 1, -1, null, forced))
            {
                return false;
            }
            if (t.Map.designationManager.DesignationOn(t, VVE_DefOf.VVE_Open) is null)
            {
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(VVE_DefOf.VVE_OpenGarage, t);
        }
    }

    public class WorkGiver_CloseGarage : WorkGiver_ManageGarage
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }
            var garageDoor = t as GarageDoor;
            if (garageDoor.Map.designationManager.DesignationOn(garageDoor, VVE_DefOf.VVE_Close) is null)
            {
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(VVE_DefOf.VVE_CloseGarage, t);
        }
    }

}
