using UnityEngine;

public class EmeraldLightMagicBoostTalent : Talent
{
    [SerializeField] private EmeraldSkin _emeraldSkin;
    
    public override void Enter()
    {
        _emeraldSkin.EnableTalentLightMagicBoost(true);
    }

    public override void Exit()
    {
        _emeraldSkin.EnableTalentLightMagicBoost(false);
    }
}
