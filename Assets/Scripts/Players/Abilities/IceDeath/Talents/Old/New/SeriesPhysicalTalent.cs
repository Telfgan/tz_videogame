using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeriesPhysicalTalent : Talent
{
    [SerializeField] private PhysicalAttack _physicalAttack;
    [SerializeField] private IceShadow _iceShadow;
    [SerializeField] private IceRolling _iceRolling;
    [SerializeField] private SkillManager _skillManager;

    public override void Enter()
    {
        _physicalAttack.SeriesPhysicalTalentActive(true);
        _skillManager.ActivateSkill(_iceShadow);
        _skillManager.ActivateSkill(_iceRolling);
    }

    public override void Exit()
    {
        _physicalAttack.SeriesPhysicalTalentActive(false);
        _skillManager.DeactivateSkill(_iceShadow);
        _skillManager.DeactivateSkill(_iceRolling);
    }
}
