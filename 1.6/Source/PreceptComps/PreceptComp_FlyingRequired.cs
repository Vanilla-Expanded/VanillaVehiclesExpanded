
using RimWorld;
using System.Collections.Generic;
using Verse;
namespace VanillaVehiclesExpanded
{
    public class PreceptComp_FlyingRequired : PreceptComp
    {
        public HistoryEventDef eventDef;

       

        public override void Notify_HistoryEvent(HistoryEvent ev, Precept precept)
        {
            if (ev.def != eventDef)
            {
                return;
            }
          
            ev.args.TryGetArg(HistoryEventArgsNames.Doer, out Pawn pawn);
           
            if (pawn != null)
            {
                if (pawn.ideo?.Ideo?.HasPrecept(InternalDefOf.VVE_Flying_Required) == true)
                {
                    StaticCollectionsClass.AddOrSetColonistToAirVehicleList(pawn, 0);
                 
                }
            }
        }

        
    }
}