using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathSpiralSecondAttackTalent : Talent
{
	[SerializeField] private DeathSpiral _deathSpiral;
	public override void Enter()
	{
		_deathSpiral.TalentSecondAttack(true);
	}

	public override void Exit()
	{
		_deathSpiral.TalentSecondAttack(false);
	}
}
