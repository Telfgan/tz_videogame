#if HE_SYSCORE

using HeathenEngineering.UnityPhysics.API;
using Unity.Mathematics;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    public class BallisticAim2D : MonoBehaviour
    {
        public float initialVelocity;
        [SerializeField]
        private Transform pivot;
        public Vector2 limit = new Vector2(-180, 180);

        public bool Aim(Vector2 target)
        {
            if (Ballistics.Solution2D(pivot.position, initialVelocity, target, Physics2D.gravity.magnitude, out Quaternion lowAngle, out Quaternion highAngle) > 0)
            {
                var ret = true;
                var eular = lowAngle.eulerAngles;
                eular.x = 0;
                eular.y = 0;

                if (eular.z > 180)
                    eular.z -= 360;
                else if (eular.z < -180)
                    eular.z += 360;

                if (eular.z < limit.x || eular.z > limit.x)
                {
                    eular.z = math.clamp(eular.z, limit.x, limit.y);
                    ret = false;
                }
                pivot.rotation = Quaternion.Euler(eular);

                return ret;
            }
            else
                return false;
        }

        public bool Aim(Vector2 target, Vector2 targetVelocity)
        {
            if (Ballistics.Solution2D(pivot.position, initialVelocity, target, targetVelocity, Physics2D.gravity.magnitude, out Quaternion lowAngle, out Quaternion highAngle) > 0)
            {
                var ret = true;
                var eular = lowAngle.eulerAngles;
                eular.x = 0;
                eular.y = 0;

                if (eular.z > 180)
                    eular.z -= 360;
                else if (eular.z < -180)
                    eular.z += 360;

                if (eular.z < limit.x || eular.z > limit.x)
                {
                    eular.z = math.clamp(eular.z, limit.x, limit.y);
                    ret = false;
                }
                pivot.rotation = Quaternion.Euler(eular);

                return ret;
            }
            else
                return false;
        }
    }
}

#endif