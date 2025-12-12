using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceRuneTalent : Talent
{
    [SerializeField] private SeriesOfStrikes runeComponent;
    [SerializeField] private NinjaResources ninjaResources;

    public override void Enter()
    {
        runeComponent.IceRuneTalentActive(true);
        //ninjaResources.EnergyToRestore(true);
    }

    public override void Exit()
    {
        runeComponent.IceRuneTalentActive(false);
        //ninjaResources.EnergyToRestore(false);
    }
}
