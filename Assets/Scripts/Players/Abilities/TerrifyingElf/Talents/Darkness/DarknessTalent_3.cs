using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarknessTalent_3 : Talent
{
    [SerializeField] private Ghost ghost;
    [SerializeField] private SkillManager skillManager;
    [SerializeField] private Silence silence;

    public override void Enter()
    {
        skillManager.ActivateSkill(ghost);
        silence.SetCanAttackMinions(true);
    }

    public override void Exit()
    {
        skillManager.DeactivateSkill(ghost);
        silence.SetCanAttackMinions(false);
    }
}
