using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjaTalent_2 : Talent
{
    [SerializeField] private SeriesOfStrikes _seriesOfStrikes;
	[SerializeField] private PhysicalAttack _physicalAttack;

	public override void Enter()
    {
        _seriesOfStrikes.SeriesCompliteCompoTalentActive(true);
		_physicalAttack.SeriesPhysicalTalentActive(true);
	}

    public override void Exit()
    {
        _seriesOfStrikes.SeriesCompliteCompoTalentActive(false);
		_physicalAttack.SeriesPhysicalTalentActive(false);
	}
}
