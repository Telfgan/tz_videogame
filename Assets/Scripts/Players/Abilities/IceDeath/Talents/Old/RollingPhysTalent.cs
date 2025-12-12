using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingPhysTalent : Talent
{
	[SerializeField] private IceRolling _iceRolling;
	public override void Enter()
	{
		_iceRolling.TalentRollingPhys(true);
	}

	public override void Exit()
	{
		_iceRolling.TalentRollingPhys(false);
	}


}
