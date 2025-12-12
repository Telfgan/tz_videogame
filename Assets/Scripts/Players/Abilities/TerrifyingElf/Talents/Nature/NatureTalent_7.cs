using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NatureTalent_7 : Talent
{
    [SerializeField] private SleepSpell sleep;
    [SerializeField] private SkillManager _ability;

    public override void Enter()
    {
        _ability.ActivateSkill(sleep);
    }

    public override void Exit()
    {
        _ability.DeactivateSkill(sleep);
    }
}
