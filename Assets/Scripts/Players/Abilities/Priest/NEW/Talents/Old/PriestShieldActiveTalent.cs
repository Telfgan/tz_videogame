using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriestShieldActiveTalent : Talent
{
    [SerializeField] private PriestShield priestShield;
    [SerializeField] private SkillManager _ability;

    public override void Enter()
    {
        _ability.ActivateSkill(priestShield);
    }

    public override void Exit()
    {
        _ability.DeactivateSkill(priestShield);
    }
}
