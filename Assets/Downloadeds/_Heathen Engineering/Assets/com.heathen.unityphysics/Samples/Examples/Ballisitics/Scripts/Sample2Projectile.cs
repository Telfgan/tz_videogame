#if HE_SYSCORE

using Unity.Mathematics;
using UnityEngine;

namespace HeathenEngineering.Demos
{
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class Sample2Projectile : MonoBehaviour
    {
        Rigidbody selfBody;

        private void Start()
        {
            selfBody = GetComponent<Rigidbody>();
            selfBody.drag = 0;
            selfBody.angularDrag = 0;
            selfBody.isKinematic = true;
        }

        public void OnPathEnd(float3 velocity)
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