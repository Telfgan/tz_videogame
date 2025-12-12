using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathSpiralHitTalent : Talent
{
    [SerializeField] private DeathSpiral _deathSpiral;

	public override void Enter()
	{
		_deathSpiral.TalentHitState(true);
	}

	public override void Exit()
	{
		_deathSpiral.TalentHitState(false);
	}
}
