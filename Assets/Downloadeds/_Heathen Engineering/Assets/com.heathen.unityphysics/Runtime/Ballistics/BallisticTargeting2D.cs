#if HE_SYSCORE
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [RequireComponent(typeof(BallisticAim2D))]
    public class BallisticTargeting2D : MonoBehaviour
    {
        [Tooltip("The target to aim at")]
        public Transform targetTransform;
        [Tooltip("If provided the tool will account for target movement")]
        public Rigidbody2D targetBody;
        public bool HasSolution { get; private set; }

        private BallisticAim2D ballisticAim;

        private void Start()
        {
            ballisticAim = GetComponent<BallisticAim2D>();
        }

        private void LateUpdate()
        {
            if (targetTransform != null)
            {
                if (targetBody != null)
                    HasSolution = ballisticAim.Aim(targetTransform.position, targetBody.linearVelocity);
                else
                    HasSolution = ballisticAim.Aim(targetTransform.position);
            }
        }
    }
}

#endif