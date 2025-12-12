using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IceShadowObject : Projectiles
{
	//[HideInInspector] public EnergyPlayer energyPlayer;
	[HideInInspector] public float timeToDestroy = 2;

	[SerializeField] private bool enemyShadow = true;
	[SerializeField] private Animator anim;

	private Health _healthPlayer;
	private Damage _damage;
	private bool _talentDamage = false;
	private bool _iceDeathInShadowTalent = false;
	private float _damageTimer = 1f;
	private float _modifierRegen;
	/*
	 * timer to destroy
	 * buff player
	 * */
	public override void Init(Character dad, float energy, bool lastHit, Skill skill)
	{
		_skill = skill;
		_dad = dad;
		_energyDad = energy;
		_healthPlayer = _dad.Health;
		_initialized = true;
		_lastHit = lastHit;

		float extraTime = Mathf.Min(energy / 10f, 3f);

		timeToDestroy += extraTime;

		_damage = new Damage
		{
			Value = 2,
			Type = DamageType.Magical,
		};

		StartCoroutine(DestroyShadow());
	}

	public void SetAnimationState(int animationHash, float normalizedTime, float velocityX, float velocityZ, Quaternion rotation)
	{
		if (anim != null)
		{
			anim.Play(animationHash, 0, normalizedTime);
			anim.Update(0);

			anim.SetFloat(HashAnimPlayer.VelocityX, velocityX);
			anim.SetFloat(HashAnimPlayer.VelocityZ, velocityZ);

			StartCoroutine(StopAnimationAfterFrame());
		}

		transform.rotation = rotation;
	}

	private IEnumerator StopAnimationAfterFrame()
	{
		yield return null;
		if (anim != null)
		{
			anim.speed = 0;
		}
	}

	private void Update()
	{
		//if(_talentDamage)
		//{
		//	MakeDamage();
		//}
	}

	private void OnTriggerExit(Collider collision)
	{
		if (collision.gameObject.TryGetComponent<IcePuddleObject>(out IcePuddleObject icePuddleObject)) _modifierRegen = 0;
		if (collision.gameObject == _dad.gameObject)
		{
			_dad.Health.DecreaseRegen(1.01f + _modifierRegen);
			//_healthPlayer.SetBoostRegen(0.01f);
			//Debug.LogError("setboost in hp has been deleted");
			return;
		}
	}
	[Server]
	private void OnTriggerEnter(Collider collision)
	{
		if(_dad == null) return;

		if (_iceDeathInShadowTalent && collision.gameObject.TryGetComponent<IcePuddleObject>(out IcePuddleObject icePuddleObject)) _modifierRegen = 0.03f;

		if (collision.gameObject == _dad.gameObject)
		{
			if (_iceDeathInShadowTalent) _dad.Health.IncreaseRegen(1.01f + _modifierRegen);
			//_healthPlayer.SetBoostRegen(0.01f);
			//Debug.LogError("setboost in hp has been deleted");
		}
		/*if(collision.TryGetComponent<IcePuddleObject>(out var obj)) 
		{
			//attact speed increase
		}*/
		if (collision.TryGetComponent<Character>(out var target) && collision.gameObject != _dad.gameObject && collision.gameObject.layer != LayerMask.NameToLayer("Allies"))
			//&& enemyShadow)
		{
			float duration = 2 + _energyDad / 20;

			target.CharacterState.AddState(States.Frozen, duration, 0, _dad.gameObject, _skill.name);
			//GetComponent<Collider2D>().enabled = false;
			//Destroy(gameObject);
			if(_lastHit)
			{
				Collider[] enemyDetected = Physics.OverlapSphere(transform.position, 3);
				foreach (var enemy in enemyDetected) 
				{
					if (enemy.TryGetComponent<Character>(out var newTatget) && collision.gameObject != _dad.gameObject)
					{
						newTatget.CharacterState.AddState(States.Frozen, duration, 0, _dad.gameObject, _skill.name);
					}
				}
			}
			Explode();
		}
		//Explode();
	}

	public void Explode()
	{
		if (_hitEffect != null)
		{
			GameObject hitEffect = Instantiate(_hitEffect, transform.position, Quaternion.identity);
			Destroy(hitEffect, 5f);
		}

		//_healthPlayer.SetBoostRegen(0);
		//Debug.LogError("SetBoostRegen has been deleted");

		Destroy(gameObject);
	}

    #region Talent

	public void TalentDamage(bool value)
	{
		_talentDamage = value;
	}

	public void IceDeathInShadow(bool value)
    {
		_iceDeathInShadowTalent = value;
	}

    #endregion

    private IEnumerator DestroyShadow()
	{
		yield return new WaitForSeconds(timeToDestroy);
		Destroy(gameObject);
		//turn off energy boost
		//destroy	
	}

	private void MakeDamage()
	{
		_damageTimer -= Time.deltaTime;
		if(_damageTimer < 0 )
		{
			List<Character> _listToDamage;
			_listToDamage = GetCloserTargets(transform.position, 4);
			if(_listToDamage !=  null)
			for (int i = 0; i < _listToDamage.Count; i++) 
			{
				_listToDamage[i].Health.CmdTryTakeDamage( _damage, null);
			}
			_damageTimer = 1;
		}
	}

	public List<Character> GetCloserTargets(Vector3 position, float radius)
	{
		List<Character> targets = new List<Character>();
		Collider[] collider = Physics.OverlapSphere(position, radius);

		foreach (var item in collider)
		{
			if (collider.Length > 0 && item.transform.TryGetComponent<Character>(out Character enemy))
			{
				if (enemy.transform == _dad.Health.transform)
				{
					continue;
				}
				targets.Add(enemy);
			}
		}
		targets = targets.OrderBy(character => Vector3.Distance(character.transform.position, gameObject.transform.position)).ToList();

		if (targets.Count <= 0)
			return null;

		return targets;
	}
}
