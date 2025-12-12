#if HE_SYSCORE
using System;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    /// <summary>
    /// Assists in the simulation of ballistics behaviours
    /// </summary>
    [Serializable]
    public struct BallisticsData2D
    {
        public Vector2 velocity;
        public float radius;
        public float Speed
        {
            get => velocity.magnitude;
            set
            {
                velocity = velocity.normalized * value;
            }
        }
        public Vector2 Direction
        {
            get => velocity.normalized;
            set
            {
                velocity = value * velocity.magnitude;
            }
        }
        public Quaternion Rotation
        {
            get => Quaternion.LookRotation(Vector3.forward, Direction);
            set => velocity = value * Vector2.right * Speed;
        }

        /// <summary>
        /// <para>Modifies the velocity such that the projectile will aim at the target accounting for the effects of gravity.</para>
        /// <para>Many solutions will have 2 options, a low or high arc, this method chooses the low arc</para>
        /// </summary>
        /// <param name="from">Where the projectile will start from</param>
        /// <param name="to">Where the projectile will end</param>
        /// <returns>True if a solution was found, false otherwise. Velocity is not modified if no solution was found</returns>
        public bool Aim(Vector2 from, Vector2 to)
        {
            if (API.Ballistics.Solution2D(from, Speed, to, Physics.gravity.magnitude, out Quaternion low, out Quaternion _) > 0)
            {
                Rotation = low;
                return true;
            }
            else
                return false;
        }
        public bool Aim(Vector2 from, Vector2 to, Vector2 constantAcceleration)
        {
            if (API.Ballistics.Solution2D(from, Speed, to, constantAcceleration, out Quaternion low, out Quaternion _) > 0)
            {
                Rotation = low;
                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// <para>Modifies the velocity such that the projectile will aim at the target accounting for the effects of gravity.</para>
        /// <para>Many solutions will have 2 options, a low or high arc, this method chooses the high arc</para>
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public bool AimHigh(Vector2 from, Vector2 to)
        {
            if (API.Ballistics.Solution2D(from, Speed, to, Physics.gravity.magnitude, out Quaternion low, out Quaternion high) > 0)
            {
                Rotation = high;
                return true;
            }
            else
                return false;
        }
        public bool AimHigh(Vector2 from, Vector2 to, Vector2 constantAcceleration)
        {
            if (API.Ballistics.Solution2D(from, Speed, to, constantAcceleration, out Quaternion low, out Quaternion high) > 0)
            {
                Rotation = high;
                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// Predicts the path the projectile will take up to the first impact if any
        /// </summary>
        /// <param name="from"></param>
        /// <param name="resolution"></param>
        /// <param name="distanceLimit"></param>
        /// <param name="collisionLayers"></param>
        /// <returns></returns>
        public BallisticPath2D Predict(Vector3 from, Collider2D fromCollider, float resolution, float distanceLimit, LayerMask collisionLayers)
        {
            if (velocity == Vector2.zero)
                return new BallisticPath2D
                {
                    flightDistance = 0,
                    flightTime = 0,
                    impact = null,
                    steps = new(Vector2 position, Vector2 velocity, float time)[] { }
                };

            if (API.Ballistics.CircleCast(from, fromCollider, velocity, Physics.gravity, radius, resolution, distanceLimit, collisionLayers, out var hit, out var path, out var flightDistance))
            {
                return new BallisticPath2D
                {
                    steps = path.ToArray(),
                    flightDistance = flightDistance,
                    flightTime = path[^1].time,
                    impact = hit
                };
            }
            else
            {
                return new BallisticPath2D
                {
                    steps = path.ToArray(),
                    flightDistance = flightDistance,
                    flightTime = path[^1].time,
                    impact = null
                };
            }
        }
        public BallisticPath2D Predict(Vector3 from, Collider2D fromCollider, float resolution, float distanceLimit, LayerMask collisionLayers, Vector2 constantAcceleration)
        {
            if (velocity == Vector2.zero)
                return new BallisticPath2D
                {
                    flightDistance = 0,
                    flightTime = 0,
                    impact = null,
                    steps = new (Vector2 position, Vector2 velocity, float time)[] { }
                };

            if (API.Ballistics.CircleCast(from, fromCollider, velocity, constantAcceleration, radius, resolution, distanceLimit, collisionLayers, out var hit, out var path, out var flightDistance))
            {
                return new BallisticPath2D
                {
                    steps = path.ToArray(),
                    flightDistance = flightDistance,
                    flightTime = path[^1].time,
                    impact = hit
                };
            }
            else
            {
                return new BallisticPath2D
                {
                    steps = path.ToArray(),
                    flightDistance = flightDistance,
                    flightTime = path[^1].time,
                    impact = null
                };
            }
        }
    }
}

#endif