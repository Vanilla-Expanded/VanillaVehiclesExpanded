using HarmonyLib;
using RimWorld;
using RimWorld.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.Sound;

namespace VanillaVehiclesExpanded
{
  [HarmonyPatch(typeof(Verb), "EquipmentSource", MethodType.Getter)]
  public static class Verb_EquipmentSource_Patch
  {
    public static void Postfix(ref ThingWithComps __result, Verb __instance)
    {
      if (__instance.DirectOwner is CompReloadableWithVerbs comp)
      {
        __result = comp.parent;
      }
    }
  }

  [HarmonyPatch(typeof(ReloadableUtility), "OwnerOf")]
  public static class ReloadableUtility_OwnerOf_Patch
  {
    public static void Postfix(IReloadableComp reloadable, ref Pawn __result)
    {
      if (reloadable is CompReloadableWithVerbs withVerbs)
      {
        __result = withVerbs.Vehicle;
      }
    }
  }

  public class CompReloadableWithVerbs : ThingComp, IVerbOwner, IReloadableComp, ICompWithCharges
  {
    public CompProperties_ReloadableWithVerbs Props => props as CompProperties_ReloadableWithVerbs;

    private VerbTracker verbTracker;

    protected int remainingCharges;

    public int RemainingCharges => remainingCharges;

    public int MaxCharges => Props.maxCharges;

    public string LabelRemaining => $"{RemainingCharges} / {MaxCharges}";

    public List<VerbProperties> VerbProperties => Props.Verbs;

    public List<Tool> Tools => Props.tools;

    public ImplementOwnerTypeDef ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.NativeVerb;

    public Thing ConstantCaster => Vehicle;

    public virtual string GizmoExtraLabel => null;

    public VerbTracker VerbTracker
    {
      get
      {
        if (verbTracker == null)
        {
          verbTracker = new VerbTracker(this);
        }
        return verbTracker;
      }
    }

    public override void PostPostMake()
    {
      base.PostPostMake();
      remainingCharges = MaxCharges;
    }

    public override string CompInspectStringExtra()
    {
      return "ChargesRemaining".Translate(Props.ChargeNounArgument) + ": " + LabelRemaining;
    }

    public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
    {
      IEnumerable<StatDrawEntry> enumerable = base.SpecialDisplayStats();
      if (enumerable != null)
      {
        foreach (StatDrawEntry item in enumerable)
        {
          yield return item;
        }
      }
      yield return new StatDrawEntry(VehicleStatCategoryDefOf.VehicleBasics,
        "Stat_Thing_ReloadChargesRemaining_Name".Translate(Props.ChargeNounArgument),
        LabelRemaining,
        "Stat_Thing_ReloadChargesRemaining_Desc".Translate(Props.ChargeNounArgument), 2749);
    }

    public void UsedOnce()
    {
      if (remainingCharges > 0)
      {
        remainingCharges--;
      }
      if (Props.destroyOnEmpty && remainingCharges == 0 && !parent.Destroyed)
      {
        parent.Destroy();
      }
      if (Props.replenishAfterCooldown && remainingCharges == 0)
      {
        replenishInTicks = Props.baseReloadTicks;
      }
    }

    public List<Verb> AllVerbs => VerbTracker.AllVerbs;

    public string UniqueVerbOwnerID()
    {
      var comps = this.parent.AllComps.OfType<CompReloadableWithVerbs>().ToList();
      return "VehicleComp_Reloadable_" + comps.IndexOf(this) + "_" + this.parent.ThingID;
    }

    public bool VerbsStillUsableBy(Pawn p)
    {
      return Vehicle == p;
    }


    public override void Initialize(CompProperties props)
    {
      base.Initialize(props);
      SetupVerbs();
    }

    private void SetupVerbs()
    {
      if (AllVerbs.Count <= 0)
      {
        VerbTracker.InitVerbsFromZero();
      }
      for (int i = 0; i < AllVerbs.Count; i++)
      {
        AllVerbs[i].caster = Vehicle;
      }
    }

    public VehiclePawn Vehicle => parent as VehiclePawn;

    public override void PostExposeData()
    {
      Scribe_Values.Look(ref remainingCharges, UniqueVerbOwnerID() + "_remainingCharges", -999);
      Scribe_Values.Look(ref replenishInTicks, UniqueVerbOwnerID() + "_replenishInTicks", -1);
      Scribe_Deep.Look(ref verbTracker, UniqueVerbOwnerID() + "_verbTracker", this);
      if (Scribe.mode == LoadSaveMode.PostLoadInit && remainingCharges == -999)
      {
        remainingCharges = MaxCharges;
      }
      if (Scribe.mode == LoadSaveMode.PostLoadInit)
      {
        SetupVerbs();
      }
    }

    public void TryReload(Verb verb)
    {
      if (NeedsReload(allowForcedReload: true))
      {
        if (Vehicle.CompFueledTravel.Props.fuelType == Props.ammoDef)
        {
          if (Props.ammoCountToRefill != 0)
          {
            var toFill = MaxCharges - RemainingCharges;
            var pct = (float)toFill / (float)MaxCharges;
            var fuelNeeded = Props.ammoCountToRefill * pct;
            var fuelToConsume = Mathf.Min(this.Vehicle.CompFueledTravel.Fuel, fuelNeeded);
            var newCharge = (int)(toFill * (fuelToConsume / fuelNeeded));
            if (newCharge <= 0)
            {
              Messages.Message(
                "VVE_CannotReloadNotEnoughAmmo".Translate(verb.verbProps.label,
                  Props.ammoDef.label), MessageTypeDefOf.RejectInput);
            }
            else
            {
              remainingCharges += newCharge;
              Vehicle.CompFueledTravel.ConsumeFuel(fuelToConsume);
              if (Props.soundReload != null)
              {
                Props.soundReload.PlayOneShot(new TargetInfo(Vehicle.Position, Vehicle.Map));
              }
            }
          }
        }
      }
      else
      {
        Messages.Message(
          "VVE_CannotReloadFullAmmo".Translate(verb.verbProps.label, Props.ammoDef.label),
          MessageTypeDefOf.RejectInput);
      }
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
      bool drafted = Vehicle.Drafted;
      if ((drafted && !Props.displayGizmoWhileDrafted) ||
        (!drafted && !Props.displayGizmoWhileUndrafted))
      {
        yield break;
      }
      ThingWithComps gear = parent;
      foreach (Verb allVerb in VerbTracker.AllVerbs)
      {
        if (allVerb.verbProps.hasStandardCommand)
        {
          yield return CreateVerbTargetCommandOverride(gear, allVerb);
        }
      }
      if (DebugSettings.ShowDevGizmos)
      {
        Command_Action command_Action = new Command_Action();
        command_Action.defaultLabel = "DEV: Reload to full";
        command_Action.action = delegate { remainingCharges = MaxCharges; };
        yield return command_Action;
      }
    }

    private Command_ReloadableWithVerbs CreateVerbTargetCommandOverride(Thing gear, Verb verb)
    {
      Command_ReloadableWithVerbs command_Reloadable = new Command_ReloadableWithVerbs(this);
      if (this.Props.commandDescriptions != null)
      {
        command_Reloadable.defaultDesc = this.Props.commandDescriptions[AllVerbs.IndexOf(verb)];
      }
      else
      {
        command_Reloadable.defaultDesc = gear.def.description;
      }
      command_Reloadable.hotKey = Props.hotKey;
      command_Reloadable.defaultLabel = verb.verbProps.label;
      command_Reloadable.verb = verb;
      if (verb.verbProps.defaultProjectile != null && verb.verbProps.commandIcon == null)
      {
        command_Reloadable.icon = verb.verbProps.defaultProjectile.uiIcon;
        command_Reloadable.iconAngle = verb.verbProps.defaultProjectile.uiIconAngle;
        command_Reloadable.iconOffset = verb.verbProps.defaultProjectile.uiIconOffset;
        command_Reloadable.overrideColor = verb.verbProps.defaultProjectile.graphicData.color;
      }
      else
      {
        command_Reloadable.icon =
          ((verb.UIIcon != BaseContent.BadTex) ? verb.UIIcon : gear.def.uiIcon);
        command_Reloadable.iconAngle = gear.def.uiIconAngle;
        command_Reloadable.iconOffset = gear.def.uiIconOffset;
      }
      if (Vehicle.Faction != Faction.OfPlayer)
      {
        command_Reloadable.Disable("CannotOrderNonControlled".Translate());
      }
      else if (verb.verbProps.violent && Vehicle.WorkTagIsDisabled(WorkTags.Violent))
      {
        command_Reloadable.Disable("IsIncapableOfViolenceLower"
         .Translate(Vehicle.LabelShort, Vehicle).CapitalizeFirst() + ".");
      }
      else if (!CanBeUsed(out var reason))
      {
        command_Reloadable.Disable(reason);
      }
      return command_Reloadable;
    }

    private int replenishInTicks = -1;

    public Thing ReloadableThing => parent;

    public ThingDef AmmoDef => Props.ammoDef;

    public int BaseReloadTicks => Props.baseReloadTicks;

    public override void CompTick()
    {
      base.CompTick();
      if (Props.replenishAfterCooldown && this.RemainingCharges == 0)
      {
        if (replenishInTicks > 0)
        {
          replenishInTicks--;
        }
        else
        {
          remainingCharges = this.MaxCharges;
        }
      }
    }

    public string DisabledReason(int minNeeded, int maxNeeded)
    {
      if (Props.replenishAfterCooldown)
      {
        return "CommandReload_Cooldown".Translate(Props.CooldownVerbArgument,
          replenishInTicks.ToStringTicksToPeriod().Named("TIME"));
      }
      if (AmmoDef == null)
      {
        return "CommandReload_NoCharges".Translate(Props.ChargeNounArgument);
      }
      return TranslatorFormattedStringExtensions.Translate(
        arg3: ((Props.ammoCountToRefill == 0) ?
          ((minNeeded == maxNeeded) ? minNeeded.ToString() : $"{minNeeded}-{maxNeeded}") :
          Props.ammoCountToRefill.ToString()).Named("COUNT"), key: "CommandReload_NoAmmo",
        arg1: Props.ChargeNounArgument, arg2: NamedArgumentUtility.Named(AmmoDef, "AMMO"));
    }

    public bool NeedsReload(bool allowForcedReload)
    {
      if (AmmoDef == null)
      {
        return false;
      }
      if (Props.ammoCountToRefill != 0)
      {
        if (!allowForcedReload)
        {
          return remainingCharges == 0;
        }
        return this.RemainingCharges != this.MaxCharges;
      }
      return this.RemainingCharges != this.MaxCharges;
    }

    public void ReloadFrom(Thing ammo)
    {
      if (!NeedsReload(allowForcedReload: true))
      {
        return;
      }
      if (Props.ammoCountToRefill != 0)
      {
        if (ammo.stackCount < Props.ammoCountToRefill)
        {
          return;
        }
        ammo.SplitOff(Props.ammoCountToRefill).Destroy();
        remainingCharges = this.MaxCharges;
      }
      else
      {
        if (ammo.stackCount < Props.ammoCountPerCharge)
        {
          return;
        }
        int num = Mathf.Clamp(ammo.stackCount / Props.ammoCountPerCharge, 0,
          this.MaxCharges - this.RemainingCharges);
        ammo.SplitOff(num * Props.ammoCountPerCharge).Destroy();
        remainingCharges += num;
      }
      if (Props.soundReload != null)
      {
        Props.soundReload.PlayOneShot(new TargetInfo(this.Vehicle.Position, this.Vehicle.Map));
      }
    }

    public int MinAmmoNeeded(bool allowForcedReload)
    {
      if (!NeedsReload(allowForcedReload))
      {
        return 0;
      }
      if (Props.ammoCountToRefill != 0)
      {
        return Props.ammoCountToRefill;
      }
      return Props.ammoCountPerCharge;
    }

    public int MaxAmmoNeeded(bool allowForcedReload)
    {
      if (!NeedsReload(allowForcedReload))
      {
        return 0;
      }
      if (Props.ammoCountToRefill != 0)
      {
        return Props.ammoCountToRefill;
      }
      return Props.ammoCountPerCharge * (this.MaxCharges - this.RemainingCharges);
    }

    public int MaxAmmoAmount()
    {
      if (AmmoDef == null)
      {
        return 0;
      }
      if (Props.ammoCountToRefill == 0)
      {
        return Props.ammoCountPerCharge * this.MaxCharges;
      }
      return Props.ammoCountToRefill;
    }

    public bool CanBeUsed(out string reason)
    {
      reason = "";
      if (remainingCharges <= 0)
      {
        reason = DisabledReason(MinAmmoNeeded(allowForcedReload: false),
          MaxAmmoNeeded(allowForcedReload: false));
        return false;
      }
      if (parent.MapHeld == null)
      {
        return false;
      }
      if (parent.MapHeld.IsPocketMap &&
        VerbProperties.Any((VerbProperties vp) => !vp.useableInPocketMaps))
      {
        reason = "CannotUseReason_PocketMap".Translate(parent.MapHeld.generatorDef.label);
        return false;
      }
      return true;
    }
  }
}