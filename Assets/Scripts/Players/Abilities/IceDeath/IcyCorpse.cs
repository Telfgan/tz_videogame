using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IcyCorpse : MinionComponent
{
	[SerializeField] private PlagueCloud _cloud;
    private bool _talentDestroy = false;
	private bool _talentBoostExplode = false;
	private float _hp = 10;
	private float _maxHp = 30;

	private void Start()
	{
		Health.SetHp(_hp, _maxHp);
	}
	public void InitWithHp(float hp, float maxHp)
	{
		_hp = hp;
		_maxHp = maxHp;
	}

    public void DestroyCorpse()
    {
        if(_talentDestroy) 
        {		
			//_dad = _heroParent.GetComponent<Character>();
			Collider2D[] colliders = Physics2D.OverlapCircleAll(gameObject.transform.position, 3);
			float damage = 10;
			if(_talentBoostExplode && Random.Range(0, 10) < 3)
			{
				damage *= 3;
			}
			foreach (Collider2D collider in colliders)
			{
				if (collider.TryGetComponent<Character>(out var enemy) && collider.gameObject != gameObject && collider.gameObject != _myHeroParent.gameObject)
				{
					Damage damage2 = new Damage
					{
						Value = damage / 2,
						Type = DamageType.Magical,
						PhysicAttackType = AttackRangeType.RangeAttack,
					};
					//_skill.CmdApplyDamage(damage, target.gameObject);
					enemy.Health.TryTakeDamage(ref damage2, null);
					//enemy.Health.TryTakeDamage(damage, DamageType.Magical, AttackRangeType.RangeAttack);
					if (Random.Range(0, 3) < 1)
					{
						enemy.CharacterState.CmdAddState(States.Plague, 4, 0, this.gameObject, name);
					}
				}
			}
			if (_myHeroParent != null)
			{
				Debug.Log("create cloud");
				Shoot();
			}
			
		}
		Destroy(gameObject);
		// _heroParent.
	}

	//[Command]
	private void Shoot()
	{
		PlagueCloud projectile = Instantiate(_cloud, gameObject.transform.position, Quaternion.Euler(0, 0, 0));
		SceneManager.MoveGameObjectToScene(projectile.gameObject, _myHeroParent.NetworkSettings.MyRoom);
		projectile.Init(_myHeroParent, 0, false, null);
		
		//projectile.TalentBoostHp(_talentBoostHPBOdy);
		//projectile.TalentHitState(_talentHitState);

		NetworkServer.Spawn(projectile.gameObject);

		//RpcInit(projectile.gameObject);
	}

	[ClientRpc]
	private void RpcInit(GameObject obj)
	{
		obj.GetComponent<DeathSpiralProjectile>().Init(_myHeroParent, 0, false, null);
	}

	public void Talents(bool destroy, bool boostExplode)
	{
		_talentDestroy = destroy;
		_talentBoostExplode = boostExplode;
	}
}
