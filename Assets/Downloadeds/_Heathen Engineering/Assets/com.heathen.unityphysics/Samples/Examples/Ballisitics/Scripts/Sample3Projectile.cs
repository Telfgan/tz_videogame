#if HE_SYSCORE

using HeathenEngineering.UnityPhysics;
using Unity.Mathematics;
using UnityEngine;

namespace HeathenEngineering.Demos
{
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class Sample3Projectile : MonoBehaviour
    {
        Rigidbody2D selfBody;

        private void Start()
        {
            selfBody = GetComponent<Rigidbody2D>();
            selfBody.bodyType = RigidbodyType2D.Kinematic;
            selfBody.drag = 0;
            selfBody.angularDrag = 0;
            selfBody.useFullKinematicContacts = true;
        }

        public void OnPathEnd(float2 velocity)
        {
            Invoke(nameof(TimeKill), 5);
        }

        void TimeKill()
        {
            Destroy(gameObject);
        }
    }
}

#endif