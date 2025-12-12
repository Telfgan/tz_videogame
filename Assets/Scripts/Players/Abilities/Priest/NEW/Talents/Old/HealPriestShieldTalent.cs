using UnityEngine;

public class HealPriestShieldTalent : Talent
{
    [SerializeField] private PriestShield _priestShield;
    
    public override void Enter()
    {
        _priestShield.EnableHealingBoost(true);
    }

    public override void Exit()
    {
        _priestShield.EnableHealingBoost(false);
    }
}
