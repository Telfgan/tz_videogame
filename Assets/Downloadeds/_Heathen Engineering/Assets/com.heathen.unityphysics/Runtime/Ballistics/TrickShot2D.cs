#if HE_SYSCORE
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    public class TrickShot2D : MonoBehaviour
    {
        public float speed;
        public Vector2 constantAcceleration = new(0, -9.81f);
        public float radius;
        public BallisticPathFollow2D template;
        public float resolution = 0.01f;
        public float distance = 10f;
        public LayerMask collisionLayers = 0;
        public int bounces = 0;
        public float bounceDamping = 0.5f;
        public bool distanceIsTotalLength = false;
        public List<BallisticPath2D> prediction = new();

        Transform selfTransform;

        private void Start()
        {
            selfTransform = transform;
        }

        public void Shoot()
        {
            var GO = Instantiate(template.gameObject);
            var comp = GO.GetComponent<BallisticPathFollow2D>();
            comp.projectile = new BallisticsData2D { velocity = selfTransform.up * speed, radius = radius };
            comp.path = new List<BallisticPath2D>(prediction);
            GO.transform.SetPositionAndRotation(selfTransform.position, selfTransform.rotation);
        }

        [ContextMenu("Predict Path")]
        public void Predict()
        {
            if(selfTransform == null)
                selfTransform = transform;

            var projectileSettings = new BallisticsData2D { velocity = selfTransform.up * speed, radius = radius };
            if(bounces == 0)
            {
                prediction.Clear();
                prediction.Add(projectileSettings.Predict(selfTransform.position, null, resolution, distance, collisionLayers, constantAcceleration));
            }
            else if (distanceIsTotalLength)
            {
                prediction = new List<BallisticPath2D>();
                var current = projectileSettings.Predict(selfTransform.position, null, resolution, distance, collisionLayers, constantAcceleration);
                prediction.Add(current);

                if (current.impact.HasValue
                    && bounces > 0)
                {
                    var project = projectileSettings;
                    var (position, velocity, time) = current.steps[current.steps.Length - 1];
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
                prediction = new List<BallisticPath2D>();
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