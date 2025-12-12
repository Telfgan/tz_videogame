using UnityEngine;

public class DoubleRangeSoulAid : Talent
{
    [SerializeField] private SoulAid _soulAid;
    
    public override void Enter()
    {
        _soulAid.EnableDoubleRange(true);
    }

    public override void Exit()
    {
        _soulAid.EnableDoubleRange(false);
    }
}
