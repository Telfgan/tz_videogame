using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightTalent_5 : Talent
{
    [SerializeField] private SparkOfLight _sparkOfLight;

    public override void Enter()
    {
        //_sparkOfLight.EnableLowHealthTalent(true);
        _sparkOfLight.EnableHealingBuffTalent(true);
    }

    public override void Exit()
    {
        //_sparkOfLight.EnableLowHealthTalent(false);
        _sparkOfLight.EnableHealingBuffTalent(false);
    }
}
