using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClapOfLightUpTalent2 : Talent
{
    [SerializeField] private Gangdollarff.ClapOfLight _skill;
    public override void Enter()
    {
        _skill.IsBaffed = true;
    }

    public override void Exit()
    {
        _skill.IsBaffed = false;
    }
}
