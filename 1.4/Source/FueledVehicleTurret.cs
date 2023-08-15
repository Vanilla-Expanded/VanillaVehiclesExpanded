using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.Sound;

namespace VanillaVehiclesExpanded
{
    public class FueledVehicleTurret : VehicleTurret
    {
        public FueledVehicleTurret()
        {

        }

        public FueledVehicleTurret(VehiclePawn vehicle)
            : base(vehicle)
        {
        }

        public FueledVehicleTurret(VehiclePawn vehicle, FueledVehicleTurret reference)
            : base(vehicle, reference)
        {
        }

        public override void ReloadCannon(ThingDef ammo = null, bool ignoreTimer = false)
        {
            if (loadedAmmo == null || shellCount < turretDef.magazineCapacity)
            {
                var newAmmo = (int)(this.vehicle.CompFueledTravel.Fuel * (float)turretDef.chargePerAmmoCount);
                newAmmo = Mathf.Min(turretDef.magazineCapacity - this.shellCount, newAmmo);
                ReloadInternal(newAmmo);
            }
            else if (shellCount > 0)
            {
                ActivateBurstTimer();
                return;
            }
            ActivateTimer(ignoreTimer);
        }

        public override bool AutoReloadCannon()
        {
            var newAmmo = (int)(this.vehicle.CompFueledTravel.Fuel * (float)turretDef.chargePerAmmoCount);
            newAmmo = Mathf.Min(turretDef.magazineCapacity - this.shellCount, newAmmo);
            return ReloadInternal(newAmmo);
        }

        private bool ReloadInternal(int newAmmo)
        {
            if (newAmmo <= 0)
            {
                return false;
            }
            var fuelToConsume = newAmmo / (float)turretDef.chargePerAmmoCount;
            this.vehicle.CompFueledTravel.ConsumeFuel(fuelToConsume);
            this.loadedAmmo = this.vehicle.CompFueledTravel.Props.fuelType;
            this.shellCount += newAmmo;
            //Log.Message("this.vehicle.CompFueledTravel.Fuel: " + this.vehicle.CompFueledTravel.Fuel 
            //    + " - fuelToConsume: " + fuelToConsume + " - newAmmo: " + newAmmo + " - " + this.shellCount);
            EventRegistry[VehicleTurretEventDefOf.Reload].ExecuteEvents(); 
            if (turretDef.reloadSound != null)
            {
                turretDef.reloadSound.PlayOneShot(new TargetInfo(vehicle.Position, vehicle.Map, false));
            }
            return true;
        }

        public override void TryRemoveShell()
        {
            if (shellCount > 0)
            {
                this.vehicle.CompFueledTravel.Refuel(shellCount / (float)turretDef.chargePerAmmoCount);
                shellCount = 0;
                this.loadedAmmo = null;
                ActivateTimer(true);
            }
        }

        public override IEnumerable<SubGizmo> SubGizmos
        {
            get
            {
                yield return SubGizmo_RemoveAmmo(this);
                yield return SubGizmo_ReloadFromFuel(this);
                yield return SubGizmo_FireMode(this);
                if (autoTargeting)
                {
                    yield return SubGizmo_AutoTarget(this);
                }
            }
        }

        public static SubGizmo SubGizmo_ReloadFromFuel(VehicleTurret turret)
        {
            return new SubGizmo
            {
                drawGizmo = delegate (Rect rect)
                {
                    Widgets.DrawTextureFitted(rect, VehicleTex.ReloadIcon, 1);
                },
                canClick = delegate ()
                {
                    return turret.vehicle.CompFueledTravel.Fuel >= 1f / (float)turret.turretDef.chargePerAmmoCount;
                },
                onClick = delegate ()
                {
                    turret.ReloadCannon(null, true);
                },
                tooltip = "VF_ReloadVehicleTurret".Translate()
            };
        }
    }
}
