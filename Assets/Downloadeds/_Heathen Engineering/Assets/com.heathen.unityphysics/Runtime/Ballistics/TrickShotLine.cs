#if HE_SYSCORE
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(TrickShot))]
    public class TrickShotLine : MonoBehaviour
    {
        private TrickShot trickShot;
        private LineRenderer lineRenderer;
        public bool runOnStart = true;
        public bool continuousRun = true;

        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;

            trickShot = GetComponent<TrickShot>();
        }

        private void LateUpdate()
        {
            if (continuousRun)
                trickShot.Predict();

            List<Vector3> trajectory = new List<Vector3>();
            foreach (var path in trickShot.prediction)
            {
                foreach (var step in path.steps)
                    trajectory.Add(step.position);
            }

            lineRenderer.positionCount = trajectory.Count;
            lineRenderer.SetPositions(trajectory.ToArray());
        }
    }
}


#endif