using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonTalent_1 : Talent
{
    [SerializeField] private SkillManager skillManager;
    [SerializeField] private SpitPoison spitPoison;

    public override void Enter()
    {
        skillManager.ActivateSkill(spitPoison);
    }

    public override void Exit()
    {
        skillManager.DeactivateSkill(spitPoison);
    }
}
