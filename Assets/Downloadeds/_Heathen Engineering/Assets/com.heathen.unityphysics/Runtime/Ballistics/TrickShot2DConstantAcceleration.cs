#if HE_SYSCORE
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [RequireComponent(typeof(TrickShot2D))]
    public class TrickShot2DConstantAcceleration : MonoBehaviour
    {
        public List<Vector2> globalConstants = new(new Vector2[] { new(0, -9.81f) });
        public List<Vector2> localConstants = new();

        private TrickShot2D ts;

        private void Start()
        {
            ts = GetComponent<TrickShot2D>();
        }

        private void LateUpdate()
        {
            Vector2 sum = new Vector2();
            foreach (var v in globalConstants)
                sum += v;
            foreach (var v in localConstants)
                sum += (Vector2)(ts.transform.rotation * v);

            ts.constantAcceleration = sum;
        }
    }
}

#endif