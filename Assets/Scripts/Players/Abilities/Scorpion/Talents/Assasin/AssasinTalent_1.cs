using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssasinTalent_1 : Talent
{
    [SerializeField] private SkillManager skill;
    [SerializeField] private ChainBlade chainBlade_Scorpion;

    public override void Enter()
    {
        skill.ActivateSkill(chainBlade_Scorpion);
    }

    public override void Exit()
    {
        skill.DeactivateSkill(chainBlade_Scorpion);
    }
}
