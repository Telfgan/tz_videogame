using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTalent_1 : Talent
{
    [SerializeField] private SkillManager skill;
    [SerializeField] private Teleportation_Scorpion teleportation_Scorpion;

    public override void Enter()
    {
        skill.ActivateSkill(teleportation_Scorpion);
    }

    public override void Exit()
    {
        skill.DeactivateSkill(teleportation_Scorpion);
    }
}
