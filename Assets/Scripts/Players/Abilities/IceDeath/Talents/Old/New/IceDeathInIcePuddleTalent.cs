using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceDeathInIcePuddleTalent : Talent
{
    [SerializeField] private IcePuddle icePuddle;

    public override void Enter()
    {
        //icePuddle.IceDeathInIcePudleTalentActive(true);
    }

    public override void Exit()
    {
        //icePuddle.IceDeathInIcePudleTalentActive(false);
    }
}
