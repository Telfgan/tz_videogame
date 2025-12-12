using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboSpeedTalent : Talent
{
    [SerializeField] private SeriesOfStrikes _combo;

	public override void Enter()
	{
		_combo.TalentBoostMultiplier(15);
	}

	public override void Exit()
	{
		_combo.TalentBoostMultiplier(5);
	}
}
