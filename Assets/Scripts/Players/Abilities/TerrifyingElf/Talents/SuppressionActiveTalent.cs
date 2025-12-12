using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuppressionActiveTalent : Talent
{
    [SerializeField] private Suppression suppression;
    [SerializeField] private SkillManager _ability;

    public override void Enter()
    {
        _ability.ActivateSkill(suppression);
    }

    public override void Exit()
    {
        _ability.DeactivateSkill(suppression);
    }
}
