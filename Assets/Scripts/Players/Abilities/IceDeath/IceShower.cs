using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IceShower : Skill
{
	[SerializeField] private IceShowerProjectile _projectile;
	[SerializeField] private SkillRenderer _skillRenderer;
	[SerializeField] private HeroComponent _playerLinks;
	[SerializeField] private SeriesOfStrikes _combo;

	private Vector3 _targetPoint = Vector3.positiveInfinity;
	private Energy _energy;
	private float _duration = 2;
	private float _damageToexit = 1;
	private bool _frozwenTalent;
	private bool _boostDmg;

	protected override bool IsCanCast => true;

	protected override int AnimTriggerCastDelay => 0;

	protected override int AnimTriggerCast => 0;

	private void Start()
	{
		for (int i = 0; i < _playerLinks.Resources.Count; i++)
		{
			if (_playerLinks.Resources[i].Type == ResourceType.Energy)
			{
				_energy = (Energy)_playerLinks.Resources[i];
			}
			/*if (_playerLinks.Resources[i].Type == ResourceType.Rune)
			{
				_rune = (RuneComponent)_playerLinks.Resources[i];
			}*/
		}
		
	}

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        _targetPoint = targetInfo.Points[0];
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
	{
		while (float.IsPositiveInfinity(_targetPoint.x))
		{
			if (GetMouseButton && IsCanCast)
			{
				Vector3 clickedPoint = GetMousePoint();

				if (IsPointInRadius(Radius, clickedPoint))
				{
					_targetPoint = clickedPoint;
				}
			}
			yield return null;
		}
		TargetInfo targetInfo = new();
		targetInfo.Points.Add(_targetPoint);
		callbackDataSaved(targetInfo);
	}

	protected override IEnumerator CastJob()
	{
		//DrawDamageZone(_targetPoint);

		//ApplyDamageToEnemiesInZone();
		//StopDamageZone();
		Shoot();
		yield return null;
	}

	private void Shoot()
	{
		Buff.AttackSpeed.ReductionPercentage(1 + _combo.GetMultipliedSpeed() / 100);

		//Vector3 lookDir = _mousePos - _playerLinks.transform.position;
		//float angle = Mathf.Atan2(lookDir.z, lookDir.x) * Mathf.Rad2Deg - 90f;
		if (_combo.MakeHit(null, AbilityForm.Magic, 1, 0, 0))
		{
			Debug.LogError("some talents i guess in ice cloud");
			//_playerLinks.RuneComponent.IceCloudBonus();
		}

		Buff.AttackSpeed.IncreasePercentage(1 + _combo.GetMultipliedSpeed() / 100);
		_targetPoint.y += 5;
		CmdCreateProjecttile(_targetPoint, _energy.CurrentValue);
		ClearData();
	}

	[Command]
	private void CmdCreateProjecttile(Vector3 position, float manaValue)
	{
		IceShowerProjectile projectile = Instantiate(_projectile, position, Quaternion.Euler(0, 0, 0));
		SceneManager.MoveGameObjectToScene(projectile.gameObject, _hero.NetworkSettings.MyRoom);
		projectile.Init(_playerLinks, manaValue, false, this);
		projectile.Talent(_boostDmg, _frozwenTalent);

		NetworkServer.Spawn(projectile.gameObject);

		RpcInit(projectile.gameObject, manaValue);
	}

	[ClientRpc]
	private void RpcInit(GameObject obj, float manaValue)
	{
		obj.GetComponent<IceShowerProjectile>().Init(_playerLinks, manaValue, false, this);
	}

	/*private void ApplyDamageToEnemiesInZone()
	{
		//CircleArea damageZone = _skillRenderer.TempDamageZone;

		//if (damageZone != null)
		{
			Debug.Log("TEST");
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			Vector3 worldPos = transform.position;

			if (Physics.Raycast(ray, out hit))
			{
				worldPos = hit.point;
			}
			Collider[] hitColliders = Physics.OverlapSphere(worldPos, Area, TargetsLayers);

			foreach (var hitCollider in hitColliders)
			{
				HeroComponent enemy = hitCollider.GetComponent<HeroComponent>();
				if (enemy != null)
				{
					_damageValue = 10 + _energy.CurrentValue / 4;
					ApplyDamage(_damageValue, DamageType.Magical, enemy);

					var targetState = enemy.CharacterState;
					if (targetState != null)
					{
						_duration = 100 + _energy.CurrentValue / 20;
						_damageToexit +=  _damageValue;
						Debug.Log(_damageToexit + " damageTo Exit");
						CmdAddState(targetState, _duration, _damageToexit);
					}
				}
			}
		}
	}

	[Command]
	private void CmdAddState(CharacterState targetState, float duration, float damageToExit)
	{
		Debug.Log(damageToExit + " damageTo Exit");
		targetState.AddState(States.Frozen, duration, damageToExit, Hero.gameObject, this.name);

	}

	private float CalculateDamage(float baseDamage)
	{
		bool isCriticalHit = Random.Range(0f, 100f) <= criticalChance;

		if (isCriticalHit)
		{
			return baseDamage * CriticalMultiplier;
		}

		return baseDamage;
	}

	private void ApplyDamage(float damage, DamageType damageType, Character target)
	{
		Damage _damage = new Damage
		{
			Value = damage,
			Type = DamageType.Physical,
			PhysicAttackType = AttackRangeType.RangeAttack,
		};
		Debug.Log("DAMAGE");
		CmdApplyDamage(_damage, target.gameObject);
	}*/

	protected override void ClearData()
	{
		_targetPoint = Vector3.positiveInfinity;
	}

	/*public void TalentBoostFrozenState(bool value)
	{
		if(value)
		{
			_damageToexit = 30;
		}
        else
        {
			_damageToexit = 1;
        }
    }*/

	public void TalentBoostDmg(bool value)
	{
		_boostDmg = value;
	}

	public void TalentBoostFrozenState(bool value)
	{
		_frozwenTalent = value;
	}
}