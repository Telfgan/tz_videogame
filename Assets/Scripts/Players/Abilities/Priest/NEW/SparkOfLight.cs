using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SparkOfLight : Skill
{
    [Header("Spark Of Light Settings")]
    [SerializeField] private float _buffDuration = 9f;
    [SerializeField] private float _healAmount = 2f;
    [SerializeField] private float _damageAmount = 2f;
    [SerializeField] private float _castTime = 0.8f;
    [SerializeField] private float _range = 4f;

	//override TryPayCost and remove this two things
	[SerializeField] private List<SkillEnergyCost> _manaCostHeal;
    [SerializeField] private List<SkillEnergyCost> _manaCostDamage;
    [SerializeField] private AbilityInfo lightInfo;

    [Header("Alternative Mode Settings")]
    [SerializeField] private float _altRange = 6f;
    [SerializeField] private float _altBuffDuration = 5f;
    [SerializeField] private float _altDamageAmount = 2f;
    [SerializeField] private List<SkillEnergyCost> _altManaCostDamage;
    [SerializeField] private FlashOfLight _flashOfLight;
    [SerializeField] private AbilityInfo darkInfo;

    [SerializeField] private LightSparkProjectile lightSparkProjectile;
    [SerializeField] private LightSparkProjectile darkSparkProjectile;
    [SerializeField] private HeroComponent playerLinks;
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private StunMagicPassiveSkill stunMagicPassiveSkill;

    private AudioSource _audioSource;
    private bool _spiritEnergyAddTalent;

    [SyncVar(hook = nameof(OnModeChanged))] public bool isLightMode = true;

    private bool _healthBoostActive = false;
    private bool _lowHealthTalentActive = false;
    private bool _manaRestoreBoostTalent = false;
    private bool _healingBuffTalentActive = false;
    private bool _spiritEnergyTalent;

    private const float LowHealthThreshold = 0.25f;
    private const float BonusDamageMultiplier = 1.25f;
    private const float HealthBoostPercentage = 0.25f;
    private const float HealthBoostDuration = 2f;
    private const float DefenseReductionPercentage = 0.25f;
    private const float DefenseDebuffDuration = 2f;

    private float _healingBuffDuration = 5f;
    private float _tickHealingBonus = 2f;
    private int _healingBonusStacks = 0;
    private float _lastFlashOfLightCastTime = 0f;

    //protected IDamageable _target;
    //private Character _characterTarget;

    public void EnableTalentPhysicalShieldBoost(bool value) => _healthBoostActive = value;
    public void EnableLowHealthTalent(bool value) => _lowHealthTalentActive = value;
    public void EnableManaRestoreBoostTalent(bool value) => _manaRestoreBoostTalent = value;
    public void EnableHealingBuffTalent(bool value) => _healingBuffTalentActive = value;


    private bool IsAllyTarget(Character target) => target.gameObject.layer == LayerMask.NameToLayer("Allies");
    private bool IsEnemyTarget(Character target) => target.gameObject.layer == LayerMask.NameToLayer("Enemy");

    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerCast => Animator.StringToHash("SparkOfLights");

    protected override bool IsCanCast => GetTargetCharacter() != null && Vector3.Distance(GetTargetCharacter().transform.position, transform.position) <= Radius && NoObstacles(GetTargetCharacter().transform.position, transform.position, _obstacle);

    public event Action OnModeChange;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        _flashOfLight.CastEnded += HandleLastTimeFlashOfLightCast;
        OnModeChange += UpdateMode;
        UpdateMode();
    }

    private void OnDisable()
    {
        _flashOfLight.CastEnded -= HandleLastTimeFlashOfLightCast;
        OnModeChange -= UpdateMode;
    }

    public void SwitchMode()
    {
        CmdSwitchMode();
    }

    private void OnModeChanged(bool oldValue, bool newValue)
    {
        UpdateMode();
        OnModeChange?.Invoke();
    }

    private void HandleLastTimeFlashOfLightCast()
    {
        _lastFlashOfLightCastTime = Time.time;
    }

    private void UpdateMode()
    {
        School = isLightMode ? Schools.Light : Schools.Dark;
        AbilityInfoHero = isLightMode ? lightInfo : darkInfo;

        ClearData();
        ClearTarget();
        //_characterTarget = null;
    }

    public void HandleMode(Character target)
    {
        if (isLightMode) HandleDefaultMode(target);
        else HandleAlternativeMode(target);
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {

        while (GetTargetCharacter() == null)
        {
            if (GetMouseButton)
            {
                FindTargetCharacter();

                if (GetTargetCharacter() is Character character)
                {
                    //_characterTarget = character;

                    if (GetTargetCharacter() != null && (IsAllyTarget(character) || character == Hero) && !isLightMode)
                    {
                        ClearTarget();
                    }

                    if (GetTargetCharacter() != null)
                    {
                        character.SelectedCircle.IsActive = true;
                    }
                }
            }

            yield return null;
        }

        TargetInfo targetInfo = new TargetInfo();
        targetInfo.Points.Add(GetTargetCharacter().transform.position);
        targetInfo.AddTarget(GetTargetCharacter());
        callbackDataSaved(targetInfo);
    }

    protected override IEnumerator CastJob()
    {
        if (GetTargetCharacter() == null) yield break;

        if (!IsCanCast)
        {
            //TryPayCost(_manaCostHeal);
            //CmdHandleDefaultMode(playerLinks);
            yield break;
        }

        if (IsAllyTarget(GetTargetCharacter()))
        {
            //TryPayCost(_manaCostHeal);

            if (GetTargetCharacter() == playerLinks) CmdHandleDefaultMode(playerLinks);
            else CmdSpawnProjectile(GetTargetCharacter());

            yield break;
        }

        if (IsEnemyTarget(GetTargetCharacter()))
        {
            //TryPayCost(isLightMode ? _manaCostDamage : _altManaCostDamage);

            if (isLightMode) CmdSpawnProjectile(GetTargetCharacter());
            else CmdSpawnProjectileDark(GetTargetCharacter());
        }
    }

    protected override bool TryPayCost(List<SkillEnergyCost> skillEnergyCosts, bool startCooldown = true)
    {
        if (IsHaveResourceOnSkill)
        {
            if (isLightMode)
            {
                if (IsAllyTarget(GetTargetCharacter()))
                {
                    skillEnergyCosts = _manaCostHeal;
                }
                else
                {
                    skillEnergyCosts = _manaCostDamage;
                }
            }
            else
            {
                skillEnergyCosts = _altManaCostDamage;

            }

            foreach (var skillCost in skillEnergyCosts)
            {
                var resource = _hero.Resources.First(r => r.Type == skillCost.resourceType);
                resource.CmdUse(Buff.ManaCost.GetBuffedValue(skillCost.resourceCost));
            }

            if (startCooldown)
                IncreaseSetCooldown(CooldownTime);

            if (!_useChargesAsComboPart) TryUseCharge();
            return true;
        }
        else
        {
            return false;
        }
    }

	private bool IsTargetBelowHealthThreshold(Character target)
    {
        var healthComponent = target.GetComponent<Health>();
        return healthComponent != null && healthComponent.CurrentValue <= healthComponent.MaxValue * LowHealthThreshold;
    }

    private void TryApplyExtraState(Character target)
    {
        if (!stunMagicPassiveSkill.IsFillingDestruction || target == null) return;

        var stateComponent = target.GetComponent<CharacterState>();
        if (stateComponent == null) return;

        if (!isLightMode && (UnityEngine.Random.value <= 0.2f)) stateComponent.AddState(States.Destruction, 12f, 0, gameObject, Name);
    }

    private void HandleDefaultMode(Character target)
    {
        if (IsAllyTarget(target))
        {
            Heal(target);
            ApplySpiritEnergyBuff(target);
            //ApplyHealthBuff(_target);
        }
        else if (IsEnemyTarget(target))
        {
            DamageCast(target);
        }
    }

    private void HandleAlternativeMode(Character target)
    {
        if (IsEnemyTarget(target))
        {
            ApplyDamageInAltMode(target);
            ApplySpiritHealthBuff(target);

            if (_lowHealthTalentActive && IsTargetBelowHealthThreshold(target))
                ApplyDefenseDebuff(target);
        }

        else if (IsAllyTarget(target)) return;

        Debug.Log("HandleAlternativeMode");
    }

    private void Heal(Character target)
    {
        bool isBonusActive = _healingBuffTalentActive && Time.time < _lastFlashOfLightCastTime + _healingBuffDuration;

        if (isBonusActive) _healingBonusStacks = Mathf.Min(_healingBonusStacks + 1, 4);
        else _healingBonusStacks = 0;

        float doublingBonus = (_healingBonusStacks > 0) ? Mathf.Pow(2f, _healingBonusStacks) : 0f;

        float bonusHealFromSpiritEnergy = 0f;
        if (_spiritEnergyTalent) bonusHealFromSpiritEnergy = GetSpiritEnergyBonus(target);

        var heal = new Heal
        {
            Value = _healAmount + doublingBonus + bonusHealFromSpiritEnergy,
            DamageableSkill = this
        };

        ApplyHeal(heal, target.gameObject, this, Name);
        TryApplyExtraState(target);
    }

    private float GetSpiritEnergyBonus(Character target)
    {
        var characterState = target?.GetComponent<CharacterState>();
        if (characterState == null) return 0f;

        var spiritEnergyState = characterState.GetState(States.SpiritEnergy) as SpiritEnergyState;
        return spiritEnergyState != null ? spiritEnergyState.GetHealBonus() : 0f;
    }

    private void DamageCast(Character target)
    {
        ApplyDamage(CreateDamage(_damageAmount), target.gameObject);
        TryApplyExtraState(target);
    }

    private void ApplyDamageInAltMode(Character target)
    {
        float damageAmount = _altDamageAmount;
        if (_lowHealthTalentActive && IsTargetBelowHealthThreshold(target))
        {
            damageAmount *= BonusDamageMultiplier;
        }

        Damage damage = CreateDamage(damageAmount);
        ApplyDamage(damage, target.gameObject);
        TryApplyExtraState(target);
    }

    private Damage CreateDamage(float amount)
    {
        return new Damage
        {
            Value = Buff.Damage.GetBuffedValue(amount),
            Type = DamageType.Magical,
            PhysicAttackType = AttackRangeType.RangeAttack,
            School = this.School,
            //DamageableSkill = this,
        };
    }

    private void ApplySpiritEnergyBuff(Character target)
    {
        var talentActive = _manaRestoreBoostTalent ? 1 : 0;
        if (_spiritEnergyAddTalent) AddBuff(States.SpiritEnergy, _buffDuration, talentActive, target.gameObject, Name);
    }

    private void ApplySpiritHealthBuff(Character target)
    {
        var talentActive = _manaRestoreBoostTalent ? 1 : 0;
        if (_spiritEnergyAddTalent) AddBuff(States.SpiritHealth, _altBuffDuration, talentActive, target.gameObject, Name);
    }

    private void ApplyHealthBuff(Character target)
    {
        if (!_healthBoostActive) return;

        AddBuff(States.SparkTalentHealthBuff, HealthBoostDuration, HealthBoostPercentage, target.gameObject, Name);
    }

    private void ApplyDefenseDebuff(Character target)
    {
        AddBuff(States.DefenseReduction, DefenseDebuffDuration, DefenseReductionPercentage, target.gameObject, Name);
    }

    public void SparkOfLightCast()
    {
        AnimStartCastCoroutine();
    }

    public void SparkOfLightEnded()
    {
        AnimCastEnded();
    }

    #region Talents

    public void SpiritEnergyTalentActive(bool value)
    {
        _spiritEnergyTalent = value;
    }

    public void SpiritEnergyAddTalent(bool value)
    {
        _spiritEnergyAddTalent = value;
    }

    #endregion

    private void AddBuff(States state, float duration, float modifier, GameObject target, string skillName)
    {
        var characterState = target.GetComponent<CharacterState>();
        characterState.AddState(state, duration, modifier, target, skillName);
    }

    [Command]
    private void CmdHandleDefaultMode(Character target)
    {
        HandleDefaultMode(target);
        RpcPlayShotSound();
    }

    [Command]
    private void CmdHandleAlternativeMode(Character target)
    {
        HandleAlternativeMode(target);
        RpcPlayShotSound();
    }

    [Command]
    private void CmdSpawnProjectile(Character target)
    {
        Vector3 targetPosition = target.transform.position + Vector3.up;
        Vector3 direction = (targetPosition - spawnPoint.transform.position).normalized;
        float distance = Vector3.Distance(targetPosition, spawnPoint.transform.position);

        LightSparkProjectile projectile = Instantiate(lightSparkProjectile, spawnPoint.transform.position, Quaternion.LookRotation(direction));

        float attackDelay = _castTime;

        projectile.Init(playerLinks, isLightMode, this, distance, attackDelay, target);

        SceneManager.MoveGameObjectToScene(projectile.gameObject, _hero.NetworkSettings.MyRoom);
        NetworkServer.Spawn(projectile.gameObject);
        projectile.StartFly(direction);

        RpcInitProjectile(projectile.gameObject, distance, attackDelay, target);
        RpcPlayShotSound();
    }

    [Command]
    private void CmdSpawnProjectileDark(Character target)
    {
        Vector3 targetPosition = target.transform.position + Vector3.up;
        Vector3 direction = (targetPosition - spawnPoint.transform.position).normalized;
        float distance = Vector3.Distance(targetPosition, spawnPoint.transform.position);

        LightSparkProjectile projectile = Instantiate(darkSparkProjectile, spawnPoint.transform.position, Quaternion.LookRotation(direction));

        float attackDelay = _castTime;

        projectile.Init(playerLinks, isLightMode, this, distance, attackDelay, target);

        SceneManager.MoveGameObjectToScene(projectile.gameObject, _hero.NetworkSettings.MyRoom);
        NetworkServer.Spawn(projectile.gameObject);
        projectile.StartFly(direction);

        RpcInitProjectile(projectile.gameObject, distance, attackDelay, target);
        RpcPlayShotSound();
    }

    [Command]
    private void CmdLightMode()
    {
        RpcLightMode();
    }

    [ClientRpc]
    private void RpcLightMode()
    {
        isLightMode = !isLightMode;
    }


    [Command]
    private void CmdSwitchMode()
    {
        isLightMode = !isLightMode;
        UpdateMode();
    }

    [ClientRpc]
    private void RpcInitProjectile(GameObject projectileObject, float distance, float attackDelay, Character target)
    {
        if (projectileObject.TryGetComponent(out LightSparkProjectile projectile))
        {
            projectile.Init(playerLinks, isLightMode, this, distance, attackDelay, target);
        }
    }

    [ClientRpc]
    private void RpcPlayShotSound()
    {
        if (_audioSource != null && audioClip != null) _audioSource.PlayOneShot(audioClip);
    }

    protected override void ClearData()
    {
        ClearTarget();
        //_target = null;

        _hero.Move.StopLookAt();
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        if (targetInfo.GetTargets().Count > 0)
            SetTarget((Character)targetInfo.GetTargets()[0]);
    }
}