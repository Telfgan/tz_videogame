#if HE_SYSCORE
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    /// <summary>
    /// Defines the ballistic path of a projectile
    /// </summary>
    public struct BallisticPath
    {
        public (Vector3 position, Vector3 velocity, float time)[] steps;
        public float flightDistance;
        public float flightTime;
        public RaycastHit? impact;

        public static BallisticPath Get(Vector3 start, Collider startCollider, Vector3 velocity, float radius, float resolution, float maxLength, LayerMask collisionLayers)
        {
            if(API.Ballistics.SphereCast(start, startCollider, velocity, Physics.gravity, radius, resolution, maxLength, collisionLayers, out var hit, out var path, out var distance))
            {
                return new BallisticPath
                {
                    flightDistance = distance,
                    flightTime = path[^1].time,
                    impact = hit,
                    steps = path.ToArray(),
                };
            }
            else
            {
                return new BallisticPath
                {
                    flightDistance = distance,
                    flightTime = path[^1].time,
                    impact = hit,
                    steps = null,
                };
            }
        }

        public (Vector3 position, Vector3 velocity, float time) Lerp(float time)
        {
            if (steps == null
                || steps.Length == 0)
                return (Vector3.zero, Vector3.zero, 0f);

            if (steps.Length == 1
                || time <= 0)
                return steps[0];

            if (time >= flightTime)
                return steps[^1];

            for (int i = 0; i < steps.Length; i++)
            {
                if (steps[i].time > time)
                {
                    var (sPos, sVel, sT) = steps[i - 1];
                    var (ePos, eVel, eT) = steps[i];
                    var dT = eT - sT;
                    var lT = (time - sT) / dT;
                    return (Vector3.Lerp(sPos, ePos, lT), Vector3.Lerp(sVel, eVel, lT), time);
                }
            }

            return steps[^1];
        }
    }
}

#endif