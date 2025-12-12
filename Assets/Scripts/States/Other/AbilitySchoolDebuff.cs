using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySchoolDebuff : AbstractCharacterState
{
	public bool turnOff = false;
	private float _baseDuration;
	private float _duration;
	public Schools canceledSchoool;

	private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.AbilitySchool };
	public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
	public override States State => States.SchoolDebuff;
	public override StateType Type => StateType.Immaterial;
	public override List<StatusEffect> Effects => _effects;

  

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
	{
		//Debug.Log("Entering AbilitySchoolDebuff State");

		_characterState = character;

		//Debug.Log("CHECK FOR SCHOOL " + canceledSchoool);
		if (character.TryGetComponent<Character>(out var ability))
		{
			_abilities = ability.Abilities;
			_abilities.SwitchAvaliable(canceledSchoool, false);
		}
		else
		{
			Debug.Log("no ability at " + character.gameObject.name);
		}
		_duration = durationToExit;
		_baseDuration = durationToExit;
	}

	public override void UpdateState()
	{
		//Debug.Log("Updating AbilitySchoolDebuff State");
		_duration -= Time.deltaTime;
		if (_duration < 0 || turnOff)
		{
			ExitState();
		}
	}

	public override void ExitState()
	{
		//Debug.Log("Exiting AbilitySchoolDebuff State");
		_characterState.RemoveState(this);
		if (!_characterState.Check(StatusEffect.Ability) && _abilities != null)
		{
			_abilities.SwitchAvaliable(canceledSchoool, true);
		}
	}

	public override bool Stack(float time)
	{
		//Debug.Log("STACKING TEST");
		if (_duration > time)
		{
		//	Debug.Log("STACKING TEST 2 2 2");
			return true;
		}
		else
		{
		//	Debug.Log("STACKING TEST 3 3 3");
			_duration = time;
			return true;
		}
	}
}
