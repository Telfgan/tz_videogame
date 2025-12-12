using UnityEngine;

public class HealingBuffTalent : Talent
{
    [SerializeField] private SparkOfLight _sparkOfLight;

    public override void Enter()
    {
        _sparkOfLight.EnableHealingBuffTalent(true);
    }

    public override void Exit()
    {
        _sparkOfLight.EnableHealingBuffTalent(false);
    }
}
