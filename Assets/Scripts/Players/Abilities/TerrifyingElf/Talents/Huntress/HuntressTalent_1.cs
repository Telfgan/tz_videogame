using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntressTalent_1 : Talent
{
	[SerializeField] private Ghost ghost;
	[SerializeField] private VisionComponent visionComponent;
	[SerializeField] private Skill skill;

	public override void Enter()
	{
		ghost.MovingToGhostWithZeroMana(true);
		visionComponent.VisionRange += 3;
		skill.Radius += 1.5f;
	}

	public override void Exit()
	{
		ghost.MovingToGhostWithZeroMana(false);
		visionComponent.VisionRange -= 3;
		skill.Radius -= 1.5f;
	}
}
