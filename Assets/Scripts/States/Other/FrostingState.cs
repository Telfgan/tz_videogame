using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostingState : AbstractCharacterState
{
	public bool turnOff = false;

	private GameObject _ice;
	private AudioSource _audioSource;
	private float _duration;
	private float _baseDuration;
	private float _damageOnStart;
	private float _damageToExit;

	private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Move, StatusEffect.AbilitySpeed };
	public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
	public override States State => States.Frosting;
	public override StateType Type => StateType.Magic;
	public override List<StatusEffect> Effects => _effects;

	public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
	{
		//Debug.Log("Entering Frosting State");
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
		_audioSource = character.GetComponent<AudioSource>();

		_damageOnStart = _characterState.Character.Health.SumDamageTaken;
		_characterState.Character.Move.CanMove = false;
		_characterState.Character.Move.LookAtTransform(_characterState.gameObject.transform);

		if (character.TryGetComponent<Character>(out var ability))
		{
			_abilities = ability.Abilities;

			foreach (var abil in _abilities.Abilities)
			{
				if (abil.AbilityForm == AbilityForm.Physical)
				{
					abil.Buff.CastSpeed.ReductionPercentage(.5f);
				}
			}
		}

		else
		{
			Debug.Log("no ability at " + character.gameObject.name);
		}

		if (_characterState.StateEffects.Ice != null)
		{
			_ice = _characterState.StateEffects.Ice;
			_ice.SetActive(true);
		}

		if (_characterState.StateEffects.FrostingAudio != null) _audioSource.PlayOneShot(_characterState.StateEffects.FrostingAudio);
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
		//Debug.Log("Exiting Frosting State");
		_characterState.RemoveState(this);

		if (!_characterState.Check(StatusEffect.Move))
		{
			_characterState.Character.Move.CanMove = true;
		}

		_characterState.Character.Move.StopLookAt();

		if (!_characterState.Check(StatusEffect.AbilitySpeed) && _abilities != null)
		{
			foreach (var abil in _abilities.Abilities)
			{
				if (abil.AbilityForm == AbilityForm.Physical)
				{
					abil.Buff.CastSpeed.IncreasePercentage(.5f);
				}
			}
		}

		if (_characterState.StateEffects.Ice != null) _ice.SetActive(false);
	}

	public override bool Stack(float time)
	{
		_duration = _baseDuration;
		return true;
	}

}
