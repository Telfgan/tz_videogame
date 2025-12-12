#if HE_SYSCORE

using HeathenEngineering.UnityPhysics.API;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace HeathenEngineering.UnityPhysics
{
    public class BallisticAim : MonoBehaviour
    {
        [FormerlySerializedAs("initialVelocity")]
        public float initialSpeed;
        public Vector3 constantAcceleration = new(0, -9.81f, 0);
        [SerializeField]
        private Transform yPivot;
        [SerializeField]
        private Transform xPivot;
        public Vector2 yLimit = new(-180, 180);
        public Vector2 xLimit = new(-180, 180);

        public bool Aim(Vector3 target)
        {
            if (Ballistics.Solution(yPivot.position, initialSpeed, target, constantAcceleration, out Quaternion lowAngle, out _) > 0)
            {
                var ret = true;
                var eular = lowAngle.eulerAngles;
                eular.x = 0;
                
                if (eular.y > 180)
                    eular.y -= 360;
                else if (eular.y < -180)
                    eular.y += 360;

                if (eular.y < yLimit.x || eular.y > yLimit.x)
                {
                    eular.y = math.clamp(eular.y, yLimit.x, yLimit.y);
                    ret = false;
                }
                eular.z = 0;
                yPivot.rotation = Quaternion.Euler(eular);

                eular = lowAngle.eulerAngles;

                if (eular.x > 180)
                    eular.x -= 360;
                else if (eular.x < -180)
                    eular.x += 360;

                if (eular.x < xLimit.x || eular.x > xLimit.x)
                {
                    eular.x = math.clamp(eular.x, xLimit.x, xLimit.y);
                    ret = false;
                }

                if (eular.y > 180)
                    eular.y -= 360;
                else if (eular.y < -180)
                    eular.y += 360;

                if (eular.y < yLimit.x || eular.y > yLimit.x)
                {
                    eular.y = math.clamp(eular.y, yLimit.x, yLimit.y);
                    ret = false;
                }

                eular.z = 0;
                xPivot.rotation = Quaternion.Euler(eular);

                return ret;
            }
            else
                return false;
        }

        public bool Aim(Vector3 target, Vector3 targetVelocity)
        {
            //Find the linear speed across the plane formed by constantAcceleration;

            if (Ballistics.Solution(yPivot.position, initialSpeed, target, targetVelocity, constantAcceleration.magnitude, out Quaternion lowAngle, out _) > 0)
            {
                var ret = true;
                var eular = lowAngle.eulerAngles;
                eular.x = 0;

                if (eular.y > 180)
                    eular.y -= 360;
                else if (eular.y < -180)
                    eular.y += 360;

                if (eular.y < yLimit.x || eular.y > yLimit.x)
                {
                    eular.y = math.clamp(eular.y, yLimit.x, yLimit.y);
                    ret = false;
                }
                eular.z = 0;
                yPivot.rotation = Quaternion.Euler(eular);

                eular = lowAngle.eulerAngles;

                if (eular.x > 180)
                    eular.x -= 360;
                else if (eular.x < -180)
                    eular.x += 360;

                if (eular.x < xLimit.x || eular.x > xLimit.x)
                {
                    eular.x = math.clamp(eular.x, xLimit.x, xLimit.y);
                    ret = false;
                }

                if (eular.y > 180)
                    eular.y -= 360;
                else if (eular.y < -180)
                    eular.y += 360;

                if (eular.y < yLimit.x || eular.y > yLimit.x)
                {
                    eular.y = math.clamp(eular.y, yLimit.x, yLimit.y);
                    ret = false;
                }

                eular.z = 0;
                xPivot.rotation = Quaternion.Euler(eular);

                return ret;
            }
            else
                return false;
        }
    }
}

#endif