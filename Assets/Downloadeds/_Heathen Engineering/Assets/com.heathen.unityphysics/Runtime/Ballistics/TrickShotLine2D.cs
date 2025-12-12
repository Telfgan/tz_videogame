#if HE_SYSCORE
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(TrickShot2D))]
    public class TrickShotLine2D : MonoBehaviour
    {
        private TrickShot2D trickShot2D;
        private LineRenderer lineRenderer;
        public bool runOnStart = true;
        public bool continiousRun = true;

        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;

            trickShot2D = GetComponent<TrickShot2D>();
        }

        private void LateUpdate()
        {
            if (continiousRun)
                trickShot2D.Predict();

            List<Vector3> trajectory = new List<Vector3>();
            foreach(var path in trickShot2D.prediction)
            {
                foreach(var step in path.steps)
                    trajectory.Add(step.position);
            }

            lineRenderer.positionCount = trajectory.Count;
            lineRenderer.SetPositions(trajectory.ToArray());
        }

        [ContextMenu("Draw Line")]
        private void EditorUpdateLine()
        {
#if UNITY_EDITOR
            if (trickShot2D == null)
                trickShot2D = GetComponent<TrickShot2D>();

            trickShot2D.Predict();

            if(lineRenderer == null)
                lineRenderer = GetComponent<LineRenderer>();

            List<Vector3> trajectory = new List<Vector3>();
            foreach (var path in trickShot2D.prediction)
            {
                foreach (var step in path.steps)
                    trajectory.Add(step.position);
            }

            lineRenderer.positionCount = trajectory.Count;
            lineRenderer.SetPositions(trajectory.ToArray());
#endif
        }
    }
}


#endif