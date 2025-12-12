using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NatureTalent_6 : Talent
{
	[SerializeField] private GrowTree growTree;
	[SerializeField] private ShotIntoSky shotIntoSky;
	[SerializeField] private ShotsIntoSky shotsIntoSky;


	public override void Enter()
	{
		growTree.treeHealthTalentActive(true);
		shotIntoSky.SetTripleShotTalentActive(true);
		shotsIntoSky.SetTripleShotTalentActive(true);
	}

	public override void Exit()
	{
		growTree.treeHealthTalentActive(false);
		shotIntoSky.SetTripleShotTalentActive(false);
		shotsIntoSky.SetTripleShotTalentActive(false);
	}
}
