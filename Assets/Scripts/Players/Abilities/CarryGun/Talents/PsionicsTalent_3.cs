using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PsionicsTalent_3 : Talent
{
    [SerializeField] private Tentacles tentacles;
    [SerializeField] private PsionicEnergySkill psionicEnergySkill;

    public override void Enter()
    {
        psionicEnergySkill.DischargingPsiTalen(true);
        tentacles.PsionicsTalentThree(true);
    }

    public override void Exit()
    {
        psionicEnergySkill.DischargingPsiTalen(false);
        tentacles.PsionicsTalentThree(false);
    }
}
