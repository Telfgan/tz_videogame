using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhisRuneRegenTalent : Talent
{
	[SerializeField] private PhysicalAttack _physicalAttack;
	

	public override void Enter()
	{
		_physicalAttack.SetTalentActive(true);
	}

	public override void Exit()
	{
		_physicalAttack.SetTalentActive(false);
	}
}
