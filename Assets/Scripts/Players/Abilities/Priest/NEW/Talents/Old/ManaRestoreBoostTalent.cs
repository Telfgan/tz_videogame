using UnityEngine;

public class ManaRestoreBoostTalent : Talent
{
    [SerializeField] private SparkOfLight _sparkOfLight;
    
    public override void Enter()
    {
        _sparkOfLight.EnableManaRestoreBoostTalent(true);
    }

    public override void Exit()
    {
        _sparkOfLight.EnableManaRestoreBoostTalent(false);
    }
}
