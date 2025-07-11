﻿using RimWorld;
using Verse;

namespace VanillaVehiclesExpanded
{
  public class ThoughtWorker_Sailing_Required : ThoughtWorker_Precept
  {
    public int gracePeriod = 1; //1 days
    public int firstPeriod = 5; //5 days
    public int secondPeriod = 10; //10 days
    public int thirdPeriod = 20; //20 days


    protected override ThoughtState ShouldHaveThought(Pawn p)
    {
      if (!StaticCollectionsClass.colonist_seavehicle_tracker.ContainsKey(p))
      {
        return false;
      }

      if (StaticCollectionsClass.colonist_seavehicle_tracker[p] < gracePeriod)
      {
        return false;
      }
      else if (StaticCollectionsClass.colonist_seavehicle_tracker[p] < firstPeriod)
      {
        return ThoughtState.ActiveAtStage(0);
      }
      else if (StaticCollectionsClass.colonist_seavehicle_tracker[p] < secondPeriod)
      {
        return ThoughtState.ActiveAtStage(1);
      }
      else if (StaticCollectionsClass.colonist_seavehicle_tracker[p] < thirdPeriod)
      {
        return ThoughtState.ActiveAtStage(2);
      }
      else if (StaticCollectionsClass.colonist_seavehicle_tracker[p] >= thirdPeriod)
      {
        return ThoughtState.ActiveAtStage(3);
      }

      return false;
    }
  }
}