using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceSwordTalents : Talent
{
	[SerializeField] private IceSword _iceSword;
	[SerializeField] private SkillManager _ability;
	public override void Enter()
	{
		_ability.ActivateSkill(_iceSword);
	}

	public override void Exit()
	{
		_ability.DeactivateSkill(_iceSword);
	}
}
