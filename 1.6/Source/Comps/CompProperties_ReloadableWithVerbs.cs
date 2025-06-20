using RimWorld;
using System.Collections.Generic;
using Vehicles;
using Verse;

namespace VanillaVehiclesExpanded
{
    public class CompProperties_ReloadableWithVerbs : CompProperties
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

        public int maxCharges = 1;

        public ThingDef ammoDef;

        public int ammoCountToRefill;

        public int ammoCountPerCharge;

        public bool destroyOnEmpty;

        public int baseReloadTicks = 60;

        public bool replenishAfterCooldown;

        public bool displayGizmoWhileUndrafted = true;

        public bool displayGizmoWhileDrafted = true;

        public KeyBindingDef hotKey;

        public SoundDef soundReload;

        [MustTranslate]
        public string chargeNoun = "charge";

        [MustTranslate]
        public string cooldownGerund = "on cooldown";

        public NamedArgument ChargeNounArgument => chargeNoun.Named("CHARGENOUN");

        public NamedArgument CooldownVerbArgument => cooldownGerund.CapitalizeFirst().Named("COOLDOWNGERUND");

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string item in base.ConfigErrors(parentDef))
            {
                yield return item;
            }
            if (ammoDef != null && ammoCountToRefill == 0 && ammoCountPerCharge == 0)
            {
                yield return "Reloadable component has ammoDef but one of ammoCountToRefill or ammoCountPerCharge must be set";
            }
            if (ammoCountToRefill != 0 && ammoCountPerCharge != 0)
            {
                yield return "Reloadable component: specify only one of ammoCountToRefill and ammoCountPerCharge";
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            foreach (StatDrawEntry item in base.SpecialDisplayStats(req))
            {
                yield return item;
            }
            if (!req.HasThing)
            {
                yield return new StatDrawEntry(VehicleStatCategoryDefOf.VehicleBasics, "Stat_Thing_ReloadMaxCharges_Name".Translate(ChargeNounArgument), maxCharges.ToString(), "Stat_Thing_ReloadMaxCharges_Desc".Translate(ChargeNounArgument), 2749);
            }
            if (ammoDef != null)
            {
                if (ammoCountToRefill != 0)
                {
                    yield return new StatDrawEntry(VehicleStatCategoryDefOf.VehicleBasics, "Stat_Thing_ReloadRefill_Name".Translate(ChargeNounArgument), $"{ammoCountToRefill} {ammoDef.label}", "Stat_Thing_ReloadRefill_Desc".Translate(ChargeNounArgument), 2749);
                }
                else
                {
                    yield return new StatDrawEntry(VehicleStatCategoryDefOf.VehicleBasics, "Stat_Thing_ReloadPerCharge_Name".Translate(ChargeNounArgument), $"{ammoCountPerCharge} {ammoDef.label}", "Stat_Thing_ReloadPerCharge_Desc".Translate(ChargeNounArgument), 2749);
                }
            }
            if (destroyOnEmpty)
            {
                yield return new StatDrawEntry(VehicleStatCategoryDefOf.VehicleBasics, "Stat_Thing_ReloadDestroyOnEmpty_Name".Translate(ChargeNounArgument), "Yes".Translate(), "Stat_Thing_ReloadDestroyOnEmpty_Desc".Translate(ChargeNounArgument), 2749);
            }
        }
    }
}
