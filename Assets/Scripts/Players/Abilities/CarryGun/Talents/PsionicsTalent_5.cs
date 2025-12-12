using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PsionicsTalent_5 : Talent
{
    [SerializeField] private CheliceraStrike cheliceraStrike;

    public override void Enter()
    {
        cheliceraStrike.PsionicsTalentTwo(true, Data.DescriptionsForInfoPanel[0]);
    }

    public override void Exit()
    {
        cheliceraStrike.PsionicsTalentTwo(false, Data.DescriptionsForInfoPanel[0]);
    }
}
