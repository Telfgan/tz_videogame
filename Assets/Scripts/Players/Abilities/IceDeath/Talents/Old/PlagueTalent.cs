using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlagueTalent : Talent
{
    [SerializeField] private DeathSpiral _deathSpiral;

	public override void Enter()
	{
		_deathSpiral.TalentPlague(true);
	}

	public override void Exit()
	{
		_deathSpiral.TalentPlague(false);
	}
}
