using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisciplineTalent_5 : Talent
{
	[SerializeField] private SoulAid _soulAid;
	[SerializeField] private SkillManager _ability;

	public override void Enter()
	{
		_ability.ActivateSkill(_soulAid);
		_soulAid.EnableDoubleRange(true);
	}

	public override void Exit()
	{
		_ability.DeactivateSkill(_soulAid);
		_soulAid.EnableDoubleRange(false);
	}
}
