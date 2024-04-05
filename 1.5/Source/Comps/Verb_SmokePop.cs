using RimWorld;
using Verse;

namespace VanillaVehiclesExpanded
{
    public class Verb_SmokePop : Verb
    {
        public override bool TryCastShot()
        {
            Pop(base.EquipmentSource);
            return true;
        }

        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = false;
            return base.EquipmentSource.GetStatValue(StatDefOf.PackRadius);
        }

        public override void DrawHighlight(LocalTargetInfo target)
        {
            DrawHighlightFieldRadiusAroundTarget(caster);
        }

        public static void Pop(Thing parent)
        {
            var comp = parent.TryGetComp<CompReloadableWithVerbs>();
            if (comp != null && comp.CanBeUsed(out var _))
            {
                float statValue = parent.GetStatValue(StatDefOf.PackRadius);
                GenExplosion.DoExplosion(parent.Position, parent.Map, statValue, DamageDefOf.Smoke, null, -1, -1f, null, null, null, null, null, 0f, 1, GasType.BlindSmoke);
                comp.UsedOnce();
            }
        }
    }
}
