#if HE_SYSCORE

using HeathenEngineering.UnityPhysics;
using HeathenEngineering.UnityPhysics.API;
using UnityEngine;

namespace HeathenEngineering.Demos
{
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class Sample3Launcher : MonoBehaviour
    {
        public Transform projector;
        public Transform emitter;
        public TrickShot2D trickShot;

        private void Aim()
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Ballistics.Solution2D(emitter.position, trickShot.speed, mousePos, Physics2D.gravity.magnitude, out Quaternion low, out Quaternion _);
            projector.rotation = low;
        }

        private void Update()
        {
            Aim();
            
            if(Input.GetMouseButtonDown(0))
            {
                trickShot.Shoot();
            }
        }
    }
}

#endif