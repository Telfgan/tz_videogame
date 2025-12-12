using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisciplineTalent_7 : Talent
{
    [SerializeField] private EmeraldSkin emeraldSkin;
    [SerializeField] private SkillManager ability;

    public override void Enter()
    {
        ability.ActivateSkill(emeraldSkin);
    }

    public override void Exit()
    {
        ability.DeactivateSkill(emeraldSkin);
    }
}
