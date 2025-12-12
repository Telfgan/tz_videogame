using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCloudBoostDmgTalent : Talent
{
	[SerializeField] private IceCloud _iceCloud;
	public override void Enter()
	{
		_iceCloud.TalentBoostDmg(true);
	}

	public override void Exit()
	{
		_iceCloud.TalentBoostDmg(false);
	}
}
