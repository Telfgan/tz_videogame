using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stupefaction : AbstractCharacterState
{
	public bool turnOff = false;
	//private PlayerAbilities _abilities;
	private float _baseDuration;
	private float _duration;

	private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Move, StatusEffect.Ability };
	public override BaffDebaff BaffDebaff => BaffDebaff.Debaff;
	public override States State => States.Stupefaction;
	public override StateType Type => StateType.Physical;
	public override List<StatusEffect> Effects => _effects;


	public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
	{
		_characterState = character;

		if (character.TryGetComponent<Character>(out var ability))
		{
			_abilities = ability.Abilities;
			_abilities.SetAbilitiesDisactive(true);
		}

		_characterState.Character.Move.IsMoveBlocked = true;
		_characterState.Character.Move.StopMoveAndAnimationMove();

		_duration = durationToExit;
		_baseDuration = durationToExit;

		_characterState.Character.Health.DamageTaken += OnAnyDamage;
	}

	public override void UpdateState()
	{
		_duration -= Time.deltaTime;
		if (_duration < 0 || turnOff)
		{
			ExitState();
		}
	}

	public override void ExitState()
	{
		_characterState.Character.Health.DamageTaken -= OnAnyDamage;
		_characterState.RemoveState(this);
		if (!_characterState.Check(StatusEffect.Move)) _characterState.Character.Move.IsMoveBlocked = false;
		if (!_characterState.Check(StatusEffect.Ability) && _abilities != null) _abilities.SetAbilitiesDisactive(false);
		turnOff = false;
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

	private void OnAnyDamage(Damage damage, Skill fromSkill) => turnOff = true;
}
