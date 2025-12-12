using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceDeathInShadow : Talent
{
    [SerializeField] IceShadow iceShadow;
    [SerializeField] IcePuddle icePuddle;

    public override void Enter()
    {
        //iceShadow.IceDeathInShadowTalentActive(true);
    }

    public override void Exit()
    {
        //iceShadow.IceDeathInShadowTalentActive(true);
    }
}
