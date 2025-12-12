using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjaTalent_3 : Talent
{
    [SerializeField] IceSword iceSword;
    [SerializeField] SkillManager manager;

    public override void Enter()
    {
        manager.ActivateSkill(iceSword);
    }

    public override void Exit()
    {
        manager.DeactivateSkill(iceSword);
    }
}
