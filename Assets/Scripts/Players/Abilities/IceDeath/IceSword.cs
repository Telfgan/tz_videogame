using Mirror;
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class IceSword : Skill
{
	[SerializeField] private float _damage = 15f;
	//[SerializeField] private GameObject _basePlayer;
	[SerializeField] private Character _playerLinks;
	[SerializeField] private DeathSpiral _deathSpiral;
	[SerializeField] private SeriesOfStrikes _seriesOfStrikes;
	[SerializeField] private GameObject _sword;
	[SerializeField] private AudioClip audioClip;


	private int _hitInTheRow = 0;
	private Character _oldtarget;
	//private Character _target;
	private float _duration = 3;
	private Energy _energy;
	private Coroutine coroutineSwordTime;
	private RuneComponent _rune;
	private bool _critDmg = false;
	private AudioSource _audioSource;
	protected override bool IsCanCast => IsCanCastCheck();

	protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerCast => Animator.StringToHash("IceSword");

	private bool IsCanCastCheck()
	{
		if (GetTargetCharacter() == null) return false;

		if (Vector3.Distance(GetTargetCharacter().transform.position, transform.position) > Radius)
		{
			return false;
		}
		return true;
	}

	private void Start()
	{
		for (int i = 0; i < _playerLinks.Resources.Count; i++)
		{
			if (_playerLinks.Resources[i].Type == ResourceType.Energy)
			{
				_energy = (Energy)_playerLinks.Resources[i];
			}
			if (_playerLinks.Resources[i].Type == ResourceType.Rune)
			{
				_rune = (RuneComponent)_playerLinks.Resources[i];
			}
		}

		_audioSource = GetComponent<AudioSource>();
	}

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        SetTarget((Character)targetInfo.GetTargets()[0]);
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
	{
		while (GetTargetCharacter() == null)
		{
			if (GetMouseButton)
			{
				FindTargetCharacter();
				//_target = GetRaycastTarget();
			}
			yield return null;
		}
		TargetInfo targetInfo = new TargetInfo();
		targetInfo.AddTarget(GetTargetCharacter());
		callbackDataSaved(targetInfo);
	}

	protected override IEnumerator CastJob()
	{
		_seriesOfStrikes.MakeHit(GetTargetCharacter(), AbilityForm.Magic, 0, 10, 0);
		if (GetTargetCharacter() == _oldtarget)
		{
			_hitInTheRow++;
			Debug.Log("hit from sword in a row");
		}
		else
		{
			_hitInTheRow = 1;
			_oldtarget = GetTargetCharacter();
			Debug.Log("first hit from sword");
		}
		if (_hitInTheRow > 2)
		{
			_deathSpiral.AddCharge();
			_hitInTheRow = 0;
		}
		ApplyDamage();
		CmdAdd(GetTargetCharacter().gameObject);
		yield return null;
	}

	protected override void ClearData()
	{
		ClearTarget();
		//_target = null;
	}

	private void ApplyDamage()
	{
		float energyBonus = Mathf.Min(_energy.CurrentValue, 10);
		_energy.CmdUse(energyBonus);

		float totalDamage = _damage + energyBonus;

		Damage damage2 = new Damage
		{
			Value = totalDamage,
			Type = DamageType.Physical,
			PhysicAttackType = AttackRangeType.RangeAttack,
		};

		if (_critDmg && GetTargetCharacter().CharacterState.CheckForState(States.Frozen))
		{
			damage2.Value *= (Random.Range(0, 100) < 15) ? 1.8f : 1.1f;
		}

		CmdApplyDamage(damage2, GetTargetCharacter().gameObject);

		_energy.SumDamageMake(damage2.Value);
		_rune.SumDamageMake(damage2.Value);
	}

	private IEnumerator ISwordTimer()
    {
		Coroutine currentCoroutine = coroutineSwordTime;
		yield return new WaitForSeconds(2.5f);

		if (currentCoroutine == coroutineSwordTime)
		{
			_sword.SetActive(false);
			coroutineSwordTime = null;
		}
	}

	[Command]
	private void CmdAdd(GameObject enemy)
	{
		Character enemyCharacter = enemy.GetComponent<Character>();
		RpcPlayShotSound();
		enemyCharacter.CharacterState.AddState(States.Cooling, _duration, 0, _playerLinks.gameObject, name);
	}

	[ClientRpc]
	private void RpcPlayShotSound()
	{
		if (_audioSource != null && audioClip != null) _audioSource.PlayOneShot(audioClip);
	}

	public void TalentCritDmg(bool value)
	{
		_critDmg = value;
	}

	public void CorutineSwordTimeStart()
	{
        if (coroutineSwordTime != null) StopCoroutine(coroutineSwordTime);
        coroutineSwordTime = StartCoroutine(ISwordTimer());
    }

	public void IceSwordCast()
	{
		AnimStartCastCoroutine();
	}

	public void IceSwordEnd()
	{
		AnimCastEnded();
	}
}
