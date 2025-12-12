using UnityEngine;

public class LowHealthSparkOfLightTalent : Talent
{
    [SerializeField] private SparkOfLight _sparkOfLight;
    
    public override void Enter()
    {
        _sparkOfLight.EnableLowHealthTalent(true);
    }

    public override void Exit()
    {
        _sparkOfLight.EnableLowHealthTalent(false);
    }
}
