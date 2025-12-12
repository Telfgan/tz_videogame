using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastBreath : AbstractCharacterState
{
	private Character _character;
	private float _durationToExit = 0;

	public override States State => States.LastBreath;
	public override StateType Type => StateType.Magic;
	public override List<StatusEffect> Effects => throw new System.NotImplementedException();
	public override BaffDebaff BaffDebaff => BaffDebaff.Baff;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
	{
		_character = character.Character;
		_abilities = _character.Abilities;
		_durationToExit = durationToExit;
		_health = _character.Health;

		_character.Move.ChangeMoveSpeed(1.2f);
		for (int i = 0; i < _abilities.Abilities.Count; i++)
		{
			_abilities.Abilities[i].Buff.AttackSpeed.IncreasePercentage(1.4f);
		}
		_health.RegenerationValue *= 4;
		//increase -regen
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
		//decrease -regen
		//_character.Move.ChangeMoveSpeedBack(1.2f);
		for (int i = 0; i < _abilities.Abilities.Count; i++)
		{
			_abilities.Abilities[i].Buff.AttackSpeed.ReductionPercentage(1.4f);
		}
		_health.RegenerationValue /= 4;
	}

	public override bool Stack(float time)
	{
		return true;
	}
}
