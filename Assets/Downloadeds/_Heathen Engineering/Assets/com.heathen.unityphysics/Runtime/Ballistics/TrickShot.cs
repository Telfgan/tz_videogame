#if HE_SYSCORE
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    public class TrickShot : MonoBehaviour
    {
        public float speed;
        [Tooltip("The acceleration to be applied to the projectile over its flight, this is in the global space and would at minimal be the effect of gravity.\nYou can (Optionally) use the TrickShot Constant Acceleration component to calculate a more complex value accounting for composite effects both local and global.")]
        public Vector3 constantAcceleration = new(0, -9.81f, 0);
        public float radius;
        public BallisticPathFollow template;
        public float resolution = 0.01f;
        public float distance = 10f;
        public LayerMask collisionLayers = 0;
        public int bounces = 0;
        public float bounceDamping = 0.5f;
        [Tooltip("Should the distance be measured as arc length from the start or should it be considered for each bounce independently.")]
        public bool distanceIsTotalLength = false;
        public List<BallisticPath> prediction = new();

        Transform selfTransform;

        private void Start()
        {
            selfTransform = transform;
        }

        public void Shoot()
        {
            var GO = Instantiate(template.gameObject);
            var comp = GO.GetComponent<BallisticPathFollow>();
            comp.projectile = new BallisticsData { velocity = selfTransform.forward * speed, radius = radius };
            comp.path = new List<BallisticPath>(prediction);
            GO.transform.SetPositionAndRotation(selfTransform.position, selfTransform.rotation);
        }

        public void Predict()
        {
            var projectileSettings = new BallisticsData { velocity = selfTransform.forward * speed, radius = radius };
            if (bounces == 0)
            {
                prediction.Clear();
                prediction.Add(projectileSettings.Predict(selfTransform.position, null, resolution, distance, collisionLayers, constantAcceleration));
            }
            else if (distanceIsTotalLength)
            {
                prediction = new List<BallisticPath>();
                var current = projectileSettings.Predict(selfTransform.position, null, resolution, distance, collisionLayers, constantAcceleration);
                prediction.Add(current);

                if (current.impact.HasValue
                    && bounces > 0)
                {
                    var project = projectileSettings;
                    var (position, velocity, time) = current.steps[^1];
                    project.velocity = Vector3.Reflect(velocity, current.impact.Value.normal);
                    project.velocity *= (1f - bounceDamping);
                    var remainingDistance = distance - current.flightDistance;

                    for (int i = 0; i < bounces; i++)
                    {
                        current = project.Predict(position, current.impact.Value.collider, resolution, remainingDistance, collisionLayers, constantAcceleration);
                        prediction.Add(current);

                        if (current.impact.HasValue)
                        {
                            (position, velocity, time) = current.steps[current.steps.Length - 1];
                            project.velocity = Vector3.Reflect(velocity, current.impact.Value.normal);
                            project.velocity *= (1f - bounceDamping);
                            remainingDistance -= current.flightDistance;
                        }
                        else
                            break;

                    }
                }
            }
            else
            {
                prediction = new List<BallisticPath>();
                var current = projectileSettings.Predict(selfTransform.position, null, resolution, distance, collisionLayers, constantAcceleration);
                prediction.Add(current);

                if (current.impact.HasValue
                    && bounces > 0)
                {
                    var project = projectileSettings;
                    var (position, velocity, time) = current.steps[current.steps.Length - 1];
                    project.velocity = Vector3.Reflect(velocity, current.impact.Value.normal);
                    project.velocity *= (1f - bounceDamping);

                    for (int i = 0; i < bounces; i++)
                    {
                        current = project.Predict(position, current.impact.Value.collider, resolution, distance, collisionLayers, constantAcceleration);
                        prediction.Add(current);

                        if (current.impact.HasValue)
                        {
                            (position, velocity, time) = current.steps[current.steps.Length - 1];
                            project.velocity = Vector3.Reflect(velocity, current.impact.Value.normal);
                            project.velocity *= (1f - bounceDamping);
                        }
                        else
                            break;

                    }
                }
            }
        }
    }
}


#endif