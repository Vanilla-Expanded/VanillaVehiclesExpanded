using HarmonyLib;
using RimWorld;
using SmashTools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace VanillaVehiclesExpanded
{
    public class CompProperties_ReloadableWithVerbs : CompProperties_Reloadable
    {
        public List<Tool> tools;
        private List<VerbProperties> verbs;
        public List<string> commandDescriptions;
        private static List<VerbProperties> EmptyVerbPropertiesList = new List<VerbProperties>();
        public List<VerbProperties> Verbs
        {
            get
            {
                if (verbs != null)
                {
                    return verbs;
                }
                return EmptyVerbPropertiesList;
            }
        }

        public CompProperties_ReloadableWithVerbs()
        {
            compClass = typeof(CompReloadableWithVerbs);
        }
    }

    public class CompReloadableWithVerbs : CompReloadable, IVerbOwner
    {
        public new CompProperties_ReloadableWithVerbs Props => props as CompProperties_ReloadableWithVerbs;
        
        [HarmonyPatch(typeof(ReloadableUtility), "WearerOf")]
        public static class ReloadableUtility_WearerOf_Patch
        {
            public static void Postfix(ThingComp comp, ref Pawn __result)
            {
                if (comp is CompReloadableWithVerbs withVerbs)
                {
                    __result = withVerbs.Vehicle;
                }
            }
        }

        public VehiclePawn Vehicle => parent as VehiclePawn;

        [HarmonyPatch(typeof(CompReloadable), "VerbProperties", MethodType.Getter)]
        public static class CompReloadable_VerbProperties_Patch
        {
            public static bool Prefix(CompReloadable __instance, ref List<VerbProperties> __result)
            {
                if (__instance is CompReloadableWithVerbs withVerbs)
                {
                    __result = withVerbs.Props.Verbs;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(CompReloadable), "Tools", MethodType.Getter)]
        public static class CompReloadable_Tools_Patch
        {
            public static bool Prefix(CompReloadable __instance, ref List<Tool> __result)
            {
                if (__instance is CompReloadableWithVerbs withVerbs)
                {
                    __result = withVerbs.Props.tools;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(CompReloadable), "UniqueVerbOwnerID")]
        public static class CompReloadable_UniqueVerbOwnerID_Patch
        {
            public static bool Prefix(CompReloadable __instance, ref string __result)
            {
                if (__instance is CompReloadableWithVerbs withVerbs)
                {
                    var comps = __instance.parent.comps.OfType<CompReloadableWithVerbs>().ToList();
                    __result = "VehicleComp_Reloadable_" + comps.IndexOf(withVerbs) + "_" + __instance.parent.ThingID;
                    return false;
                }
                return true;
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref remainingCharges, UniqueVerbOwnerID() + "_remainingCharges", -999);
            Scribe_Values.Look(ref replenishInTicks, UniqueVerbOwnerID() + "_replenishInTicks", -1);
            Scribe_Deep.Look(ref verbTracker, UniqueVerbOwnerID() + "_verbTracker", this);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && remainingCharges == -999)
            {
                remainingCharges = MaxCharges;
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
                            Messages.Message("VVE_CannotReloadNotEnoughAmmo".Translate(verb.verbProps.label, Props.ammoDef.label), MessageTypeDefOf.RejectInput);
                        }
                        else
                        {
                            remainingCharges += newCharge;
                            Vehicle.CompFueledTravel.ConsumeFuel(fuelToConsume);
                            if (Props.soundReload != null)
                            {
                                Props.soundReload.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map));
                            }
                        }
                    }
                }
            }
            else
            {
                Messages.Message("VVE_CannotReloadFullAmmo".Translate(verb.verbProps.label, Props.ammoDef.label), MessageTypeDefOf.RejectInput);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            bool drafted = Wearer.Drafted;
            if ((drafted && !Props.displayGizmoWhileDrafted) || (!drafted && !Props.displayGizmoWhileUndrafted))
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
                command_Action.action = delegate
                {
                    remainingCharges = MaxCharges;
                };
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
                command_Reloadable.icon = ((verb.UIIcon != BaseContent.BadTex) ? verb.UIIcon : gear.def.uiIcon);
                command_Reloadable.iconAngle = gear.def.uiIconAngle;
                command_Reloadable.iconOffset = gear.def.uiIconOffset;
            }
            if (Wearer.Faction != Faction.OfPlayer)
            {
                command_Reloadable.Disable("CannotOrderNonControlled".Translate());
            }
            else if (verb.verbProps.violent && Wearer.WorkTagIsDisabled(WorkTags.Violent))
            {
                command_Reloadable.Disable("IsIncapableOfViolenceLower".Translate(Wearer.LabelShort, Wearer).CapitalizeFirst() + ".");
            }
            else if (!CanBeUsed)
            {
                command_Reloadable.Disable(DisabledReason(MinAmmoNeeded(allowForcedReload: false), MaxAmmoNeeded(allowForcedReload: false)));
            }
            return command_Reloadable;
        }
    }

    [HotSwappable]
    public class Command_ReloadableWithVerbs : Command_VerbTarget
    {
        private readonly CompReloadableWithVerbs comp;

        public Color? overrideColor;

        public override string TopRightLabel => comp.LabelRemaining;

        public override Color IconDrawColor => overrideColor ?? defaultIconColor;

        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                var label = "VVE_Reload".Translate(verb.verbProps.label, comp.AmmoDef.Named("AMMO")) + " (" + comp.LabelRemaining + ")";
                yield return new FloatMenuOption(label, delegate
                {
                    comp.TryReload(verb);
                });
            }
        }

        public Command_ReloadableWithVerbs(CompReloadableWithVerbs comp)
        {
            this.comp = comp;
        }

        public override void GizmoUpdateOnMouseover()
        {
            verb.DrawHighlight(LocalTargetInfo.Invalid);
        }

        public override GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms)
        {
            Text.Font = GameFont.Tiny;
            Color color = Color.white;
            bool flag = false;
            if (Mouse.IsOver(butRect))
            {
                flag = true;
                if (!disabled)
                {
                    color = GenUI.MouseoverColor;
                }
            }
            MouseoverSounds.DoRegion(butRect, SoundDefOf.Mouseover_Command);
            if (parms.highLight)
            {
                Widgets.DrawStrongHighlight(butRect.ExpandedBy(12f));
            }
            Material material = ((disabled || parms.lowLight) ? TexUI.GrayscaleGUI : null);
            GUI.color = (parms.lowLight ? LowLightBgColor : color);
            GenUI.DrawTextureWithMaterial(butRect, parms.shrunk ? BGTextureShrunk : BGTexture, material);
            GUI.color = color;
            DrawIcon(butRect, material, parms);
            bool flag2 = false;
            GUI.color = Color.white;
            if (parms.lowLight)
            {
                GUI.color = LowLightLabelColor;
            }
            Vector2 vector = (parms.shrunk ? new Vector2(3f, 0f) : new Vector2(5f, 3f));
            Rect rect = new Rect(butRect.x + vector.x, butRect.y + vector.y, butRect.width - 10f, Text.LineHeight);
            if (SteamDeck.IsSteamDeckInNonKeyboardMode)
            {
                if (parms.isFirst)
                {
                    GUI.DrawTexture(new Rect(rect.x, rect.y, 21f, 21f), TexUI.SteamDeck_ButtonA);
                    if (KeyBindingDefOf.Accept.KeyDownEvent)
                    {
                        flag2 = true;
                        Event.current.Use();
                    }
                }
            }
            else
            {
                KeyCode keyCode = ((hotKey != null) ? hotKey.MainKey : KeyCode.None);
                if (keyCode != 0 && !GizmoGridDrawer.drawnHotKeys.Contains(keyCode))
                {
                    Widgets.Label(rect, keyCode.ToStringReadable());
                    GizmoGridDrawer.drawnHotKeys.Add(keyCode);
                    if (hotKey.KeyDownEvent)
                    {
                        flag2 = true;
                        Event.current.Use();
                    }
                }
            }
            if (GizmoGridDrawer.customActivator != null && GizmoGridDrawer.customActivator(this))
            {
                flag2 = true;
            }
            if (Widgets.ButtonInvisible(butRect))
            {
                flag2 = true;
            }
            if (!parms.shrunk)
            {
                string topRightLabel = TopRightLabel;
                if (!topRightLabel.NullOrEmpty())
                {
                    Vector2 vector2 = Text.CalcSize(topRightLabel);
                    Rect position;
                    Rect rect2 = (position = new Rect(butRect.xMax - vector2.x - 2f, butRect.y + 3f, vector2.x, vector2.y));
                    position.x -= 2f;
                    position.width += 3f;
                    Text.Anchor = TextAnchor.UpperRight;
                    GUI.DrawTexture(position, TexUI.GrayTextBG);
                    Widgets.Label(rect2, topRightLabel);
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                string labelCap = LabelCap;
                if (!labelCap.NullOrEmpty())
                {
                    float num = Text.CalcHeight(labelCap, butRect.width);
                    Rect rect3 = new Rect(butRect.x, butRect.yMax - num + 12f, butRect.width, num);
                    GUI.DrawTexture(rect3, TexUI.GrayTextBG);
                    Text.Anchor = TextAnchor.UpperCenter;
                    Widgets.Label(rect3, labelCap);
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                GUI.color = Color.white;
            }
            if (Mouse.IsOver(butRect) && DoTooltip)
            {
                TipSignal tip = Desc;
                if (disabled && !disabledReason.NullOrEmpty())
                {
                    tip.text += ("\n\n" + "DisabledCommand".Translate() + ": " + disabledReason).Colorize(ColorLibrary.RedReadable);
                }
                tip.text += DescPostfix;
                TooltipHandler.TipRegion(butRect, tip);
            }
            if (!HighlightTag.NullOrEmpty() && (Find.WindowStack.FloatMenu == null || !Find.WindowStack.FloatMenu.windowRect.Overlaps(butRect)))
            {
                UIHighlighter.HighlightOpportunity(butRect, HighlightTag);
            }
            Text.Font = GameFont.Small;
            if (flag2)
            {
                if (disabled && Event.current.button != 1)
                {
                    if (!disabledReason.NullOrEmpty())
                    {
                        Messages.Message(disabledReason, MessageTypeDefOf.RejectInput, historical: false);
                    }
                    return new GizmoResult(GizmoState.Mouseover, null);
                }
                GizmoResult result;
                if (Event.current.button == 1)
                {
                    result = new GizmoResult(GizmoState.OpenedFloatMenu, Event.current);
                }
                else
                {
                    if (!TutorSystem.AllowAction(TutorTagSelect))
                    {
                        return new GizmoResult(GizmoState.Mouseover, null);
                    }
                    result = new GizmoResult(GizmoState.Interacted, Event.current);
                    TutorSystem.Notify_Event(TutorTagSelect);
                }
                return result;
            }
            if (flag)
            {
                return new GizmoResult(GizmoState.Mouseover, null);
            }
            return new GizmoResult(GizmoState.Clear, null);
        }

        public override bool GroupsWith(Gizmo other)
        {
            return false;
        }
    }
}
