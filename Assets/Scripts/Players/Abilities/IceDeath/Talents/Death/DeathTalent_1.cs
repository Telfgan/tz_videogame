using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTalent_1 : Talent
{
    [SerializeField] private SeriesOfStrikes _seriesOfStrikes;
    [SerializeField] private NinjaResources _ninjaResources;

    public override void Enter()
    {
        _seriesOfStrikes.IceRuneTalentActive(true);
        _ninjaResources.EnergyToRestore(true, Data.DescriptionsForInfoPanel[0]);
    }

    public override void Exit()
    {
        _seriesOfStrikes.IceRuneTalentActive(false);
        _ninjaResources.EnergyToRestore(false, Data.DescriptionsForInfoPanel[0]);
    }
}
