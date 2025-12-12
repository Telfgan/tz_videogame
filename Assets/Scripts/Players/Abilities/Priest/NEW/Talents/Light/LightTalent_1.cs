using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightTalent_1 : Talent
{
    [SerializeField] private FlashOfLight flashOfLight;
    [SerializeField] private SkillManager ability;

    public override void Enter()
    {
        ability.ActivateSkill(flashOfLight);
    }

    public override void Exit()
    {
        ability.DeactivateSkill(flashOfLight);
    }
}
