using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PsionicsTalent_2 : Talent
{
    [SerializeField] private Conversion conversion;
    [SerializeField] private SkillManager skillManager;

    public override void Enter()
    {
        skillManager.ActivateSkill(conversion);
    }

    public override void Exit()
    {
        skillManager.DeactivateSkill(conversion);
    }
}
