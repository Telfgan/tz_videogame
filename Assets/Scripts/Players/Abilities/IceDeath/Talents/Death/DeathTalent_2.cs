using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTalent_2 : Talent
{
    [SerializeField] private HarvestOfEnergy energy;
    [SerializeField] private HarvestOfRunes runes;
    [SerializeField] private SkillManager skillManager;

    public override void Enter()
    {
        skillManager.ActivateSkill(energy);
        skillManager.ActivateSkill(runes);
    }

    public override void Exit()
    {
        skillManager.DeactivateSkill(energy);
        skillManager.DeactivateSkill(runes);
    }
}
