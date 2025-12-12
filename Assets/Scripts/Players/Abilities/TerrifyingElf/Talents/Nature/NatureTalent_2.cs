using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NatureTalent_2 : Talent
{
    [SerializeField] private GrowTree growTree;
    [SerializeField] private SkillManager _ability;

    public override void Enter()
    {
        _ability.ActivateSkill(growTree);
    }

    public override void Exit()
    {
        _ability.DeactivateSkill(growTree);
    }
}
