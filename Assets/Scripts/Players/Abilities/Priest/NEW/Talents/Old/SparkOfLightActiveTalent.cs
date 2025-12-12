using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparkOfLightActiveTalent : Talent
{
    [SerializeField] private SparkOfLight sparkOfLight;
    [SerializeField] private FlashOfLight flashOfLight;
    [SerializeField] private SkillManager _ability;

    public override void Enter()
    {
        _ability.ActivateSkill(sparkOfLight);
        _ability.ActivateSkill(flashOfLight);
    }

    public override void Exit()
    {
        _ability.DeactivateSkill(sparkOfLight);
        _ability.DeactivateSkill(flashOfLight);
    }
}
