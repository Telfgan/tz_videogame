using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightTalent_2 : Talent
{
    [SerializeField] private Restoration restoration;
    [SerializeField] private SkillManager ability;

    public override void Enter()
    {
        ability.ActivateSkill(restoration);
    }

    public override void Exit()
    {
        ability.DeactivateSkill(restoration);
    }
}
