using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritEnergyAdd : Talent
{
    [SerializeField] private SparkOfLight sparkOfLight;

    public override void Enter()
    {
        sparkOfLight.SpiritEnergyTalentActive(true);
    }

    public override void Exit()
    {
        sparkOfLight.SpiritEnergyTalentActive(false);
    }
}
