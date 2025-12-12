using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolutionTalent_9 : Talent
{
    [SerializeField] private CheliceraStrike cheliceraStrike;

    public override void Enter()
    {
        cheliceraStrike.EvolutionTalentTwo(true);
    }

    public override void Exit()
    {
        cheliceraStrike.EvolutionTalentTwo(false);
    }
}
