using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceShowerProjectile : Projectiles
{
	private Vector2 _startPos;
	private Damage _damage;
	private bool _boostDmg;
	private float _curDamage;
	private float _damageToExit = 1;

	private void Start()
	{
		_startPos = transform.position;
		_curDamage = 10 + _energyDad / 4;
		_damage = new Damage
		{
			Value = _curDamage,
			Type = DamageType.Physical,
		};
	}

	public override void Init(Character dad, float energy, bool lastHit, Skill skill)
	{
		_dad = dad;
		_energyDad = energy;
		_initialized = true;
		_lastHit = lastHit;
		_skill = skill;
		_rb.AddForce(-transform.up * _force, ForceMode.Impulse);
		for (int i = 0; i < _dad.Resources.Count; i++)
		{
			if (_dad.Resources[i].Type == ResourceType.Energy)
			{
				_energy = (Energy)_dad.Resources[i];
			}
			if (_dad.Resources[i].Type == ResourceType.Rune)
			{
				_rune = (RuneComponent)_dad.Resources[i];
			}
		}
		Debug.Log("bullet init");
	}

	private void Update()
	{
		if (Vector2.Distance(transform.position, _startPos) > _distance * GlobalVariable.cellSize)
		{
			Explode();
		}
	}


	[Server]
	private void OnTriggerEnter(Collider collision)
	{
		if (_dad == null) return;
		if (collision.gameObject == _dad.gameObject)
			return;

		if (collision.TryGetComponent<IDamageable>(out var damageable))
		{
			if (collision.TryGetComponent<Character>(out var target) && target != _dad)
			//if (damageable is HeroComponent target)
			{

				//target.CharacterState.AddState(States.Plague, 40, 0, _dad.gameObject, _skill.Name);

				float duration = 100 + _energyDad / 20;

				if (target.CharacterState.CheckForState(States.Frozen) && _boostDmg)
				{
					_curDamage *= 1.4f;
					Debug.Log("NEW DAMAGE");
				}

				TargetRpcDamgeMake(_curDamage);
				//_skill.CmdApplyDamage(_damage, target.gameObject);
				target.Health.TryTakeDamage(ref _damage, _skill);

				//talents???
				if (_dad.Health.ResistMagDamage >= 20)
				{
					_dad.Health.SetEvadeMagic(5);
				}
				else
				{
					_dad.Health.SetEvadeMagic(20);
				}
				for (int i = 0; i < _dad.Resources.Count; i++)
				{
					if (_dad.Resources[i].Type == ResourceType.Energy)
					{
						_energy = (Energy)_dad.Resources[i];
					}
				}
				//_energy.TryUse(_energyDad);
				_energy.UseAllEnergy();
				//ClientUse(_energyDad, _energy.gameObject);
				target.CharacterState.AddState(States.Frozen, duration, target.Health.SumDamageTaken + _damageToExit, _dad.gameObject, _skill.name);
				//damage
				GetComponent<Collider>().enabled = false;
				Explode();
			}
			else
			{
				damageable.TryTakeDamage(ref _damage, _skill);
				if (_damage.Value <= 0)
				{
					Explode();
				}
				return;
			}
			Explode();
		}
	}


	private void Explode()
	{
		if (_hitEffect != null)
		{
			GameObject hitEffect = Instantiate(_hitEffect, transform.position, Quaternion.identity);
			Destroy(hitEffect, 5f);
		}
		Destroy(gameObject);
	}

	public void Talent(bool value, bool frozenState)
	{
		_boostDmg = value;
		if (frozenState)
		{
			_damageToExit = 30;
		}
		else
		{
			_damageToExit = 1;
		}
	}
}
