using DG.Tweening;
using Mirror;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class IceCloudProjectile : Projectiles
{	
	private Vector2 _startPos;
	private Damage _damage;
	private bool _boostDmg;
	private float _curDamage;
	private float _damageToExit = 1;

	private void Start()
	{
		_startPos = transform.position;
		_curDamage = 10 + _energyDad / 5;
		_damage = new Damage
		{
			Value = _curDamage,
			Type = DamageType.Physical,
		};
	}

	private void Update()
	{
		if (!_initialized) return;

		_spriteRenderer.DOFade(0, 1);
		//Debug.Log("Dist " + Vector2.Distance(transform.position, _startPos) + " Max dist " + _distance);
		if(Vector2.Distance(transform.position, _startPos) > _distance)
		{
			Explode();
		}
	}


	[Server]
	private void OnTriggerEnter(Collider collision)
	{
		if (!_initialized) return;
		if (_dad == null) return;
		if (collision.gameObject == _dad.gameObject || collision.gameObject.layer == LayerMask.NameToLayer("Allies"))
			return;

		if(collision.TryGetComponent<IDamageable>(out var damageable))
		{
			if (collision.TryGetComponent<Character>(out var target) && target != _dad)
			//if (damageable is HeroComponent target)
			{
				
				//target.CharacterState.AddState(States.Plague, 40, 0, _dad.gameObject, _skill.Name);

				float duration = 100+ _energyDad / 20;

				if (target.CharacterState.CheckForState(States.Frozen) && _boostDmg)
				{
					_curDamage *= 1.4f;
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
				_energy.UseAllEnergy();
				ClientUse(_energyDad, _energy.gameObject);

				StartCoroutine(CrutchDelay(target, duration));

				//target.CharacterState.AddState(States.Frozen, duration, target.Health.SumDamageTaken + _damageToExit, _dad.gameObject, _skill.name);
				GetComponent<Collider>().enabled = false;
				//Explode();
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
			//Explode();
		}
	}

	private IEnumerator CrutchDelay(Character target, float duration)
	{
		//yield return new WaitForSecondsRealtime(0.1f);
		yield return null;
		target.CharacterState.AddState(States.Frozen, duration, target.Health.SumDamageTaken + _damageToExit, _dad.gameObject, _skill.name);
		Explode();
	}

	//[ClientRpc]
	private void ClientUse(float value, GameObject player)
	{
		/*Energy energy = null;
		for (int i = 0; i < _dad.Resources.Count; i++)
		{
			if (_dad.Resources[i].Type == ResourceType.Energy)
			{
				energy = (Energy)_dad.Resources[i];
			}
		}
		energy.TryUse(value);*/
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

	public void Talent(bool value, bool frozenState, bool lastHit)
	{
		_boostDmg = value;
		if(lastHit)
		{
			if (frozenState) _damageToExit = 60;
			else _damageToExit = 30;
		}
		else
		{
			_damageToExit = 1;
		}
	}
}
