using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisciplineTalent_1 : Talent
{
    [SerializeField] private PriestShield priestShield;
    [SerializeField] private SkillManager ability;

    public override void Enter()
    {
        ability.ActivateSkill(priestShield);
    }

    public override void Exit()
    {
        ability.DeactivateSkill(priestShield);
    }
}
