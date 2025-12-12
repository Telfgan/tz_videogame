using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathSpiralChargeTalent : Talent
{
	[SerializeField] private DeathSpiral _deathSpiral;

	public override void Enter()
	{
		_deathSpiral.TalentMaxCharges(3);
	}

	public override void Exit()
	{
		_deathSpiral.TalentMaxCharges(1);
	}
}
