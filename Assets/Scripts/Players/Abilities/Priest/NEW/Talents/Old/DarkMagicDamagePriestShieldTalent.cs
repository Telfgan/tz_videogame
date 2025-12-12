using UnityEngine;

public class DarkMagicDamagePriestShieldTalent : Talent
{
    [SerializeField] private PriestShield _priestShield;
    
    public override void Enter()
    {
        _priestShield.EnableDarkMagicBoost(true);
    }

    public override void Exit()
    {
        _priestShield.EnableDarkMagicBoost(false);
    }
}
