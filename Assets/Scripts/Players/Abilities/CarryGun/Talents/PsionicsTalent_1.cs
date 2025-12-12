using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PsionicsTalent_1 : Talent
{
    [SerializeField] private SkillManager skillManager;
    [SerializeField] private PsionicEnergySkill psionicEnergySkill;

    public override void Enter()
    {
        skillManager.ActivateSkill(psionicEnergySkill);
        psionicEnergySkill.PsiEnergyActive(true);
    }

    public override void Exit()
    {
        skillManager.DeactivateSkill(psionicEnergySkill);
        psionicEnergySkill.PsiEnergyActive(false);
    }
}
