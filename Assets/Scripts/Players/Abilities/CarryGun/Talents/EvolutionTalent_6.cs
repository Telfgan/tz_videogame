using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolutionTalent_6 : Talent
{
    [SerializeField] private CheliceraStrike cheliceraStrike;
    [SerializeField] private ClawStrike clawStrike;

    public override void Enter()
    {
        cheliceraStrike.ChanceApplyBleedingIncrease(true);
        clawStrike.ChanceApplyBleedingIncrease(true);
    }

    public override void Exit()
    {
        cheliceraStrike.ChanceApplyBleedingIncrease(false);
        clawStrike.ChanceApplyBleedingIncrease(false);
    }
}
