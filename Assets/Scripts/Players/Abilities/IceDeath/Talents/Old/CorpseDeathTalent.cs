using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorpseDeathTalent : Talent
{
	[SerializeField] private DeathSpiral _deathSpiral;
	public override void Enter()
	{
		_deathSpiral.TalentCosrpseDeath(true);
	}

	public override void Exit()
	{
		_deathSpiral.TalentCosrpseDeath(false);
	}
}
