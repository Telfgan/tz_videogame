using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostTalent_1 : Talent
{
    [SerializeField] private IceCloud _iceCloud;
    [SerializeField] private SkillManager _skillManager;

    public override void Enter()
    {
        //skillManager.ActivateSkill(iceCloud);
    }

    public override void Exit()
    {
        //skillManager.DeactivateSkill(iceCloud);
    }
}
