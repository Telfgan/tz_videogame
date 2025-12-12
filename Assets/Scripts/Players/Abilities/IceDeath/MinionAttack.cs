using UnityEngine;

public class MinionAttack : AutoAttackSkill
{
	[SerializeField] private float _damage = 3f;	
	[SerializeField] private Character _dad;
	[SerializeField] private float _abilityCooldown = 1.6f; //cooldown between shots
	private float _cooldownTimer = 1.6f;
	private bool _isReadyToShot = true;
	private float _speedBoost = 1;

    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerAutoAttack => throw new System.NotImplementedException();

    private void CheckEnemy()
	{
        if (!_isReadyToShot)
        {
			return;
        }
        Collider2D[] enemyDetected = Physics2D.OverlapCircleAll(transform.position, Radius);

		foreach (Collider2D col in enemyDetected)
		{
			if(col.TryGetComponent<Character>(out var enemy))
			{
				if (enemy == _dad)
				{
					continue;
				}				
				//Debug.Log("Enemy detected: " + enemy.gameObject.name);
				Hit(enemy);
				break;
			}			
		}
	}
	private void Hit(Character enemy)
	{
		_isReadyToShot = false;

		//enemy.Health.TakeDamage(_damage + Random.Range(0, 1), DamageType.Physical, this);
		Debug.LogError("!!!damage method has been changed!!!");
	}

	public void Timer()
	{
		if(_cooldownTimer > 0 && !_isReadyToShot) 
		{
			_cooldownTimer -= Time.deltaTime;
		}
		else
		{
			_isReadyToShot = true;
			_cooldownTimer = _abilityCooldown;
		}
	}

	protected override void CastAction()
	{
		Damage damage = new Damage
		{
			Value = 1 + Random.Range(0, 2),
			Type = DamageType.Physical,
			PhysicAttackType = AttackRangeType.MeleeAttack,
		};
		GetTargetCharacter().Health.TryTakeDamage(ref damage, this);
	}

	public void TalentBoostSpeed(float speed)
	{
		_speedBoost *= speed;
	}
	public void TalentReduceSpeed(float speed)
	{
		_speedBoost /= speed;
	}

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        throw new System.NotImplementedException();
    }
}
