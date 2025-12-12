using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvadeTalent : Talent
{
	[SerializeField] private IceShadow _iceShadow;
	public override void Enter()
	{
		_iceShadow.TalentEvade(true);
	}

	public override void Exit()
	{
		_iceShadow.TalentEvade(false);
	}
}
