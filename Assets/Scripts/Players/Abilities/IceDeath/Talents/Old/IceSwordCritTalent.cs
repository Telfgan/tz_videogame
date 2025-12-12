using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceSwordCritTalent : Talent
{
	[SerializeField] private IceSword _iceSword;
	public override void Enter()
	{
		_iceSword.TalentCritDmg(true);
	}

	public override void Exit()
	{
		_iceSword.TalentCritDmg(false);
	}
}
