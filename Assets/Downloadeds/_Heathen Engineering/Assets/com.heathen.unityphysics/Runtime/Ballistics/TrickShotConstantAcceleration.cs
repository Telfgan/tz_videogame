#if HE_SYSCORE
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [RequireComponent(typeof(TrickShot))]
    [RequireComponent(typeof(TrickShot))]
    public class TrickShotConstantAcceleration : MonoBehaviour
    {
        [Header("Базовая «гравитация»")]
        public List<Vector3> globalConstants = new(new Vector3[] { new(0, -9.81f, 0) });
        public List<Vector3> localConstants = new();

        [Header("Диапазон длины траектории")]
        [SerializeField, Min(0f)] float minDrawDistance = 5f;
        [SerializeField, Min(0f)] float maxDrawDistance = 50f;

        [Header("Диапазон скорости, синхронный расстоянию")]
        [SerializeField, Min(0f)] float minSpeed = 5f;
        [SerializeField, Min(0f)] float maxSpeed = 40f;

        TrickShot ts;

        void Start() => ts = GetComponent<TrickShot>();

        void LateUpdate()
        {
            Vector3 acc = Vector3.zero;
            foreach (var g in globalConstants) acc += g;
            foreach (var l in localConstants) acc += ts.transform.rotation * l;
            ts.constantAcceleration = acc;

            SyncByMouse();
            ts.Predict();
        }

        void SyncByMouse()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit)) return;

            float rawDist = Vector3.Distance(ts.transform.position, hit.point);
            float dist = Mathf.Clamp(rawDist, minDrawDistance, maxDrawDistance);

            ts.distance = dist;

            float t = Mathf.InverseLerp(minDrawDistance, maxDrawDistance, dist);
            ts.speed = Mathf.Lerp(minSpeed, maxSpeed, t);
        }
    }
}

#endif
