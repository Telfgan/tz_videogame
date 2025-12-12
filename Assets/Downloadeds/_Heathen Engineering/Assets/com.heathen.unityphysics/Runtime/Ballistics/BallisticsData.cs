#if HE_SYSCORE
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    /// <summary>
    /// Assists in the simulation of ballistics behaviours
    /// </summary>
    [Serializable]
    public struct BallisticsData
    {
        public Vector3 velocity;
        public float radius;
        public float Speed
        {
            get => velocity.magnitude;
            set
            {
                velocity = velocity.normalized * value;
            }
        }
        public Vector3 Direction
        {
            get => velocity.normalized;
            set
            {
                velocity = value * velocity.magnitude;
            }
        }
        public Quaternion Rotation
        {
            get => Quaternion.LookRotation(Direction);
            set => velocity = value * Vector3.forward * Speed;
        }

        /// <summary>
        /// <para>Modifies the velocity such that the projectile will aim at the target accounting for the effects of gravity.</para>
        /// <para>Many solutions will have 2 options, a low or high arc, this method chooses the low arc</para>
        /// </summary>
        /// <param name="from">Where the projectile will start from</param>
        /// <param name="to">Where the projectile will end</param>
        /// <returns>True if a solution was found, false otherwise. Velocity is not modified if no solution was found</returns>
        public bool Aim(Vector3 from, Vector3 to)
        {
            if (API.Ballistics.Solution(from, Speed, to, Physics.gravity.magnitude, out Quaternion low, out Quaternion _) > 0)
            {
                Rotation = low;
                return true;
            }
            else
                return false;
        }

        public bool Aim(Vector3 from, Vector3 to, Vector3 constantAcceleration)
        {
            if (API.Ballistics.Solution(from, Speed, to, constantAcceleration, out Quaternion low, out Quaternion _) > 0)
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
        public bool AimHigh(Vector3 from, Vector3 to)
        {
            if (API.Ballistics.Solution(from, Speed, to, Physics.gravity.magnitude, out Quaternion low, out Quaternion high) > 0)
            {
                Rotation = high;
                return true;
            }
            else
                return false;
        }

        public bool AimHigh(Vector3 from, Vector3 to, Vector3 constantAcceleration)
        {
            if (API.Ballistics.Solution(from, Speed, to, constantAcceleration, out Quaternion low, out Quaternion high) > 0)
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
        /// <param name="fromCollider"></param>
        /// <param name="resolution"></param>
        /// <param name="distanceLimit"></param>
        /// <param name="collisionLayers"></param>
        /// <returns></returns>
        public BallisticPath Predict(Vector3 from, Collider fromCollider, float resolution, float distanceLimit, LayerMask collisionLayers)
        {
            if (velocity == Vector3.zero)
                return new BallisticPath
                {
                    flightDistance = 0,
                    flightTime = 0,
                    impact = null,
                    steps = new (Vector3 position, Vector3 velocity, float time)[] { }
                };

            if (API.Ballistics.SphereCast(from, fromCollider, velocity, Physics.gravity, radius, resolution, distanceLimit, collisionLayers, out var hit, out var path, out var flightDistance))
            {
                return new BallisticPath
                {
                    steps = path.ToArray(),
                    flightDistance = flightDistance,
                    flightTime = path[^1].time,
                    impact = hit
                };
            }
            else
            {
                return new BallisticPath
                {
                    steps = path.ToArray(),
                    flightDistance = flightDistance,
                    flightTime = path[^1].time,
                    impact = null
                };
            }
        }

        /// <summary>
        /// Predicts the path the projectile will take up to the first impact if any
        /// </summary>
        /// <param name="from"></param>
        /// <param name="fromCollider"></param>
        /// <param name="resolution"></param>
        /// <param name="distanceLimit"></param>
        /// <param name="collisionLayers"></param>
        /// <param name="constantAcceleration"></param>
        /// <returns></returns>
        public BallisticPath Predict(Vector3 from, Collider fromCollider, float resolution, float distanceLimit, LayerMask collisionLayers, Vector3 constantAcceleration)
        {
            if (velocity == Vector3.zero)
                return new BallisticPath
                {
                    flightDistance = 0,
                    flightTime = 0,
                    impact = null,
                    steps = new (Vector3 position, Vector3 velocity, float time)[] { }
                };


            RaycastHit hit;
            List<(Vector3 position, Vector3 velocity, float time)> path;
            bool impact;
            float flightDistance;
            if (constantAcceleration == Vector3.zero)
                impact = API.Ballistics.SphereCast(from, fromCollider, velocity, radius, resolution, distanceLimit, collisionLayers, out hit, out path, out flightDistance);
            else
                impact = API.Ballistics.SphereCast(from, fromCollider, velocity, constantAcceleration, radius, resolution, distanceLimit, collisionLayers, out hit, out path, out flightDistance);

            if (impact)
            {
                return new BallisticPath
                {
                    steps = path.ToArray(),
                    flightDistance = flightDistance,
                    flightTime = path[^1].time,
                    impact = hit
                };
            }
            else
            {
                return new BallisticPath
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