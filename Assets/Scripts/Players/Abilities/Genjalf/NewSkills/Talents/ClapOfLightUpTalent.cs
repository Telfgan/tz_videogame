using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClapOfLightUpTalent : Talent
{
    [SerializeField] private Gangdollarff.ClapOfLight _skill;
    public override void Enter()
    {
        _skill.Radius = _skill.Radius * 1.5f;
    }

    public override void Exit()
    {
        _skill.Radius = _skill.Radius / 1.5f;
    }
}
