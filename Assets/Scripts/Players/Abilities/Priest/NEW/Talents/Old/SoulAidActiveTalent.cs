using UnityEngine;

public class SoulAidActiveTalent : Talent
{
	[SerializeField] private SoulAid _soulAid;
	[SerializeField] private SkillManager _ability;
	
	public override void Enter()
	{
		_ability.ActivateSkill(_soulAid);
	}

	public override void Exit()
	{
		_ability.DeactivateSkill(_soulAid);
	}
}

