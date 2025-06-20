using UnityEngine;
using Verse;

namespace VanillaVehiclesExpanded
{
    public class VanillaVehiclesExpandedMod : Mod
    {
        public static VanillaVehiclesExpandedSettings settings;
        public VanillaVehiclesExpandedMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<VanillaVehiclesExpandedSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return Content.Name;
        }
    }

    public class VanillaVehiclesExpandedSettings : ModSettings
    {
        public static bool handbrakeDealsDamage = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref handbrakeDealsDamage, "handbrakeEnabled", true);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            var ls = new Listing_Standard();
            ls.Begin(inRect);
            ls.CheckboxLabeled("VVE_HandbrakeDealsDamage".Translate(), ref handbrakeDealsDamage);
            ls.End();
        }
    }
}
