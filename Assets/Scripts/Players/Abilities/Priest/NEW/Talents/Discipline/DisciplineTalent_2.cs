using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisciplineTalent_2 : Talent
{
    [SerializeField] private ReversePolarity reversePolarity;
    [SerializeField] private SkillManager ability;

    public override void Enter()
    {
        ability.ActivateSkill(reversePolarity);
    }

    public override void Exit()
    {
        ability.DeactivateSkill(reversePolarity);
    }
}
