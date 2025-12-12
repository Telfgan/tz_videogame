using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTalent_3 : Talent
{
    [SerializeField] private NinjaResources ninjaResources;

    public override void Enter()
    {
        ninjaResources.HardenedFleshTalent(true, Data.DescriptionsForInfoPanel[0]);
    }

    public override void Exit()
    {
        ninjaResources.HardenedFleshTalent(false, Data.DescriptionsForInfoPanel[0]);
    }
}
