using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssasinTalent_2 : Talent
{
    [SerializeField] private SkillManager skill;
    [SerializeField] private CleavingBlade_Scorpion cleavingBlade_Scorpion;

    public override void Enter()
    {
        skill.ActivateSkill(cleavingBlade_Scorpion);
    }

    public override void Exit()
    {
        skill.DeactivateSkill(cleavingBlade_Scorpion);
    }
}
