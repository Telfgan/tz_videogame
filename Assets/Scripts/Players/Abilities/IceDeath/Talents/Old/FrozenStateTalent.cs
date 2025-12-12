using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrozenStateTalent : Talent
{
	[SerializeField] private IceShower _iceShower;
	[SerializeField] private IceCloud _iceCloud;

	public override void Enter()
	{
		_iceShower.TalentBoostFrozenState(true);
		//_iceCloud.TalentBoostFrozenState(true);
	}

	public override void Exit()
	{
		_iceShower.TalentBoostFrozenState(false);
		//_iceCloud.TalentBoostFrozenState(false);
	}
}
