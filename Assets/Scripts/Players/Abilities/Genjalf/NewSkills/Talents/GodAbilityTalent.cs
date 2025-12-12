using UnityEngine;

public class GodAbilityTalent : Talent
{
	[SerializeField] private SkillManager _skillManager;

	public override void Enter()
	{
		_skillManager.TalentAddCharges(2);
	}

	public override void Exit()
	{
		_skillManager.TalentAddCharges(0);
	}
}
