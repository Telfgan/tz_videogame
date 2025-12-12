using Mirror;
using System;
using System.Collections;
using UnityEngine;

public class FlashOfLight : Skill
{
    [Header("Flash of Light Settings")]
    [SerializeField] private float _healAmount = 35f;
    [SerializeField] private float _lightRange = 4f;
    [SerializeField] private AbilityInfo lightInfo;

    [Header("Flash of Darkness Settings")]
    [SerializeField] private float _damageAmount = 35f;
    [SerializeField] private float _darkRange = 6f;
    [SerializeField] private AbilityInfo darkInfo;

    [SerializeField] private AudioClip audioClip;
    [SerializeField] private ReversePolarity reversePolarity;

    private bool _spiritEnergyTalent;

    //private IDamageable _target;
    //private Character _previousTarget;

    private AudioSource _audioSource;
    private bool _isCooldownTalentActive = false;
    private float _talentCooldown = 5f;
    private float _lastTalentTime = -5f;
    private float _cooldownReduction = 5f;

    public event Action OnModeChange;
    [SyncVar(hook = nameof(OnModeChanged))] public bool isLightMode = true;

    protected override bool IsCanCast => IsCanCastCheck();

    private bool IsCanCastCheck()
    {
        if (GetTargetCharacter() == null) return false;

        if (isLightMode) return (GetTargetCharacter() is Character character && character == Hero) || GetTargetCharacter().gameObject.layer == LayerMask.NameToLayer("Allies");
        else
            return GetTargetCharacter().gameObject.layer == LayerMask.NameToLayer("Enemy");
    }

    protected override int AnimTriggerCastDelay => Animator.StringToHash("Spell");
    protected override int AnimTriggerCast => 0;

    public void EnableTalentPhysicalShieldBoost(bool value)
    {
        _isCooldownTalentActive = value;
    }
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    public override void LoadTargetData(TargetInfo targetInfo)
    {
        SetTarget((Character)targetInfo.GetTargets()[0]);
    }

    private void OnEnable()
    {
        OnModeChange += UpdateMode;
        UpdateMode();
    }

    private void OnDisable()
    {
        OnModeChange -= UpdateMode;
    }

    public void SwitchMode()
    {
        CmdSwitchMode();
    }

    [Command]
    private void CmdSwitchMode()
    {
        isLightMode = !isLightMode;
    }

    private void OnModeChanged(bool oldValue, bool newValue)
    {
        UpdateMode();
        OnModeChange?.Invoke();
    }

    public void SpiritEnergyTalentActive(bool value)
    {
        _spiritEnergyTalent = value;
    }

    private void UpdateMode()
    {
        Radius = isLightMode ? _lightRange : _darkRange;
        School = isLightMode ? Schools.Light : Schools.Dark;
        AbilityInfoHero = isLightMode ? lightInfo : darkInfo;
        TargetsLayers = isLightMode
            ? LayerMask.GetMask("Allies", "Player")
            : LayerMask.GetMask("Enemy");
        Hero.Abilities.SkillPanelUpdate();
    }
    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
       // _previousTarget = null;

        while (GetTargetCharacter() == null)
        {
            if (Input.GetMouseButton(0))
            {
                FindTargetCharacter(true);
                //_target = GetRaycastTarget(true);

                if (GetTargetCharacter() != null && GetTargetCharacter() is Character characte && IsValidTarget(characte)) SetTarget(characte);
                else ClearTarget();

            }
            yield return null;
        }
        TargetInfo targetInfo = new();
        targetInfo.AddTarget(GetTargetCharacter());
        callbackDataSaved(targetInfo);
    }

    protected override IEnumerator CastJob()
    {
        if (GetTargetCharacter() == null || !IsCanCast) yield break;

        if (TryPayCost())
        {
            CmdPlayShootSound();

            if (reversePolarity != null && Hero.CharacterState.CheckForState(States.ReversePolarity))
            {
                reversePolarity.SwitchSpells();
                reversePolarity.RemoveReversePolarityEffect();
            }

                if (isLightMode) HandleFlashOfLight();
                else HandleFlashOfDarkness();
        }

        yield return null;
    }

    private void HandleFlashOfLight()
    {
        if (_isCooldownTalentActive && Time.time - _lastTalentTime >= _talentCooldown)
        {
            ReduceCooldowns();
            _lastTalentTime = Time.time;
        }

        Heal(GetTargetCharacter());
    }

    private void HandleFlashOfDarkness()
    {
        Damage(GetTargetCharacter());
    }

    private void Heal(Character target)
    {
        var health = target.GetComponent<Health>();
        if (health == null) return;

        float bonusHealFromSpiritEnergy = 0;
        if (_spiritEnergyTalent) bonusHealFromSpiritEnergy = GetSpiritEnergyBonus(target);
        var heal = new Heal 
        {
            Value = _healAmount + bonusHealFromSpiritEnergy,
            DamageableSkill = this
        };

        CmdApplyHeal(heal, health.gameObject, this, Name);
    }

    private float GetSpiritEnergyBonus(Character target)
    {
        if (target == null) return 0f;

        var characterState = target.GetComponent<CharacterState>();
        if (characterState == null) return 0f;

        var spiritEnergyState = characterState.GetState(States.SpiritEnergy) as SpiritEnergyState;
        if (spiritEnergyState == null) return 0f;

        return spiritEnergyState.GetHealBonus();
    }

    private void Damage(Character target)
    {
        var damage = new Damage
        {
            Value = Buff.Damage.GetBuffedValue(_damageAmount),
            Type = DamageType.Physical,
            PhysicAttackType = AttackRangeType.RangeAttack,
            School = this.School,
        };

        CmdApplyDamage(damage, target.gameObject);
    }

    private bool IsValidTarget(Character target)
    {
        if (target == null) return false;

        if (isLightMode) return target == Hero || target.gameObject.layer == LayerMask.NameToLayer("Allies");
        else return target.gameObject.layer == LayerMask.NameToLayer("Enemy");
    }

    private void ReduceCooldowns()
    {
        foreach (var ability in Hero.Abilities.Abilities)
            ability.DecreaseSetCooldown(_cooldownReduction);
    }

    [Command]
    private void CmdPlayShootSound()
    {
        RpcPlayShotSound();
    }

    [ClientRpc]
    private void RpcPlayShotSound()
    {
        if (_audioSource && audioClip)
            _audioSource.PlayOneShot(audioClip);
    }

    protected override void ClearData()
    {
        ClearTarget();
        //_target = null;
    }
}
