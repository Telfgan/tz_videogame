using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClapOfLightTalent : Talent
{
    [SerializeField] private Gangdollarff.ClapOfLight _clapOfLight;
    [SerializeField] private SkillManager _ability;
    
    public override void Enter()
    {
        _ability.ActivateSkill(_clapOfLight);
    }

    public override void Exit()
    {
        _ability.DeactivateSkill(_clapOfLight);
    }
}
