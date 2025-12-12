using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathSpiralBoostHpBodyTalent : Talent
{
	[SerializeField] private DeathSpiral _deathSpiral;
	public override void Enter()
	{
		_deathSpiral.TalentBoostHpCorpse(true);
	}

	public override void Exit()
	{
		_deathSpiral.TalentBoostHpCorpse(false);
	}
}
