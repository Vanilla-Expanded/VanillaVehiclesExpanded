using Verse;
using UnityEngine;
using RimWorld;

namespace VanillaVehiclesExpanded
{
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

        public static void ThrowSmokeTrail(Vector3 loc, Map map, float angle, float size)
        {
            FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, FleckDefOf.Smoke, size);
            dataStatic.rotationRate = Rand.Range(-0.5f, 0.5f);
            dataStatic.velocityAngle = angle;
            dataStatic.velocitySpeed = Rand.Range(0.008f, 0.012f);
            map.flecks.CreateFleck(dataStatic);
        }

        public override Quaternion ExactRotation => Quaternion.LookRotation(this.LookTowards);

        public override void Tick()
        {
            base.Tick();
            if (this.Map != null)
            {
                if (Rand.Chance(0.5f))
                {
                    float num = ArcHeightFactor * GenMath.InverseParabola(DistanceCoveredFraction);
                    Vector3 drawPos = DrawPos;
                    Vector3 position = drawPos + new Vector3(0f, 0f, 1f) * num;
                    ThrowSmokeTrail(position, base.Map, Vector3.Angle(origin, position), 1f);
                }
            }
        }
    }
}
