using UnityEngine;

public class PhyscialDamagePriestShieldTalent : Talent
{
    [SerializeField] private PriestShield _priestShield;

    public override void Enter()
    {
        _priestShield.EnableTalentPhysicalShieldBoost(true);
    }

    public override void Exit()
    {
        _priestShield.EnableTalentPhysicalShieldBoost(false);
    }
}
