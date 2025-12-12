using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkTalent_1 : Talent
{
    [SerializeField] StunMagicPassiveSkill stunMagicPassiveSkill;
    [SerializeField] SkillManager manager;

    public override void Enter()
    {
        manager.ActivateSkill(stunMagicPassiveSkill);
        stunMagicPassiveSkill.DamageDarkHealLightAddHealth(true, Data.DescriptionsForInfoPanel[0]);
    }

    public override void Exit()
    {
        manager.DeactivateSkill(stunMagicPassiveSkill);
        stunMagicPassiveSkill.DamageDarkHealLightAddHealth(false, Data.DescriptionsForInfoPanel[0]);
    }
}
