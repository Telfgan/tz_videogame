using Mirror;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IceCloud : Skill
{
	[SerializeField] private IceCloudProjectile _projectile;
	[SerializeField] private SeriesOfStrikes _combo;
	[SerializeField] private AudioClip audioClip;

	private Vector3 _mousePos = Vector3.positiveInfinity;

	//private bool _enabled;
	private AudioSource _audioSource;
	private bool _boostDmg;
	private bool _lastHit;
	private Energy _energy;
	private bool _frozwenTalent;
	//private Character _target;

	//private RuneComponent _rune;

	protected override bool IsCanCast
	{
		get
		{
			if (GetTargetCharacter() != null) return Vector3.Distance(GetTargetCharacter().transform.position, transform.position) <= Radius;

			else return true;
		}
	}

	protected override int AnimTriggerCastDelay => 0;

	protected override int AnimTriggerCast => Animator.StringToHash("IceCloud");

    private void OnDestroy()
    {
        OnSkillCanceled -= HandleSkillCanceled;
    }

    private void OnEnable()
    {
        OnSkillCanceled += HandleSkillCanceled;
    }

    private void HandleSkillCanceled()
    {
		CanMoveIceCloud();
		ClearTarget();
		//_target = null;
		_mousePos = Vector3.positiveInfinity;
	}

    private void Start()
	{
		_audioSource = GetComponent<AudioSource>();

		for (int i = 0; i < Hero.Resources.Count; i++)
		{
			if (Hero.Resources[i].Type == ResourceType.Energy)
			{
				_energy = (Energy)Hero.Resources[i];
			}
			/*if (_playerLinks.Resources[i].Type == ResourceType.Rune)
			{
				_rune = (RuneComponent)_playerLinks.Resources[i];
			}*/
		}
	}

	private void Shoot()
	{
		Buff.AttackSpeed.ReductionPercentage(1 + _combo.GetMultipliedSpeed() / 100);

		Vector3 lookDir = _mousePos - Hero.transform.position;
		float angle = Mathf.Atan2(lookDir.z, lookDir.x) * Mathf.Rad2Deg - 90f;
		if (_combo.MakeHit(null, AbilityForm.Magic, 1, 0, 0)) _lastHit = true;

		Buff.AttackSpeed.IncreasePercentage(1 + _combo.GetMultipliedSpeed() / 100);

		CmdCreateProjecttile(angle, _energy.CurrentValue);
		ClearTarget();
		_mousePos = Vector2.positiveInfinity;
		ClearData();
	}

	[Command]
	private void CmdCreateProjecttile(float angle, float manaValue)
	{
		IceCloudProjectile projectile = Instantiate(_projectile, gameObject.transform.position, Quaternion.Euler(0, -angle, 0));
		SceneManager.MoveGameObjectToScene(projectile.gameObject, _hero.NetworkSettings.MyRoom);
		projectile.Init(Hero, manaValue, false, this);
		projectile.Talent(_boostDmg, _frozwenTalent, _lastHit);

		NetworkServer.Spawn(projectile.gameObject);

		RpcPlayShotSound();
		RpcInit(projectile.gameObject, manaValue);
	}

	[ClientRpc]
	private void RpcInit(GameObject obj, float manaValue)
	{
		obj.GetComponent<IceCloudProjectile>().Init(Hero, manaValue, false, this);
	}

	[ClientRpc]
	private void RpcPlayShotSound()
	{
		if (_audioSource != null && audioClip != null) _audioSource.PlayOneShot(audioClip);
	}

	public void TalentBoostDmg(bool value)
	{
		_boostDmg = value;
	}

	public void TalentBoostFrozenState(bool value)
	{
		_frozwenTalent = value;
	}

	public override void LoadTargetData(TargetInfo targetInfo)
	{
		if (targetInfo.Points.Count > 0) _mousePos = targetInfo.Points[0];
		if (targetInfo.GetTargets().Count > 0 && targetInfo.GetTargets()[0] is Character character) SetTarget(character);
	}

	protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
	{
		while (float.IsPositiveInfinity(_mousePos.x))
		{
			if (GetMouseButton)
			{
				if(GetTargetCharacter() == null) yield return null;
				if (GetTargetCharacter() != null)
				{
					float distance = Vector3.Distance(_hero.transform.position, _mousePos);

					if (distance <= Radius) _mousePos = GetTargetCharacter().transform.position;

					else
					{
						//FindTarget();

						//_target = GetTarget().character;
						_damageValue = 10 + _energy.CurrentValue / 5;
						_mousePos = GetTargetCharacter().transform.position;
					}
				}

				else _mousePos = GetMousePoint();
			}
			yield return null;
		}

		TargetInfo targetInfo = new TargetInfo();
		if (GetTargetCharacter() != null) targetInfo.Points.Add(GetTargetCharacter().Position);
		else if (_mousePos != Vector3.positiveInfinity) targetInfo.Points.Add(_mousePos);
		callbackDataSaved(targetInfo);
	}

	protected override IEnumerator CastJob()
	{
		Shoot();
		yield return null;
	}

	protected override void ClearData()
	{
		ClearTarget();
		_mousePos = Vector2.positiveInfinity;
		//_enabled = false;
	}

	public void IceCloudsCast()
	{
		AnimStartCastCoroutine();
	}

	public void IceCloudsEnd()
	{
		AnimCastEnded();
	}

	public void CanMoveIceCloud() => Hero.Move.CanMove = true;
	public void StopMoveIceCloud() => Hero.Move.CanMove = false;
}