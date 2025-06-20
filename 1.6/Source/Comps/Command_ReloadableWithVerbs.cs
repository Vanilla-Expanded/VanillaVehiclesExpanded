using System.Collections.Generic;
using RimWorld;
using SmashTools;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace VanillaVehiclesExpanded
{
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
        var label = "VVE_Reload".Translate(verb.verbProps.label, comp.AmmoDef.Named("AMMO")) +
          " (" + comp.LabelRemaining + ")";
        yield return new FloatMenuOption(label, delegate { comp.TryReload(verb); });
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

    protected override GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms)
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
      Rect rect = new Rect(butRect.x + vector.x, butRect.y + vector.y, butRect.width - 10f,
        Text.LineHeight);
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
          Rect rect2 = (position = new Rect(butRect.xMax - vector2.x - 2f, butRect.y + 3f,
            vector2.x, vector2.y));
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
          tip.text +=
            ("\n\n" + "DisabledCommand".Translate() + ": " + disabledReason).Colorize(ColorLibrary
             .RedReadable);
        }
        tip.text += DescPostfix;
        TooltipHandler.TipRegion(butRect, tip);
      }
      if (!HighlightTag.NullOrEmpty() && (Find.WindowStack.FloatMenu == null ||
        !Find.WindowStack.FloatMenu.windowRect.Overlaps(butRect)))
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