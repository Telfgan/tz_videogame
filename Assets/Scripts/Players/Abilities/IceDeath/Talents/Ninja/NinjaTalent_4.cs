using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjaTalent_4 : Talent
{
    [SerializeField] private IceShadow iceShadow;
    [SerializeField] private IcePuddle icePuddle;

    public override void Enter()
    {
        iceShadow.IceDeathInShadowTalentActive(true, Data.DescriptionsForInfoPanel[0]);
        icePuddle.IceDeathInIcePudleTalentActive(true, "");
    }

    public override void Exit()
    {
        iceShadow.IceDeathInShadowTalentActive(false, Data.DescriptionsForInfoPanel[0]);
        icePuddle.IceDeathInIcePudleTalentActive(false, "");
    }
}
