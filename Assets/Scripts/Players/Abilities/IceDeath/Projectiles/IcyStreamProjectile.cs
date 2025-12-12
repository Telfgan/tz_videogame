using DG.Tweening;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class IcyStreamProjectile : Projectiles
{
	private float _timer = 1.5f;
	private float _time = 0;
	private float _stepOfTimer = 0.3f;
	private float _damage = 1;
	private float _chanceOfFrosting = 0.05f;
	private float _durationOfDebuff = 2;
	private List<Character> _enemyList = new List<Character>();
	//private Vector2 startPos;

	/*private void Start()
	{
		Debug.Log("bullet");
		//startPos = transform.position;
		//_rb.AddForce(transform.up * _force, ForceMode2D.Impulse);
		if (_energyDad >= 40)
		{
			_durationOfDebuff += 4;
		}
		else
		{
			_durationOfDebuff += _energyDad / 10;
		}
		_rb.DOMove(transform.up * _distance * GlobalVariable.cellSize, _timer).OnComplete(Explode);
	}*/

	public override void Init(Character dad, float energy, bool lastHit, Skill skill)
	{
		_dad = dad;
		_energyDad = energy;
		_initialized = true;
		_lastHit = lastHit;
		_skill = skill;
		for (int i = 0; i < _dad.Resources.Count; i++)
		{
			if (_dad.Resources[i].Type == ResourceType.Energy)
			{
				_energy = (Energy)_dad.Resources[i];
			}
		}
		Debug.Log("bullet init");
	}

	private void Update()
	{
		Timer();
		_spriteRenderer.DOFade(0, _timer);
	}

	[Server]
	private void OnTriggerEnter(Collider collision)
	{
		if (!_initialized || _dad == null) return;
		if (collision.gameObject == _dad.gameObject)
			return;

		if (collision.TryGetComponent<Character>(out var target))
		{
			_enemyList.Add(target);
			_energy.SumDamageMake(_damage);

			Damage damage = new Damage
			{
				Value = _damage,
				Type = DamageType.Magical,
				PhysicAttackType = AttackRangeType.RangeAttack,
			};
			//_skill.CmdApplyDamage(damage, target.gameObject);
			target.Health.TryTakeDamage(ref damage, _skill);

			target.CharacterState.AddState(States.Cooling, _durationOfDebuff, 0, _dad.gameObject, _skill.name);
		}
		Debug.Log("check collider");
		//Explode();
	}
	private void OnTriggerExit(Collider collision)
	{
		if (collision.gameObject == _dad.gameObject || collision.CompareTag("Ability"))
			return;

		if (collision.TryGetComponent<Character>(out var target))
		{
			_enemyList.Remove(target);
		}
	}
	private void Explode()
	{
		_enemyList.Clear();
		if (_hitEffect != null)
		{
			GameObject hitEffect = Instantiate(_hitEffect, transform.position, Quaternion.identity);
			Destroy(hitEffect, 5f);
		}
		Destroy(gameObject);
	}

	private void Timer()
	{
		_time += Time.deltaTime;
		if(_time >= _stepOfTimer)
		{
			_time = 0;
			_distance++;
			_chanceOfFrosting *= 2;
			for(int i = 0; i < _enemyList.Count; i ++)
			{
				Damage damage = new Damage
				{
					Value = _damage,
					Type = DamageType.Magical,
					PhysicAttackType = AttackRangeType.RangeAttack,
				};
				//_skill.CmdApplyDamage(damage, target.gameObject);
				_enemyList[i].Health.TryTakeDamage(ref damage, _skill);


				_enemyList[i].CharacterState.AddState(States.Cooling, _durationOfDebuff, 0, _dad.gameObject, _skill.name);
				if (Random.Range(0, 1f) <= _chanceOfFrosting)
				{
					_enemyList[i].CharacterState.AddState(States.Frosting, _durationOfDebuff, 0, _dad.gameObject, _skill.name);
					if (_lastHit) //its talent bool, no last hit 
					{
						_enemyList[i].CharacterState.AddState(States.Frozen, _durationOfDebuff, 0, _dad.gameObject, _skill.name);
					}
				}
			}
		}
	}
}
