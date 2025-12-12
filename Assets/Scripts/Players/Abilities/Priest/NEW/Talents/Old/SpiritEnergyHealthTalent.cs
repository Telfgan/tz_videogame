using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritEnergyHealthTalent : Talent
{
    [SerializeField] private Restoration restoration;
    [SerializeField] private FlashOfLight flashOfLight;
    [SerializeField] private SparkOfLight spark;

    public override void Enter()
    {
        restoration.SpiritEnergyTalentActive(true);
        flashOfLight.SpiritEnergyTalentActive(true);
        spark.SpiritEnergyTalentActive(true);
    }

    public override void Exit()
    {
        restoration.SpiritEnergyTalentActive(false);
        flashOfLight.SpiritEnergyTalentActive(false);
        spark.SpiritEnergyTalentActive(false);
    }
}
