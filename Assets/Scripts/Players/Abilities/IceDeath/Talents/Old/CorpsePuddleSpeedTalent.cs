using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorpsePuddleSpeedTalent : Talent
{
	[SerializeField] private MinionAttack _minoinAttact;

	public override void Enter()
	{
		//_minoinAttact.TalentBoostSpeed(1.5f);
	}

	public override void Exit()
	{
		//_minoinAttact.TalentReduceSpeed(1.5f);
	}
}
