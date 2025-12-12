using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonTalent_2 : Talent
{
    [SerializeField] private PoisonBall poisonBall;
    [SerializeField] private SkillManager skillManager;

    public override void Enter()
    {
        skillManager.ActivateSkill(poisonBall);
    }

    public override void Exit()
    {
        skillManager.DeactivateSkill(poisonBall);
    }
}
