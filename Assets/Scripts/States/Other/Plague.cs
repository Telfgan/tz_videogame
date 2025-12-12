using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plague : AbstractCharacterState
{
	private int _stack = 0;
	private float _durationToExit = 0f;
	private float _damageTimer = 1f;
	public int GetStack => _stack;
	public override States State => States.Plague;
	public override StateType Type => StateType.Magic;
	public override List<StatusEffect> Effects => new List<StatusEffect>();
	public override BaffDebaff BaffDebaff => BaffDebaff.Baff;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
	{
		_characterState = character;
		_durationToExit = durationToExit;
		_health = _characterState.Character.Health;
		_abilities = character.Character.Abilities;

		for (int i = 0; i < _abilities.Abilities.Count; i++)
		{
			_abilities.Abilities[i].Buff.Damage.ReductionPercentage(0.05f);
		}
		// reduce damage given
	}

	public override void UpdateState()
	{
		_durationToExit -= Time.deltaTime;
		_damageTimer -= Time.deltaTime;
		GameObject obj = null;
		if (_damageTimer <= 0)
		{
			int damage = Random.Range(1, 4);

			//MakeDamage(damage, _characterState.gameObject);

			Collider2D[] enemyDetected = Physics2D.OverlapCircleAll(_characterState.transform.position, 5);
			foreach (var enemy in enemyDetected)
			{
				if (enemy.TryGetComponent<Character>(out var enemyCharacter) && enemy != _characterState.gameObject)
				{
					obj = enemy.gameObject;
					Debug.Log(enemy);
					MakeDamage(damage, enemy.gameObject);
				}
			}
			if (obj != null)
				MakeDamage(damage, obj);


			/*if (Random.Range(0, 100) < 50 && _personWhoMadeBuff != null)
			{
				/*DeathSpiral deathSpiral = (DeathSpiral)_characterState.personWhoShoted.Abilities.GetAbilityByName("DeathSpiral");
				if(deathSpiral != null) 
				{
					Debug.Log("ADD CHRAGE");
					deathSpiral.AddCharge();
				}
			}*/

			if (Random.Range(0, 5) < 1)
			{
				AddState();
			}
			_damageTimer = 10;
			//20% chance of inflicting close enemy
		}
		if (_durationToExit <= 0)
		{
			ExitState();
		}
	}

	public override void ExitState()
	{
		// return reduced damage given
		for (int i = 0; i < _abilities.Abilities.Count; i++)
		{
			_abilities.Abilities[i].Buff.Damage.IncreasePercentage(0.05f);
		}
		_characterState.RemoveState(this);
	}

	public override bool Stack(float time)
	{
		if (_stack <= 4)
		{
			_durationToExit = time;
			_stack++;
			return true;
		}
		else
		{
			_durationToExit = time;
			return true;
		}
	}

	[Command]
	private void MakeDamage(float damages, GameObject gm)
	{
		Damage damage = new Damage
		{
			Value = damages,
			Type = DamageType.Magical,
			PhysicAttackType = AttackRangeType.RangeAttack,
		};
		//_characterState.Character.Health.TryUse(10);
		Character charac = gm.gameObject.GetComponent<Character>();
		charac.Health.TryUse(10);
	}

	[Server]
	private void AddState()
	{
		Collider2D[] colliders = Physics2D.OverlapCircleAll(_characterState.gameObject.transform.position, 3);

		foreach (Collider2D collider in colliders)
		{
			if (collider.TryGetComponent<Character>(out var enemy) && collider.gameObject != _characterState.gameObject)
			{
				//enemy.Health.TryTakeDamage(damage / 2, DamageType.Magical, AttackRangeType.RangeAttack);
				enemy.CharacterState.CmdAddState(States.Plague, 4, 0, null, null);
			}
		}
	}
}
