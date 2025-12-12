using DG.Tweening;
using Mirror;
using System.Linq;
using UnityEngine;

public class DeathSpiralProjectile : Projectiles
{
	private IcyCorpse _icyCorpse;

	private Vector3 startPos;
	private bool _inTheRow = false;
	private bool _talentBoostHPBOdy = false;
	private bool _talentHitState = true;
	private bool _talentPlague = false;
	private bool _talentChragesPlague = false;
	private bool _talentCorpseDeath = false;
	private bool _talentSuperCharge = false;
	//private bool _talentCorpseDestroy;
	private bool _talentCorpseBoostExplode;
	private float _corpseHp = 10;
	private float _corpseMaxHp = 30;
	private Damage _damage;
	private float _curDamage;

	public void SetTarget(GameObject  target)
	{
		_rb.DOMove(target.transform.position, 0.5f);
	}

	private void Start()
	{
		if (_inTheRow)
		{
			_curDamage = 100;
		}
		else
		{
			_curDamage = 200;
		}
		_damage = new Damage
		{
			Value = _curDamage,
			Type = DamageType.Magical,
		};

		startPos = transform.position;
	}

	private void Update()
	{
		if (Vector3.Distance(transform.position, startPos) > _distance * GlobalVariable.cellSize)
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
		//damage, freez etc
		//if (collision.TryGetComponent<IDamageable>(out var damageable) && collision.gameObject != _dad.gameObject)
		if (collision.TryGetComponent<Character>(out var target))
		{
			/*if(damageable is IcyCorpse corpse)
			{
				var heal = new Heal { Value = 10 };
				corpse.Health.Heal(ref heal, name);
				Explode();
			}*/
			//else if (damageable is Character target)
			{
				TalentHit(target);
				Explode();
			}
			/*else
			{
				damageable.TryTakeDamage(ref _damage, _skill);
				if (_damage.Value <= 0)
				{
					Explode();
				}
				return;
			}*/
			Explode();
		}
		if (collision.TryGetComponent<IceShadowObject>(out var shadow))
		{
			if (_talentBoostHPBOdy)
			{
				_corpseHp = 20;
			}
			if (_inTheRow)
			{
				_corpseMaxHp = 40;
			}

			SetAlive(_corpseHp, shadow.transform, _corpseMaxHp);
			shadow.Explode();
			//Destroy(shadow.gameObject);

			Explode();
			Debug.Log(shadow.name + " become alive");
		}
		//if collision == ice puddle or ice shadow
		//
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

	public void Talents(bool boostHp, bool HitState, bool inTheRow, bool curse, bool charges, bool superCharge)
	{
		_talentBoostHPBOdy = boostHp;
		_talentHitState = HitState;
		_inTheRow = inTheRow;
		_talentPlague = curse;
		_talentChragesPlague = charges;
		_talentSuperCharge = superCharge;
	}
	public void Talents(bool destroy, bool boostExplode)
	{
		_talentCorpseDeath = destroy;
		_talentCorpseBoostExplode = boostExplode;
	}

	public void SetAlive(float hp, Transform transform, float maxHp)
	{
		_dad.SpawnComponent.SpawnUnit(0, transform.position);
		/*_icyCorpse =  (IcyCorpse)_dad.SpawnComponent.Units.Last();
		_icyCorpse.InitWithHp(hp, maxHp);
		_icyCorpse.Talents(_talentCorpseDeath, _talentCorpseBoostExplode);*/
		Explode();
	}

	private void TalentHit(Character target)
	{
		if (_inTheRow)
		{
			//_skill.CmdApplyDamage(damage, target.gameObject);
			target.Health.TryTakeDamage(ref _damage, _skill);
			if (_talentPlague)
			{
				target.CharacterState.AddState(States.Plague, 5, 0, _dad.gameObject, _skill.name);
			}
			if(_talentChragesPlague)
			{
				//target.CharacterState.personWhoShoted = _dad;
			}
			if(_talentSuperCharge)
			{
				target.CharacterState.AddState(States.Curse, 40, 0, _dad.gameObject, _skill.name);
			}
			if (_talentHitState)
			{
				StateTalent(target, 10);
			}
		}
		else
		{
			//_skill.CmdApplyDamage(damage, target.gameObject);
			target.Health.TryTakeDamage(ref _damage, _skill);
			if (_talentPlague)
			{
				target.CharacterState.AddState(States.Plague, 5, 0, _dad.gameObject, _skill.name);
			}
			if (_talentChragesPlague)
			{
				//target.CharacterState.personWhoShoted = _dad;
			}
			if (_talentSuperCharge)
			{
				target.CharacterState.AddState(States.Curse, 40, 0, _dad.gameObject, _skill.name);
			}
			if (_talentHitState)
			{
				StateTalent(target, _damage.Value);
			}
		}
		TargetRpcDamgeMake(_curDamage);
	}

	private void StateTalent(Character target, float damage)
	{
		Debug.Log("ENTERed talent state hit");
		if (target.CharacterState.CheckForState(States.Frozen))
		{
			target.CharacterState.RemoveState(States.Frozen);
			target.CharacterState.AddState(States.Frosting, 40, 0, _dad.gameObject, _skill.name);

			Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 4);

			foreach (Collider2D collider in colliders)
			{
				if (collider.TryGetComponent<Character>(out var enemy) &&  collider.gameObject != target.gameObject ) //collider.gameObject != _dad &&)
				{
					Damage damage2 = new Damage
					{
						Value = damage/2,
						Type = DamageType.Magical,
					};

					//_skill.CmdApplyDamage(damage, target.gameObject);
					target.Health.TryTakeDamage(ref damage2, _skill);
					enemy.CharacterState.AddState(States.Frosting, 40, 0, _dad.gameObject, _skill.name);
				}
			}
		}
		else if (target.CharacterState.CheckForState(States.Frosting))
		{
			target.CharacterState.RemoveState(States.Frosting);
			for (int i = 0; i < 5; i++)
			{
				target.CharacterState.AddState(States.Cooling, 4, 0, _dad.gameObject, _skill.name);
			}

			Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 4);

			foreach (Collider2D collider in colliders)
			{
				if (collider.TryGetComponent<Character>(out var enemy) && collider.gameObject != _dad)
				{
					Damage damage2 = new Damage
					{
						Value = damage/2,
						Type = DamageType.Magical,
					};
					//_skill.CmdApplyDamage(damage, target.gameObject);
					target.Health.TryTakeDamage(ref damage2, _skill);
					for (int i = 0; i < 5; i++)
					{
						enemy.CharacterState.AddState(States.Cooling, 4, 0, _dad.gameObject, _skill.name);
					}
				}
			}
		}
	}

	/*internal void Talents(bool talentBoostHPBOdy, bool talentHitState, bool inTheRow, object talentPlague)
	{
		throw new NotImplementedException();
	}*/
}
