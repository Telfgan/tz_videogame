using UnityEngine;

public class CooldownReduceFlashOfLighttalent : Talent
{
    [SerializeField] private FlashOfLight _flashOfLight;
    
    public override void Enter()
    {
        _flashOfLight.EnableTalentPhysicalShieldBoost(true);
    }

    public override void Exit()
    {
        _flashOfLight.EnableTalentPhysicalShieldBoost(false);
    }
}
