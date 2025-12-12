using UnityEngine;

public class BlockOfIceTalent : Talent
{
	[SerializeField] private BlockOfIce _blockOfIce;
	[SerializeField] private SkillManager _ability;
	public override void Enter()
	{
		Debug.Log("Talent activated " + GetType().Name);
		_ability.ActivateSkill(_blockOfIce);
	}

	public override void Exit()
	{
		_ability.DeactivateSkill(_blockOfIce);
	}
}

