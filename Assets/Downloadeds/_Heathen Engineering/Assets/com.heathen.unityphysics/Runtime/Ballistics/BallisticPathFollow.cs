#if HE_SYSCORE
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    public class BallisticPathFollow : MonoBehaviour
    {
        public BallisticsData projectile;
        public List<BallisticPath> path;
        public bool resumeDynamicOnEnd = true;
        [Header("Planned Collisions")]
        [Tooltip("Tests if the bouce surface has moved before impact, if so the path will end")]
        public bool validateBounce = true;
        [Tooltip("Should the system invoke the OnCollisionEnter message for planned impacts")]
        public bool invokeOnCollisionEnter = true;
        [Header("Unplanned Collisions")]
        [Tooltip("If End On Collision is true what layers should collision be tested for.")]
        public LayerMask collisionMask = 1;
        [Tooltip("Should we check for unplanned collisions")]
        public bool endOnCollision = true;
        public Events.UnityVector3Event endOfPath;

        Transform selfTransform;
        float time;
        bool playing = false;
        int previous = 0;

        Queue<(GameObject target, Collision collision)> impactQueue = new Queue<(GameObject target, Collision collision)>();

        private void Start()
        {
            selfTransform = transform;
            playing = true;
            previous = 0;
        }

        private void OnEnable()
        {
            time = Time.time;
        }

        private void LateUpdate()
        {
            if (path == null
                || path.Count == 0)
                playing = false;

            if (playing)
            {
                var total = Time.time - time;
                if (Lerp(total))
                    playing = false;
            }
        }

        private void FixedUpdate()
        {
            while (impactQueue.Count > 0)
            {
                var impact = impactQueue.Dequeue();
                InjectCollision(impact.target, impact.collision);
            }
        }

        private bool Lerp(float time)
        {
            //Mark our step
            var pastStepTime = 0f;
            var traveledTime = 0f;

            //Track the current step data
            (Vector3 position, Vector3 velocity, float time) current = default;
            var endPath = true;
            int currentSegment;

            for (currentSegment = 0; currentSegment < path.Count; currentSegment++)
            {
                //p is the current path segment to walk
                var p = path[currentSegment];
                //Track our current flight time
                traveledTime += p.flightTime;

                //if the input time is greter than the travle time then this segment is the segment we are currently pathing
                if (traveledTime > time)
                {
                    //Find the time we have been in this segment
                    var deltaTime = time - pastStepTime;
                    //Lerp this segment to get the current step data
                    current = p.Lerp(deltaTime);

                    endPath = false;
                    break;
                }

                pastStepTime = traveledTime;
            }

            //If we did not find a matching current step then we must be at the end
            if (endPath)
            {
                var cPath = path[^1];
                var cStep = cPath.steps[^1];
                var cancelImpact = false;

                //Simulate the collision if any and if required
                if (cPath.impact.HasValue
                    && invokeOnCollisionEnter)
                    cancelImpact = SimulatePlannedCollsion(cPath.impact.Value, cStep.velocity);

                if (cancelImpact)
                    EndOfPath(selfTransform.position, cStep.velocity);
                else
                    EndOfPath(cStep.position, cStep.velocity);

                return endPath;
            }

            //If we found a matching step test for prior bounce if required
            if (invokeOnCollisionEnter
                && previous < currentSegment
                && path[previous].impact.HasValue)
            {
                endPath = SimulatePlannedCollsion(path[previous].impact.Value, path[previous].steps[^1].velocity);
                previous = currentSegment;

                if (endPath)
                {
                    EndOfPath(selfTransform.position, path[previous].steps[^1].velocity);
                    return endPath;
                }
            }

            //If path has not terminated and we are testing dynamic collision
            if (endOnCollision)
            {
                endPath = ConsiderCollision(selfTransform.position, current.velocity, path[currentSegment].impact.HasValue ? path[currentSegment].impact.Value.collider : null);

                if (endPath)
                {
                    EndOfPath(selfTransform.position, current.velocity);
                    return endPath;
                }
            }

            selfTransform.position = current.position;
            return false;
        }

        private bool ConsiderCollision(Vector3 position, Vector3 velocity, Collider segmentImpact)
        {
            //Look forward 2 fixed updates 
            //If we are going to hit something unexpected then end the traverse
            if (Physics.SphereCast(position, projectile.radius, velocity.normalized, out var perdictHit, velocity.magnitude * Time.fixedDeltaTime * 2, collisionMask)
                && (segmentImpact == null
                    || segmentImpact != perdictHit.collider))
            {
                //Let the Dynamic solver do it
                return true;
            }
            else
                return false;
        }

        private bool SimulatePlannedCollsion(RaycastHit impact, Vector3 velocity)
        {
            if (validateBounce)
            {
                //Perdict forward 1 fixed frame and see if we see the thing we supposedly hit
                if (!Physics.SphereCast(selfTransform.position, projectile.radius, velocity.normalized, out var perdictHit, velocity.magnitude * Time.fixedDeltaTime)
                    || perdictHit.collider != impact.collider)
                {
                    //Nope either hit nothing or something different
                    return true;
                }
            }

            var invokeTarget = impact.collider.gameObject;
            impactQueue.Enqueue((invokeTarget, null));

            return false;
        }

        private void EndOfPath(Vector3 final, Vector3 velocity)
        {
            selfTransform.position = final;

            if (resumeDynamicOnEnd)
            {
                var body = GetComponentInParent<Rigidbody>();
                body.isKinematic = false;
                body.linearVelocity = velocity;
                body.WakeUp();
            }

            endOfPath.Invoke(velocity);
        }

        private void InjectCollision(GameObject target, Collision collision)
        {
            foreach (var component in target.GetComponents<MonoBehaviour>())
            {
                var method = component.GetType().GetMethod("OnCollisionEnter", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method != null)
                {
                    method.Invoke(component, new object[] { collision });
                }
            }

        }
    }
}


#endif