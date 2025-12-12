using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyRegenTalent : Talent
{
	[SerializeField] private Energy _energy;
	public override void Enter()
	{
		_energy.TalentRegenEnergy(1);
	}

	public override void Exit()
	{
		_energy.TalentRegenEnergy(3);
	}

}
