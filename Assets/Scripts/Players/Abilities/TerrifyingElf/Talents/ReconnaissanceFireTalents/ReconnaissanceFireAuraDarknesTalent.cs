using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReconnaissanceFireAuraDarknesTalent : Talent
{
	[SerializeField] private ReconnaissanceFire reconnaissanceFire;

	public override void Enter()
	{
		reconnaissanceFire.ReconnaissanceFireAuraDarknesActive(true);
	}

	public override void Exit()
	{
		reconnaissanceFire.ReconnaissanceFireAuraDarknesActive(false);
	}
}
