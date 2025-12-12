using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkTalent_2 : Talent
{
    [SerializeField] private FlowOfLight flowOfLight;
    [SerializeField] private SkillManager skillManager;

    public override void Enter()
    {
        skillManager.ActivateSkill(flowOfLight);
    }

    public override void Exit()
    {
        skillManager.DeactivateSkill(flowOfLight);
    }
}
