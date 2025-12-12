using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NatureTalent_1 : Talent
{
	[SerializeField] private Silence silence;
    [SerializeField] private SkillManager _ability;

	public override void Enter()
	{
		_ability.ActivateSkill(silence);
	}

	public override void Exit()
	{
		_ability.DeactivateSkill(silence);
	}
}
