using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntressTalent_4 : Talent
{
    [SerializeField] private ShotIntoSky shotIntoSky;
    [SerializeField] private SkillManager ability;

    public override void Enter()
    {
        ability.ActivateSkill(shotIntoSky);
    }

    public override void Exit()
    {
        ability.DeactivateSkill(shotIntoSky);
    }
}
