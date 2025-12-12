using UnityEngine;

public class HealthBoostSparkOfLightTalent : Talent
{
    [SerializeField] private SparkOfLight _sparkOfLight;
    
    public override void Enter()
    {
        _sparkOfLight.EnableTalentPhysicalShieldBoost(true);
    }

    public override void Exit()
    {
        _sparkOfLight.EnableTalentPhysicalShieldBoost(false);
    }
}
