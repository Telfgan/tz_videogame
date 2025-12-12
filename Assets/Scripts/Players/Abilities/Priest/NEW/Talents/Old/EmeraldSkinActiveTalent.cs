using UnityEngine;

public class EmeraldSkinActiveTalent : Talent
{
	[SerializeField] private EmeraldSkin _emeraldSkin;
	[SerializeField] private SkillManager _ability;
	
	public override void Enter()
	{
		_ability.ActivateSkill(_emeraldSkin);
	}

	public override void Exit()
	{
		_ability.DeactivateSkill(_emeraldSkin);
	}
}

