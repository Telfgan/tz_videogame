using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarknessTalent_1 : Talent
{
    [SerializeField] private ShotDarkness shotDarkness;
    [SerializeField] private SkillManager ability;

    public override void Enter()
    {
        ability.ActivateSkill(shotDarkness);
    }

    public override void Exit()
    {
        ability.ActivateSkill(shotDarkness);
    }
}
