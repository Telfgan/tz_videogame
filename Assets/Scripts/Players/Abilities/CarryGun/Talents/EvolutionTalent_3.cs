using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolutionTalent_3 : Talent
{
    [SerializeField] private CheliceraStrike cheliceraStrike;
    [SerializeField] private ClawStrike clawStrike;

    public override void Enter()
    {
        clawStrike.ClawStrikeSpeed(true);
    }

    public override void Exit()
    {
        clawStrike.ClawStrikeSpeed(false);
    }
}
