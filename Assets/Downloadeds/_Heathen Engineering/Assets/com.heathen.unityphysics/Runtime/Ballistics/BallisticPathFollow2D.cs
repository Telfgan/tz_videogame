#if HE_SYSCORE
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    public class BallisticPathFollow2D : MonoBehaviour
    {
        public BallisticsData2D projectile;
        public List<BallisticPath2D> path;
        public bool resumeDynamicOnEnd = true;
        [Header("Planned Collisions")]
        [Tooltip("Tests if the bouce surface has moved before impact, if so the path will end")]
        public bool validateBounce = true;
        [Tooltip("Should the system invoke the OnCollisionEnter2D message for planned impacts")]
        public bool invokeOnCollisionEnter2D = true;
        [Tooltip("If OnCollisionEnter2D is invoked should we simulate the Collision2D input, if false we will input null")]
        public bool simulateCollision2D = false;
        [Header("Unplanned Collisions")]
        [Tooltip("If End On Collision is true what layers should collision be tested for.")]
        public LayerMask collisionMask = 1;
        [Tooltip("Should we check for unplanned collisions")]
        public bool endOnCollision = true;

        public Events.UnityVector2Event endOfPath;

        Transform selfTransform;
        float time;
        bool playing = false;
        int previous = -1;

        Queue<(GameObject target, Collision2D collision)> impactQueue = new Queue<(GameObject target, Collision2D collision)>();

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
            if(path == null
                || path.Count == 0)
                playing = false;

            if (playing)
            {
                var total = Time.time - time;
                if(Lerp(total))
                    playing = false;
            }
        }

        private void FixedUpdate()
        {
            while(impactQueue.Count > 0)
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
            (Vector2 position, Vector2 velocity, float time) current = default;
            var endPath = true;
            int currentSegment;

            //Walk the path
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
            if(endPath)
            {
                var cPath = path[^1];
                var cStep = cPath.steps[^1];
                var cancelImpact = false;

                //Simulate the collision if any and if required
                if (cPath.impact.HasValue
                    && invokeOnCollisionEnter2D)
                    cancelImpact = SimulatePlannedCollsion(cPath.impact.Value, cStep.velocity);

                if (cancelImpact)
                    EndOfPath(selfTransform.position, cStep.velocity);
                else
                    EndOfPath(cStep.position, cStep.velocity);

                return endPath;
            }
            
            //If we found a matching step test for prior bounce if required
            if (invokeOnCollisionEnter2D
                && previous < currentSegment
                && path[previous].impact.HasValue)
            {
                endPath = SimulatePlannedCollsion(path[previous].impact.Value, path[previous].steps[^1].velocity);
                previous = currentSegment;

                if(endPath)
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

        private bool ConsiderCollision(Vector2 position, Vector2 velocity, Collider2D segmentImpact)
        {
            //Look forward 2 fixed updates 
            var perdictHit = Physics2D.CircleCast(position, projectile.radius, velocity.normalized, velocity.magnitude * Time.fixedDeltaTime * 2, collisionMask);
            //If we are going to hit something unexpected then end the traverse
            if (perdictHit.collider != null
                && (segmentImpact == null
                    || segmentImpact != perdictHit.collider))
            {
                //Let the Dynamic solver do it
                return true;
            }
            else
                return false;
        }

        private bool SimulatePlannedCollsion(RaycastHit2D impact, Vector2 velocity)
        {
            if (validateBounce)
            {
                //Perdict forward 1 fixed frame and see if we see the thing we supposedly hit
                var perdictHit = Physics2D.CircleCast(selfTransform.position, projectile.radius, velocity.normalized, velocity.magnitude * Time.fixedDeltaTime);
                if (perdictHit.collider != impact.collider)
                {
                    //Nope either hit nothing or something different
                    return true;
                }
            }

            try
            {
                var invokeTarget = impact.collider.gameObject;

                if (simulateCollision2D)
                {
                    // Create a fake collision2D object for testing
                    var collisionData = (Collision2D)Activator.CreateInstance(typeof(Collision2D));

                    var myCollider = GetComponentInParent<Collider2D>();
                    var myBody = GetComponentInParent<Rigidbody2D>();

                    if (myCollider != null)
                    {
                        FieldInfo colliderField = typeof(Collision2D).GetField("m_Collider", BindingFlags.Instance | BindingFlags.NonPublic);
                        colliderField.SetValue(collisionData, myCollider.GetInstanceID()); // Set the collider property
                    }

                    if (myBody != null)
                    {
                        FieldInfo colliderField = typeof(Collision2D).GetField("m_Rigidbody", BindingFlags.Instance | BindingFlags.NonPublic);
                        colliderField.SetValue(collisionData, myBody.GetInstanceID()); // Set the collider property
                    }

                    FieldInfo contactsField = typeof(Collision2D).GetField("m_ReusedContacts", BindingFlags.Instance | BindingFlags.NonPublic);

                    ContactPoint2D contact = (ContactPoint2D)Activator.CreateInstance(typeof(ContactPoint2D));
                    FieldInfo pointField = typeof(ContactPoint2D).GetField("m_Point", BindingFlags.Instance | BindingFlags.NonPublic);
                    pointField.SetValue(contact, impact.point);
                    FieldInfo normalField = typeof(ContactPoint2D).GetField("m_Normal", BindingFlags.Instance | BindingFlags.NonPublic);
                    normalField.SetValue(contact, impact.normal);
                    FieldInfo relVelField = typeof(ContactPoint2D).GetField("m_RelativeVelocity", BindingFlags.Instance | BindingFlags.NonPublic);
                    relVelField.SetValue(contact, velocity);

                    contactsField.SetValue(collisionData, new ContactPoint2D[] { contact }); // Set the contacts property


                    FieldInfo relativeVelocityField = typeof(Collision2D).GetField("m_RelativeVelocity", BindingFlags.Instance | BindingFlags.NonPublic);
                    relativeVelocityField.SetValue(collisionData, velocity);

                    FieldInfo contactCountField = typeof(Collision2D).GetField("m_ContactCount", BindingFlags.Instance | BindingFlags.NonPublic);
                    contactCountField.SetValue(collisionData, 1);

                    impactQueue.Enqueue((invokeTarget, collisionData));
                }
                else
                {
                    impactQueue.Enqueue((invokeTarget, null));
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return false;
        }

        private void EndOfPath(Vector2 final, Vector2 velocity)
        {
            selfTransform.position = final;

            if (resumeDynamicOnEnd)
            {
                var body = GetComponentInParent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Dynamic;
                body.linearVelocity = velocity;
                body.WakeUp();
            }

            endOfPath.Invoke(velocity);
        }

        private void InjectCollision(GameObject target, Collision2D collision)
        {
            foreach (var component in target.GetComponents<MonoBehaviour>())
            {
                var method = component.GetType().GetMethod("OnCollisionEnter2D", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method != null)
                {
                    method.Invoke(component, new object[] { collision });
                }
            }

        }
    }
}


#endif