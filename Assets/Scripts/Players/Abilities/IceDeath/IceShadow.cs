using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IceShadow : Skill
{
	[Header("Ability properties")]
	[SerializeField] private IceShadowObject _shadow;
	[SerializeField] private HeroComponent _playerLinks; 
	[SerializeField] private SeriesOfStrikes _combo;
	[SerializeField] private AudioClip audioClip;
	//[SerializeField] private bool isTest = true;

	private AudioSource _audioSource;
	private Energy _energy;
	//private RuneComponent _rune;
	private bool _lastHit = false;
	private bool _talentEvade = false;
	private bool _talentDamage = false;
	private bool _iceDeathInShadowTalent = false;
	private bool _evaded = false;
	private float _evadedTimer = 2f;
	private float _manaUsed = 0;

	protected override bool IsCanCast => IsCanCastCheck();

    protected override int AnimTriggerCastDelay => 0;

    protected override int AnimTriggerCast => 0;

    private bool IsCanCastCheck()
	{
		return true;
		/*if (_rune.CurrentValue >= 1)
		{
			_rune.CmdUse(1);
			return true;
		}
		else
		{
			return false;
		}*/
	}
	private void Start()
	{
		_audioSource = GetComponent<AudioSource>();

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

	private void OnEnable()
	{
		_playerLinks.Health.Evaded += Evaded;
	}

	private void OnDestroy()
	{
		_playerLinks.Health.Evaded -= Evaded;
	}
    public override void LoadTargetData(TargetInfo targetInfo)
    {
		if (targetInfo == null) return;
		if (targetInfo.GetTargets().Contains(Hero)) return;
		targetInfo.AddTarget(Hero);
	}

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
	{
		TargetInfo targetInfo = new TargetInfo();
		targetInfo.AddTarget(Hero);
		callbackDataSaved(targetInfo);
		yield return null;
	}

	protected override IEnumerator CastJob()
	{
		Shoot();
		yield return null;
	}

	protected override void ClearData()
	{
		
	}

	private void Shoot()
	{
		Buff.AttackSpeed.ReductionPercentage(1 + _combo.GetMultipliedSpeed() / 100);
		/*IceShadowObject projectileGm = Instantiate(_shadow, gameObject.transform.position, Quaternion.identity);
		projectileGm.Init(_playerLinks.gameObject ,Mana.Value);*/
		_lastHit = _combo.MakeHit(null, AbilityForm.Magic, 1, _manaUsed, 0);

		Buff.AttackSpeed.IncreasePercentage(1 + _combo.GetMultipliedSpeed() / 100);

		_manaUsed = _energy.CurrentValue;
		_energy.CmdUse(_manaUsed);
		CmdCreateProjecttile(0, _manaUsed, _lastHit, _talentDamage, _iceDeathInShadowTalent);
	}

	[Command]
	private void CmdCreateProjecttile(float angle, float manaValue, bool lastHit, bool damage, bool inShadow)
	{
		AnimatorStateInfo stateInfo = _playerLinks.Animator.GetCurrentAnimatorStateInfo(0);
		int animationHash = stateInfo.fullPathHash;
		float normalizedTime = stateInfo.normalizedTime % 1f;
		float velocityX = _playerLinks.Animator.GetFloat(HashAnimPlayer.VelocityX);
		float velocityZ = _playerLinks.Animator.GetFloat(HashAnimPlayer.VelocityZ);
		Quaternion rotation = _playerLinks.transform.rotation;

		Vector3 basePosition = _playerLinks.transform.position;

		if (lastHit)
		{
			Vector3 right = _playerLinks.transform.right;
			Vector3 left = -_playerLinks.transform.right;
			Vector3 forward = _playerLinks.transform.forward;

			Vector3 offsetRight = basePosition + right;
			Vector3 offsetLeft = basePosition + left;
			Vector3 centerPosition = basePosition + forward;

			SpawnShadow(offsetRight, rotation, manaValue, lastHit, damage, inShadow, animationHash, normalizedTime, velocityX, velocityZ);
			SpawnShadow(offsetLeft, rotation, manaValue, lastHit, damage, inShadow, animationHash, normalizedTime, velocityX, velocityZ);
			SpawnShadow(centerPosition, rotation, manaValue, lastHit, damage, inShadow, animationHash, normalizedTime, velocityX, velocityZ);
		}

		else SpawnShadow(basePosition, rotation, manaValue, lastHit, damage, inShadow, animationHash, normalizedTime, velocityX, velocityZ);

		RpcPlayShotSound();
	}

	private void SpawnShadow(Vector3 position, Quaternion rotation, float manaValue, bool lastHit, bool damage, bool inShadow,
		int animationHash, float normalizedTime, float velocityX, float velocityZ)
	{
		IceShadowObject shadow = Instantiate(_shadow, position, rotation);
		SceneManager.MoveGameObjectToScene(shadow.gameObject, _hero.NetworkSettings.MyRoom);
		shadow.Init(_playerLinks, manaValue, lastHit, this);
		shadow.TalentDamage(damage);

		NetworkServer.Spawn(shadow.gameObject);
		RpcSetShadowAnimation(shadow.gameObject, animationHash, normalizedTime, velocityX, velocityZ, rotation);
		RpcInit(shadow.gameObject, manaValue, lastHit, damage, inShadow);
	}

	[ClientRpc]
	private void RpcSetShadowAnimation(GameObject shadowObj, int animationHash, float normalizedTime, float velocityX, float velocityZ, Quaternion rotation)
	{
		if (shadowObj.TryGetComponent(out IceShadowObject shadow))
		{
			shadow.SetAnimationState(animationHash, normalizedTime, velocityX, velocityZ, rotation);
		}
	}

	[ClientRpc]
	private void RpcInit(GameObject obj, float manaValue, bool lastHit, bool damage, bool inShadow)
	{
		obj.GetComponent<IceShadowObject>().Init(_playerLinks, manaValue, lastHit, this);
		obj.GetComponent<IceShadowObject>().TalentDamage(damage);
		obj.GetComponent<IceShadowObject>().TalentDamage(inShadow);
	}

	[ClientRpc]
	private void RpcPlayShotSound()
	{
		if (_audioSource != null && audioClip != null) _audioSource.PlayOneShot(audioClip);
	}

    #region Talent

    public void TalentEvade(bool value)
	{
		_talentEvade = value;
	}

	public void TalentDamage(bool value)
	{
		_talentDamage = value;
	}

	public void IceDeathInShadowTalentActive(bool value, string text)
    {
		_iceDeathInShadowTalent = value;
		AbilityInfoHero.FinalDescription = value ? AbilityInfoHero.Description + $" {text}" : AbilityInfoHero.Description;

	}

    #endregion

    public void Evaded()
	{
		if( _talentEvade) 
		{
			_evaded = true;
			StartCoroutine(CountDownToTalentEvede());
		}
	}
		
	private IEnumerator CountDownToTalentEvede()
	{
		yield return new WaitForSeconds(_evadedTimer);
		_evaded = false;
	}

	protected override bool TryPayCost(List<SkillEnergyCost> skillEnergyCosts, bool startCooldown = true)
	{
		if (IsHaveResourceOnSkill)
		{
			if (_evaded && _talentEvade)
			{
				/*foreach (var skillCost in _skillEnergyCosts)
				{
					var resource = _hero.Resources.First(r => r.Type == skillCost.resourceType);
					resource.CmdUse(Buff.ManaCost.GetBuffedValue(skillCost.resourceCost));
				}*/
				_evaded = false;
			}
			else
			{
				foreach (var skillCost in _skillEnergyCosts)
				{
					var resource = _hero.Resources.First(r => r.Type == skillCost.resourceType);
					resource.CmdUse(Buff.ManaCost.GetBuffedValue(skillCost.resourceCost));
				}
				_evaded = false;
			}

			if (startCooldown)
				IncreaseSetCooldown(CooldownTime);

			TryUseCharge();
			return true;
		}
		else
		{
			return false;
		}
	}
}

