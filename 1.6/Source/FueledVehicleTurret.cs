﻿using HarmonyLib;
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

    public override void Reload(ThingDef ammo = null, bool ignoreTimer = false)
    {
      if (loadedAmmo == null || shellCount < def.magazineCapacity)
      {
        var newAmmo =
          (int)(this.vehicle.CompFueledTravel.Fuel / (float)def.chargePerAmmoCount);
        newAmmo = Mathf.Min(def.magazineCapacity - this.shellCount, newAmmo);
        ReloadInternal(newAmmo);
      }
      else if (shellCount > 0)
      {
        ActivateBurstTimer();
        return;
      }
      ActivateTimer(ignoreTimer);
    }

    public override bool AutoReload()
    {
      var newAmmo = (int)(this.vehicle.CompFueledTravel.Fuel / (float)def.chargePerAmmoCount);
      newAmmo = Mathf.Min(def.magazineCapacity - this.shellCount, newAmmo);
      return ReloadInternal(newAmmo);
    }

    private bool ReloadInternal(int newAmmo)
    {
      if (newAmmo <= 0)
      {
        return false;
      }
      var fuelToConsume = newAmmo * (float)def.chargePerAmmoCount;
      this.vehicle.CompFueledTravel.ConsumeFuel(fuelToConsume);
      this.loadedAmmo = this.vehicle.CompFueledTravel.Props.fuelType;
      this.shellCount += newAmmo;
      EventRegistry[VehicleTurretEventDefOf.Reload].ExecuteEvents();
      if (def.reloadSound != null)
      {
        def.reloadSound.PlayOneShot(new TargetInfo(vehicle.Position, vehicle.Map, false));
      }
      return true;
    }

    public override void TryClearChamber()
    {
      if (shellCount > 0)
      {
        this.vehicle.CompFueledTravel.Refuel(shellCount * (float)def.chargePerAmmoCount);
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
      (
        drawGizmo: delegate(Rect rect)
        {
          //Widgets.DrawTextureFitted(rect, BGTex, 1);
          if (turret.loadedAmmo != null)
          {
            using (new TextBlock(GUI.color with { a = turret.IconAlphaTicked }))
            {
              Widgets.DrawTextureFitted(rect, turret.loadedAmmo.uiIcon, 1);
            }
            Rect ammoCountRect = new Rect(rect);
            string ammoCount = turret.vehicle.CompFueledTravel.Fuel.ToString("F0");
            ammoCountRect.y += ammoCountRect.height / 2;
            ammoCountRect.x += ammoCountRect.width - Text.CalcSize(ammoCount).x;
            Widgets.Label(ammoCountRect, ammoCount);
          }
          else if (turret.def.genericAmmo && turret.def.ammunition.AllowedDefCount > 0)
          {
            Widgets.DrawTextureFitted(rect,
              turret.def.ammunition.AllowedThingDefs.FirstOrDefault().uiIcon, 1);

            Rect ammoCountRect = new Rect(rect);
            string ammoCount = turret.vehicle.CompFueledTravel.Fuel.ToString("F0");
            ammoCountRect.y += ammoCountRect.height / 2;
            ammoCountRect.x += ammoCountRect.width - Text.CalcSize(ammoCount).x;
            Widgets.Label(ammoCountRect, ammoCount);
          }
        },
        canClick: delegate() { return turret.shellCount > 0; },
        onClick: delegate()
        {
          turret.TryClearChamber();
          SoundDefOf.Artillery_ShellLoaded.PlayOneShot(new TargetInfo(turret.vehicle.Position,
            turret.vehicle.Map, false));
        },
        tooltip: turret.loadedAmmo?.LabelCap
      );
    }

    public static SubGizmo SubGizmo_ReloadFromFuel(VehicleTurret turret)
    {
      return new SubGizmo
      (
        drawGizmo: delegate(Rect rect)
        {
          Widgets.DrawTextureFitted(rect, VehicleTex.ReloadIcon, 1);
        },
        canClick: delegate()
        {
          return turret.vehicle.CompFueledTravel.Fuel >=
            1f / (float)turret.def.chargePerAmmoCount;
        },
        onClick: delegate() { turret.Reload(null, true); },
        tooltip: "VF_ReloadVehicleTurret".Translate()
      );
    }
  }
}