using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VanillaVehiclesExpanded
{
    public class GarageAutoDoor : GarageDoor
    {
        protected override IEnumerable<Gizmo> GetDoorGizmos()
        {
            if (!opened)
            {
                var openButton = new Command_Action
                {
                    defaultLabel = "VVE_Open".Translate(),
                    defaultDesc = "VVE_OpenDescription_AutoDoor".Translate(),
                    icon = ContentFinder<Texture2D>.Get("Things/Building/Structure/GarageDoor_Open"),
                    action = delegate
                    {
                        this.StartOpening();
                    }
                };
                if (this.compPower.PowerOn is false)
                {
                    openButton.Disable("NoPower".Translate());
                }
                yield return openButton;
            }
            else if (opened)
            {
                var closeButton = new Command_Action
                {
                    defaultLabel = "VVE_Close".Translate(),
                    defaultDesc = "VVE_CloseDescription_AutoDoor".Translate(),
                    icon = ContentFinder<Texture2D>.Get("Things/Building/Structure/GarageDoor_Close"),
                    action = delegate
                    {
                        this.StartClosing();
                    }
                }; 
                if (this.compPower.PowerOn is false)
                {
                    closeButton.Disable("NoPower".Translate());
                }
                yield return closeButton;
                CheckVehicleObstructingClosingGarage(closeButton);
            }
        }
    }
}
