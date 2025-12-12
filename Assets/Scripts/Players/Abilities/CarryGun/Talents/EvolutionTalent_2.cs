using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolutionTalent_2 : Talent
{
    [SerializeField] private JumpWithChelicera jumpWithChelicera;
    [SerializeField] private SkillManager skillManager;

    public override void Enter()
    {
        skillManager.ActivateSkill(jumpWithChelicera);
    }

    public override void Exit()
    {
        skillManager.DeactivateSkill(jumpWithChelicera);
    }
}
