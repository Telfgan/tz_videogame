using DG.Tweening;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockOfIceProjectile : Projectiles
{
	private Vector3 startPos;
	private Damage _damage;
	private float _curDamage;

	private void Start()
	{
		_curDamage = 20 + Random.Range(0, 10);
		_damage = new Damage
		{
			Value = _curDamage,
			Type = DamageType.Magical,
		};
		Debug.Log("bullet");
		startPos = transform.position;
	}

	private void Update()
	{
		if (!_initialized) return;
		//_spriteRenderer.DOFade(0, 1);
		if (Vector2.Distance(transform.position, startPos) > _distance * GlobalVariable.cellSize)
		{
			Explode();
		}
	}

	[Server]
	private void OnTriggerEnter(Collider collision)
	{
		if (!_initialized || _dad == null) return;
		if (collision.gameObject == _dad.gameObject || collision.CompareTag("Ability"))
			return;
		//damage, freez etc
		if (collision.TryGetComponent<IDamageable>(out var damageable))
		{
			if (collision.TryGetComponent<Character>(out var target))
			{
				float duration = 9;
				
				if (target.CharacterState.CheckForState(States.Frozen))
				{
					_curDamage *= 1.4f;
				}
				//_energy.SumDamageMake(_curDamage);
				//_rune.SumDamageMake(curDamage);
				TargetRpcDamgeMake(_curDamage);

				target.Health.TryTakeDamage(ref _damage, _skill);
;
				target.CharacterState.AddState(States.Cooling, duration, 0, _dad.gameObject, _skill.name);
				GetComponent<Collider>().enabled = false;
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
}
