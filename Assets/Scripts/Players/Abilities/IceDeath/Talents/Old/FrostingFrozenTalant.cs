using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostingFrozenTalant : Talent
{
	[SerializeField] private IcePuddle _icePuddle;
	[SerializeField] private CircularFrosting _circularFrosting;

	public override void Enter()
	{
		_icePuddle.SetTalentFrostingFrozen(true);
		_circularFrosting.SetTalentFrostingFrozen(true);
	}

	public override void Exit()
	{
		_icePuddle.SetTalentFrostingFrozen(false);
		_circularFrosting.SetTalentFrostingFrozen(false);
	}

}
