using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityFormDebuff : AbstractCharacterState
{
	public bool turnOff = false;
	//private PlayerAbilities _abilities;
	private float _baseDuration;
	private float _duration;
	public AbilityForm canceledForm;
	public bool canCancel = false;

	private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.AbilitySchool };
	public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
	public override States State => States.FormDebuf;
	public override StateType Type => StateType.Immaterial;
	public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
	{
		Debug.Log("Entering AbilityFormDebuff State");
		_characterState = character;

		Debug.Log("CHECK FOR FORM " + canceledForm);

		if (character.TryGetComponent<Character>(out var ability))
		{
			_abilities = ability.Abilities;
			_abilities.SwitchAvaliable(canceledForm, false);
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
		Debug.Log("Updating AbilityFormDebuff State");
		_duration -= Time.deltaTime;
		if (_duration < 0 || turnOff)
		{
			ExitState();
		}
	}

	public override void ExitState()
	{
		Debug.Log("Exiting AbilityFormDebuff State");
		_characterState.RemoveState(this);
		if (!_characterState.Check(StatusEffect.Ability) && _abilities != null)
		{
			_abilities.SwitchAvaliable(canceledForm, true);
		}
	}

	public override bool Stack(float time)
	{

		if (_duration > time)
		{
			return true;
		}
		else
		{
			_duration = time;
			return true;
		}
	}
}
