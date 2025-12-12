using UnityEngine;

public class TiredSoulPriestTalent : Talent
{
    [SerializeField] private PriestShield _priestShield;
    public override void Enter()
    {
        _priestShield.EnableTiredSoulEvade(true);
    }

    public override void Exit()
    {
        _priestShield.EnableTiredSoulEvade(false);
    }
}
