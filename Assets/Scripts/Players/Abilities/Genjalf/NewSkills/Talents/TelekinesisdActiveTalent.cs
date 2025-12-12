using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelekinesisdActiveTalent : Talent
{
    [SerializeField] private Gangdollarff.Telekinesis _telekinesis;
    [SerializeField] private SkillManager _ability;

    public override void Enter()
    {
        _ability.ActivateSkill(_telekinesis);
    }

    public override void Exit()
    {
        _ability.DeactivateSkill(_telekinesis);
    }
}
