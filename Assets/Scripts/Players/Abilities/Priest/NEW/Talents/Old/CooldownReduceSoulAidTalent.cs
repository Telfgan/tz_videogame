using UnityEngine;

public class CooldownReduceSoulAidTalent : Talent
{
    [SerializeField] private SoulAid _soulAid;
    
    public override void Enter()
    {
        _soulAid.EnableCooldownReduce(true);
    }

    public override void Exit()
    {
        _soulAid.EnableCooldownReduce(false);
    }
}
