using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathSpiralTalents : Talent
{
	[SerializeField] private DeathSpiral _deathSpiral;
	[SerializeField] private SkillManager _ability;
	public override void Enter()
	{
		_ability.ActivateSkill(_deathSpiral);
	}

	public override void Exit()
	{
		_ability.DeactivateSkill(_deathSpiral);
	}
}
