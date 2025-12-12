using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicDefenseTalent : Talent
{
	[SerializeField] private MagicDefense _magicDefense;
	[SerializeField] private SkillManager _ability;
	public override void Enter()
	{
		//_ability.ActivateSkill(_magicDefense);
	}

	public override void Exit()
	{
		//_ability.DeactivateSkill(_magicDefense);
	}
}
