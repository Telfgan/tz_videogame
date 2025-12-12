using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceShardTalent : Talent
{
	[SerializeField] private IceShard _iceShard;
	[SerializeField] private SkillManager _ability;
	public override void Enter()
	{
		_ability.ActivateSkill(_iceShard);
	}

	public override void Exit()
	{
		_ability.DeactivateSkill(_iceShard);
	}
}
