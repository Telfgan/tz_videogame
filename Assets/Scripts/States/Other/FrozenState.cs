using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrozenState : AbstractCharacterState
{
	//public bool turnOff = false;
	private GameObject _frozenEffectInstance;
	private AudioSource _audioSource;
	private float _duration;
	private float _baseDuration;
	private float _damageToExit;
	private float _damageOnStart = 0;
	private bool _isInited = false;

	private Animator _animator;
	private AnimatorStateInfo _currentState;

	private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Move, StatusEffect.Ability };
	public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
	public override States State => States.Frozen;
	public override StateType Type => StateType.Magic;
	public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
	{
		//Debug.Log("Entering Frozen State");
		//MaxStacksCount = 5;
		//CanStack = false;
		_characterState = character;
		_duration = durationToExit;
		_baseDuration = durationToExit;
		if (damageToExit == 0)
		{
			_damageToExit = 10000;
		}
		else
		{
			_damageToExit = damageToExit;
		}

		_damageOnStart = _characterState.Character.Health.SumDamageTaken;
		_damageOnStart = 0;
		_characterState.Character.Move.CanMove = false;
		_characterState.Character.Move.Rigidbody.isKinematic = true;
		_characterState.Character.Move.LookAtTransform(_characterState.gameObject.transform);
		_audioSource = character.GetComponent<AudioSource>();

		if (character.TryGetComponent<Character>(out var ability))
		{
			_abilities = ability.Abilities;

			foreach (var abil in _abilities.Abilities)
			{
				abil.Disactive = true;
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
		if (_characterState.StateEffects.FrozenStateEffect != null)
		{
			_frozenEffectInstance = _characterState.StateEffects.FrozenStateEffect;
			_frozenEffectInstance.SetActive(true);
		}

		foreach (var mat in _characterState.StateEffects.MaterialsCharacter) mat.color = Color.cyan;
		if (_characterState.StateEffects.FrostingAudio != null) _audioSource.PlayOneShot(_characterState.StateEffects.FrozenAudio);

		_animator = _characterState.GetComponent<Animator>();

		if (_animator != null)
		{
			_currentState = _animator.GetCurrentAnimatorStateInfo(0);
			float normalizedTime = _currentState.normalizedTime % 1f;
			_animator.Play(_currentState.fullPathHash, 0, normalizedTime);
			_animator.Update(0);
			_animator.enabled = false;
		}
		_isInited = true;
		//_characterState.Health.sumDamageTaken = 0;
	}

	public override void UpdateState()
	{
		if(!_isInited) return;
		//Debug.Log(" Diffrence" + (_damageToExit - (_characterState.Character.Health.SumDamageTaken - _damageOnStart)) + "Damage to exit " + _damageToExit + " DamageHave " + (_characterState.Character.Health.SumDamageTaken - _damageOnStart));
		_duration -= Time.deltaTime;
		if (_characterState.Character.Health.SumDamageTaken - _damageOnStart >= _damageToExit || _duration <= 0 )//|| turnOff)
		{
			ExitState();
		}
	}

	public override void ExitState()
	{
		//Debug.Log("Exiting Frozen State");

		_characterState.RemoveState(this);
		if (!_characterState.Check(StatusEffect.Move))
		{
			_characterState.Character.Move.CanMove = true;
			_characterState.Character.Move.Rigidbody.isKinematic = false;
			_characterState.Character.Move.StopLookAt();
		}
		if (!_characterState.Check(StatusEffect.Ability) && _abilities != null)
		{
			foreach (var abil in _abilities.Abilities)
			{
				abil.Disactive = false;
			}
		}

		if (_frozenEffectInstance != null) _frozenEffectInstance.SetActive(false);
		foreach (var mat in _characterState.StateEffects.MaterialsCharacter) mat.color = Color.white;

		if (_animator != null)
		{
			_animator.enabled = true;
			_animator.speed = 1;
		}
	}

	public override bool Stack(float time)
	{
		//Debug.Log("Try stack");
		//_damageOnStart = _characterState.Character.Health.SumDamageTaken;
		//_damageToExit = 1;
		_duration = _baseDuration;
		return true;
	}
}
