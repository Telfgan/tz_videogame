using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cooling : AbstractCharacterState
{
	public bool turnOff = false;
	private float _duration;
	private float _baseDuration;
	private float _damageOnStart;
	private float _damageToExit;
	private float _curAbilityDebuf = 0.1f;
	private float _curSpeedDebuf = 0.05f;

	private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.MoveSpeed, StatusEffect.AbilitySpeed };
	public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
	public override States State => States.Cooling;
	public override StateType Type => StateType.Physical;
	public override List<StatusEffect> Effects => _effects;


    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
	{
		//Debug.Log("Entering cooling State");
		_characterState = character;

		if (damageToExit == 0)
		{
			_damageToExit = 10000;
		}
		else
		{
			_damageToExit = damageToExit;
		}
		_duration = durationToExit;
		_baseDuration = durationToExit;
		_damageOnStart = _characterState.Character.Health.SumDamageTaken;

		_characterState.Character.Move.ChangeMoveSpeed(1 - _curSpeedDebuf);
		//decrease speed of attact and movement
		//_characterState.Health.sumDamageTaken = 0;
	}

	public override void UpdateState()
	{
		_duration -= Time.deltaTime;
		if (_characterState.Character.Health.SumDamageTaken - _damageOnStart >= _damageToExit || _duration < 0 || turnOff)
		{
			ExitState();
		}
	}

	public override void ExitState()
	{
		//Debug.Log("Exiting cooling State");
		_characterState.RemoveState(this);
		if (!_characterState.Check(StatusEffect.MoveSpeed))
		{
			_characterState.Character.Move.SetDefaultSpeed();
			//_characterState.Move.CanMove = true;
		}
		if (!_characterState.Check(StatusEffect.AbilitySpeed))
		{
			//return speed of attact
		}
	}

	public override bool Stack(float time)
	{
		//Debug.Log("stacked");
		//_characterState.Move.SetDefaultSpeed();
		_duration = time;
		_curSpeedDebuf += 0.05f;
		_curAbilityDebuf += 0.1f;
		//ability speed decrease
		_characterState.Character.Move.ChangeMoveSpeed(1 - _curSpeedDebuf);
		//_duration = _baseDuration;
		return true;
	}

}
