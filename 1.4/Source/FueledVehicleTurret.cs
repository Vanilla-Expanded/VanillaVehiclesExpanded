using HarmonyLib;
using RimWorld;
using SmashTools;
using System.Collections.Generic;
using System.Linq;
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
                var newAmmo = (int)(this.vehicle.CompFueledTravel.Fuel / (float)turretDef.chargePerAmmoCount);
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
            var newAmmo = (int)(this.vehicle.CompFueledTravel.Fuel / (float)turretDef.chargePerAmmoCount);
            newAmmo = Mathf.Min(turretDef.magazineCapacity - this.shellCount, newAmmo);
            return ReloadInternal(newAmmo);
        }

        private bool ReloadInternal(int newAmmo)
        {
            if (newAmmo <= 0)
            {
                return false;
            }
            var fuelToConsume = newAmmo * (float)turretDef.chargePerAmmoCount;
            this.vehicle.CompFueledTravel.ConsumeFuel(fuelToConsume);
            this.loadedAmmo = this.vehicle.CompFueledTravel.Props.fuelType;
            this.shellCount += newAmmo;
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
                this.vehicle.CompFueledTravel.Refuel(shellCount * (float)turretDef.chargePerAmmoCount);
                shellCount = 0;
                this.loadedAmmo = null;
                ActivateTimer(true);
            }
        }


        public override IEnumerable<SubGizmo> SubGizmos
        {
            get
            {
                yield return SubGizmo_AmmoToFuel(this);
                yield return SubGizmo_ReloadFromFuel(this);
                yield return SubGizmo_FireMode(this);
                if (autoTargeting)
                {
                    yield return SubGizmo_AutoTarget(this);
                }
            }
        }

        public static SubGizmo SubGizmo_AmmoToFuel(VehicleTurret turret)
        {
            return new SubGizmo
            {
                drawGizmo = delegate (Rect rect)
                {
                    //Widgets.DrawTextureFitted(rect, BGTex, 1);
                    if (turret.loadedAmmo != null)
                    {
                        GUIState.Push();
                        {
                            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, turret.CannonIconAlphaTicked); //Only modify alpha
                            Widgets.DrawTextureFitted(rect, turret.loadedAmmo.uiIcon, 1);

                            GUIState.Reset();

                            Rect ammoCountRect = new Rect(rect);
                            string ammoCount = turret.vehicle.CompFueledTravel.Fuel.ToString("F0");
                            ammoCountRect.y += ammoCountRect.height / 2;
                            ammoCountRect.x += ammoCountRect.width - Text.CalcSize(ammoCount).x;
                            Widgets.Label(ammoCountRect, ammoCount);
                        }
                        GUIState.Pop();
                    }
                    else if (turret.turretDef.genericAmmo && turret.turretDef.ammunition.AllowedDefCount > 0)
                    {
                        Widgets.DrawTextureFitted(rect, turret.turretDef.ammunition.AllowedThingDefs.FirstOrDefault().uiIcon, 1);

                        Rect ammoCountRect = new Rect(rect);
                        string ammoCount = turret.vehicle.CompFueledTravel.Fuel.ToString("F0");
                        ammoCountRect.y += ammoCountRect.height / 2;
                        ammoCountRect.x += ammoCountRect.width - Text.CalcSize(ammoCount).x;
                        Widgets.Label(ammoCountRect, ammoCount);
                    }
                },
                canClick = delegate ()
                {
                    return turret.shellCount > 0;
                },
                onClick = delegate ()
                {
                    turret.TryRemoveShell();
                    SoundDefOf.Artillery_ShellLoaded.PlayOneShot(new TargetInfo(turret.vehicle.Position, turret.vehicle.Map, false));
                },
                tooltip = turret.loadedAmmo?.LabelCap
            };
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
