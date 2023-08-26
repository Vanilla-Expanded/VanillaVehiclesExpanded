
using RimWorld;
using System;
using Verse;
namespace VanillaVehiclesExpanded
{
    public class ThoughtWorker_NeedRoomSize_Horrible : ThoughtWorker
    {
        public override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.needs.roomsize == null)
            {
                return ThoughtState.Inactive;
            }
            Room room = p.GetRoom();
            if (room == null || room.PsychologicallyOutdoors)
            {
                return ThoughtState.Inactive;
            }
            RoomSizeCategory curCategory = p.needs.roomsize.CurCategory;
            if (p.Ideo != null && (int)curCategory < 2 && p.Ideo.IdeoDisablesCrampedRoomThoughts())
            {
                return ThoughtState.Inactive;
            }

            if (p.Ideo?.HasPrecept(InternalDefOf.VVE_SmallSpaces_Horrible)!=true)
            {
                return ThoughtState.Inactive;
            }
            switch (curCategory)
            {
                case RoomSizeCategory.VeryCramped:
                    return ThoughtState.ActiveAtStage(0);
                case RoomSizeCategory.Cramped:
                    return ThoughtState.ActiveAtStage(1);
                case RoomSizeCategory.Normal:
                    return ThoughtState.Inactive;
                case RoomSizeCategory.Spacious:
                    return ThoughtState.ActiveAtStage(2);
                default:
                    throw new InvalidOperationException("Unknown RoomSizeCategory");
            }
        }
    }
}
