using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostTalent_4 : Talent
{
    [SerializeField] private IcePuddle _icePuddle;

    public override void Enter()
    {
        _icePuddle.IceDeathInIcePudleTalentActive(true, Data.DescriptionsForInfoPanel[0]);
    }

    public override void Exit()
    {
        _icePuddle.IceDeathInIcePudleTalentActive(false, Data.DescriptionsForInfoPanel[0]);
    }
}
