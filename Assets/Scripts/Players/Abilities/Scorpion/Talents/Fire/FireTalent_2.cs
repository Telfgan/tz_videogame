using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTalent_2 : Talent
{
    [SerializeField] private SkillManager skill;
    [SerializeField] private FireBreath_Scorpion fireBreath_Scorpion;

    public override void Enter()
    {
        skill.ActivateSkill(fireBreath_Scorpion);
    }

    public override void Exit()
    {
        skill.DeactivateSkill(fireBreath_Scorpion);
    }
}
