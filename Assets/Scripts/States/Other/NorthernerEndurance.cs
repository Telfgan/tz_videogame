using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NorthernerEndurance : AbstractCharacterState
{
	private float _durationToExit;
	private float _damageToExit;
	public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
	public override States State => States.NorthernerEndurance;
	public override StateType Type => StateType.Magic;
	public override List<StatusEffect> Effects => throw new System.NotImplementedException();

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
	{
		_characterState = character;	
		_health = character.Character.Health;
		//_health.BoostHpBonus(damageToExit);
		_durationToExit = durationToExit;
		_damageToExit = damageToExit;
	}

	public override void UpdateState()
	{
		_durationToExit -= Time.deltaTime;
		if (_durationToExit < 0)
		{
			ExitState();
		}
	}

	public override void ExitState()
	{
		_characterState.RemoveState(this);
		
		//_health.BoostHpReverse(_damageToExit);
	}

	public override bool Stack(float time)
	{
		_durationToExit = time;
		return true;
	}
}
