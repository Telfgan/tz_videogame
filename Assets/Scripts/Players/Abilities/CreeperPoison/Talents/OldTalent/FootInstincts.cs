using UnityEngine;

public class FootInstincts : Talent
{
    [SerializeField] private LightningMovement _lightningMovement;
    private float _reductionCooldownTime = 2.0f;

    public override void Enter()
    {
        SetActive(true);
    }

    public override void Exit()
    {
        SetActive(false);
    }

    public void ReductionCooldownLightningMovement()
    {
        if (_lightningMovement.RemainingCooldownTime > 0)
        {
            Debug.Log("FootInstincts / ReductionCooldown / baseRemainingCooldown = " + _lightningMovement.RemainingCooldownTime);
            float newRemainingCooldownTime = _lightningMovement.RemainingCooldownTime - _reductionCooldownTime;
            Debug.Log("FootInstincts / ReductionCooldown / newRemainingTime = " + newRemainingCooldownTime);
            _lightningMovement.ReductionSetCooldown(newRemainingCooldownTime);
            Debug.Log("FootInstincts / ReductionCooldown / _lightningMovement.RemainingCooldown = " + _lightningMovement.RemainingCooldownTime);
        }
    }
}
