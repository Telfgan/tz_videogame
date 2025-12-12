using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmTalent_1 : Talent
{
    [SerializeField] private SkillManager skillManager;
    [SerializeField] private Tentacles tentacles;

    public override void Enter()
    {
        skillManager.ActivateSkill(tentacles);
    }

    public override void Exit()
    {
        skillManager.DeactivateSkill(tentacles);
    }
}
