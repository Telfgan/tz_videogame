using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReconnaissanceFireHealthTalent : Talent
{
	[SerializeField] private ReconnaissanceFire reconnaissanceFire;

	public override void Enter()
	{
		reconnaissanceFire.ReconnaissanceFireHealthTalentActive(true);
	}

	public override void Exit()
	{
		reconnaissanceFire.ReconnaissanceFireHealthTalentActive(false);
	}
}
