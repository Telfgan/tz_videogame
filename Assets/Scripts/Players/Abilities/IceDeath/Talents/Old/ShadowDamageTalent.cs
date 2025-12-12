using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowDamageTalent : Talent
{
	[SerializeField] private IceShadow _iceShadow;
	public override void Enter()
	{
		_iceShadow.TalentDamage(true);
	}

	public override void Exit()
	{
		_iceShadow.TalentDamage(false);
	}
}
