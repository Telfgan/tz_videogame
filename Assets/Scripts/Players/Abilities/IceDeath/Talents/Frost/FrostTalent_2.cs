using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostTalent_2 : Talent
{
    [SerializeField] private IcePuddle _icePuddle;
    [SerializeField] IceShadow _iceShadow;
    [SerializeField] private SkillManager _skillManager;

    public override void Enter()
    {
        //skillManager.ActivateSkill(iceShadow);
        //skillManager.ActivateSkill(icePuddle);
    }

    public override void Exit()
    {
        //skillManager.DeactivateSkill(iceShadow);
        //skillManager.DeactivateSkill(icePuddle);
    }
}
