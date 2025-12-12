using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TentacleGrip : AbstractCharacterState
{
	public bool turnOff = false;
	//private PlayerAbilities _abilities;
	private float _baseDuration;
	private float _duration;

	private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Move, StatusEffect.Ability };
	public override BaffDebaff BaffDebaff => BaffDebaff.Null;
	public override States State => States.TentacleGrip;
	public override StateType Type => StateType.Immaterial;
	public override List<StatusEffect> Effects => _effects;


	public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
	{
		_characterState = character;

		if (character.TryGetComponent<Character>(out var ability))
		{
			_abilities = ability.Abilities;
			_abilities.SetPhysicalAbilitiesDisactive(true);
		}
		else Debug.Log("no ability at " + character.gameObject.name);

		_characterState.Character.Move.IsMoveBlocked = true;
		_characterState.Character.Move.StopMoveAndAnimationMove();

		_duration = durationToExit;
		_baseDuration = durationToExit;
	}

	public override void UpdateState()
	{

	}

	public override void ExitState()
	{
		_characterState.RemoveState(this);
		if (!_characterState.Check(StatusEffect.Move)) _characterState.Character.Move.IsMoveBlocked = false;
		if (!_characterState.Check(StatusEffect.Ability) && _abilities != null) _abilities.SetPhysicalAbilitiesDisactive(false);
	}

	public override bool Stack(float time)
	{
		if (_baseDuration > time) return false;

		else
		{
			_duration = time;
			return true;
		}
	}
}

