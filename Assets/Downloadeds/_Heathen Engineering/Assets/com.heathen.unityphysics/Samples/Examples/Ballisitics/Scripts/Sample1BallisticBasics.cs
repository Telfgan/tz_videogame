#if HE_SYSCORE

using HeathenEngineering.UnityPhysics.API;
using System;
using UnityEngine;

namespace HeathenEngineering.Demos
{
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class Sample1BallisticBasics : MonoBehaviour
    {
        [Serializable]
        public struct FantasyShooter
        {
            public Transform shotStart;
            public Transform target;
            public UnityEngine.UI.Slider maxHeight;
            public UnityEngine.UI.Slider linearSpeed;
        }

        [Header("Fantasy Ballistics")]
        public GameObject fantasyProjectile;
        public FantasyShooter shooter1;
        public FantasyShooter shooter2;
        public FantasyShooter shooter3;
        public FantasyShooter shooter4;

        private float nextFantasyShotTime = 0f;

        private void Start()
        {
            nextFantasyShotTime = Time.time + 2f;
        }

        private void Update()
        {
            if(nextFantasyShotTime <= Time.time)
            {
                nextFantasyShotTime = Time.time + 2f;
                Shoot(shooter1);
                Shoot(shooter2);
                Shoot(shooter3);
                Shoot(shooter4);
            }
        }

        private void Shoot(FantasyShooter shooter)
        {
            if (Ballistics.Solution(shooter.shotStart.position, shooter.linearSpeed.value, shooter.target.position, shooter.maxHeight.value, out Vector3 velocity, out float gravity))
            {
                var go = Instantiate(fantasyProjectile);
                go.transform.position = shooter.shotStart.position;
                var comp = go.GetComponent<Sample1FantasyProjectile>();
                comp.velocity = velocity;
                comp.gravity = gravity;
            }
        }
    }
}

#endif