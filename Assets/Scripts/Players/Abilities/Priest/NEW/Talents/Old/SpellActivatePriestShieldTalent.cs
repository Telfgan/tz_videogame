using UnityEngine;

public class SpellActivatePriestShieldTalent : Talent
{
    [SerializeField] private PriestShield _priestShield;
    
    public override void Enter()
    {
        _priestShield.EnableDisciplineShieldBoost(true);
    }

    public override void Exit()
    {
        _priestShield.EnableDisciplineShieldBoost(false);
    }
}
