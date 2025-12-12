using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceShardPlagueTalent : Talent
{
	[SerializeField] private IceShard _iceShard;
	public override void Enter()
	{
		_iceShard.TalentPlague(true);
	}

	public override void Exit()
	{
		_iceShard.TalentPlague(false);
	}

	
}
