using UnityEngine;

public class DispelTiredSoulTalent : Talent
{
    [SerializeField] private SoulAid _soulAid;
    
    public override void Enter()
    {
        _soulAid.EnableTiredSoulDispel(true);
    }

    public override void Exit()
    {
        _soulAid.EnableTiredSoulDispel(false);
    }
}
