using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvadeBoostIcePuddleTalent : Talent
{
	[SerializeField] private IcePuddle _icePuddle;
	public override void Enter()
	{
		_icePuddle.SetTalentEvadeDadBoost(true);
	}

	public override void Exit()
	{
		_icePuddle.SetTalentEvadeDadBoost(false);
	}
}
