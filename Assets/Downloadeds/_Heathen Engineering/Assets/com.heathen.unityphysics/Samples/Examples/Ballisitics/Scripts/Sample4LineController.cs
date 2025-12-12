#if HE_SYSCORE

using HeathenEngineering.UnityPhysics;
using UnityEngine;

namespace HeathenEngineering.Demos
{
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class Sample4LineController : MonoBehaviour
    {
        public BallisticPathLineRender lineRender;
        public Transform emitter;
        public BallisticAim aim;

        private void LateUpdate()
        {
            lineRender.start = emitter.position;
            lineRender.projectile.radius = 0.1f;
            lineRender.projectile.velocity = emitter.forward * aim.initialSpeed;
        }
    }
}

#endif