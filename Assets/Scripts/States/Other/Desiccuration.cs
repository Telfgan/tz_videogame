using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Desiccuration : AbstractCharacterState
{
	public bool turnOff = false;
	private float _baseDuration;
	private float _duration;
	private float _damageToExit;

	private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Move, StatusEffect.Ability };
	public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
	public override States State => States.Desiccuration;
	public override StateType Type => StateType.Physical;
	public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
	{
	//	Debug.Log("Entering Desiccuration State");

		_characterState = character;

		if (character.TryGetComponent<Character>(out var ability))
		{
			_abilities = ability.Abilities;
			_abilities.SetAbilitiesDisactive(true);
		}
		else
		{
			Debug.Log("no ability at " + character.gameObject.name);
		}
		_characterState.Character.Move.CanMove = false;
		_duration = durationToExit;
		_baseDuration = durationToExit;
		//_damageToExit = damageToExit;
		_damageToExit = 0.01f;
	}

	public override void UpdateState()
	{
	//	Debug.Log("Updating Desiccuration State");
		_duration -= Time.deltaTime;
		if (_duration < 0 || turnOff || _characterState.Character.Health.SumDamageTaken >= _damageToExit)
		{
			ExitState();
		}
	}

	public override void ExitState()
	{
	//	Debug.Log("Exiting Desiccuration State");
		_characterState.RemoveState(this);
		if (!_characterState.Check(StatusEffect.Move)) _characterState.Character.Move.CanMove = true;
		if (!_characterState.Check(StatusEffect.Ability) && _abilities != null) _abilities.SetAbilitiesDisactive(false);
	}

	public override bool Stack(float time)
	{
		if (_baseDuration > time)
		{
			return false;
		}
		else
		{
			_duration = time;
			return true;
		}
	}
}
