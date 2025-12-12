using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FisuraActiveTalent : Talent
{
    [SerializeField] private Gangdollarff.Fisura _fisura;
    [SerializeField] private SkillManager _ability;

    public override void Enter()
    {
        _ability.ActivateSkill(_fisura);
    }

    public override void Exit()
    {
        _ability.DeactivateSkill(_fisura);
    }
}
