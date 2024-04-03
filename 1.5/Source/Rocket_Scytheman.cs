using Verse;
using UnityEngine;
using RimWorld;

namespace VanillaVehiclesExpanded
{
    [HotSwappable]
    public class Rocket_Scytheman : Projectile_Explosive
    {
        private Vector3 LookTowards =>
            new(this.destination.x - this.origin.x, this.def.Altitude, this.destination.z - this.origin.z +
                                                                       this.ArcHeightFactor * (4 - 8 * this.DistanceCoveredFraction));
        private float ArcHeightFactor
        {
            get
            {
                float num = this.def.projectile.arcHeightFactor;
                float num2 = (this.destination - this.origin).MagnitudeHorizontalSquared();
                if (num * num > num2 * 0.2f * 0.2f) num = Mathf.Sqrt(num2) * 0.2f;

                return num;
            }
        }

        public override Quaternion ExactRotation => Quaternion.LookRotation(this.LookTowards);

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            ThrowDustPuffThick(this.DrawPos, Map, 5f);
        }

        public override void Tick()
        {
            base.Tick();
            if (this.Map != null)
            {
                float num = ArcHeightFactor * GenMath.InverseParabola(DistanceCoveredFraction);
                Vector3 drawPos = DrawPos;
                Vector3 position = drawPos + new Vector3(0f, 0f, 1f) * num;
                if (Rand.Chance(0.5f))
                {
                    ThrowSmokeTrail(position, base.Map, Vector3.Angle(origin, position), 1.5f);
                }
                ThrowRocketExhaust(position, base.Map, Vector3.Angle(origin, position), 1f);
            }
        }

        public void ThrowSmokeTrail(Vector3 loc, Map map, float angle, float size)
        {
            FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, VVE_DefOf.VVE_Scytheman_RocketSmoke, size);
            dataStatic.rotationRate = Rand.Range(-30f, 30f);
            dataStatic.velocityAngle = angle;
            dataStatic.velocitySpeed = Rand.Range(0.008f, 0.012f);
            map.flecks.CreateFleck(dataStatic);
        }

        public void ThrowRocketExhaust(Vector3 loc, Map map, float angle, float size)
        {
            FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, VVE_DefOf.VVE_Scytheman_RocketExhaust, size);
            dataStatic.velocityAngle = angle;
            dataStatic.solidTimeOverride = 0.20f * (1f - (DistanceCoveredFraction + 0.1f));
            dataStatic.velocitySpeed = 0.01f;
            map.flecks.CreateFleck(dataStatic);
        }

        public void ThrowDustPuffThick(Vector3 loc, Map map, float scale)
        {
            FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, FleckDefOf.DustPuffThick, scale);
            dataStatic.rotationRate = Rand.Range(-60, 60);
            dataStatic.velocityAngle = Rand.Range(0, 360);
            dataStatic.velocitySpeed = Rand.Range(0.6f, 0.75f);
            map.flecks.CreateFleck(dataStatic);
        }

    }
}
