using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class PriestShield : Skill
{
    //---------------- LightSettings
    [Header("Shield (Light Mode) Settings")]
    [SerializeField] private float lightShieldDuration = 18f;
    [SerializeField] private float tiredSoulDuration = 12f;
    [SerializeField] private float selfCastTime = 0.6f;
    [SerializeField] private float allyCastTime = 1.2f;
    [SerializeField] private float absorbAmount = 20f;
    [SerializeField] private List<SkillEnergyCost> manaCostLight;
    [SerializeField] private float cooldownLight = 4f;

    //---------------- DarkSettings
    [Header("Shield (Dark Mode) Settings")]
    [SerializeField] private float darkShieldDuration = 12f;
    [SerializeField] private float maxDamagePerTick = 20f;
    [SerializeField] private List<SkillEnergyCost> manaCostDark;
    [SerializeField] private float cooldownDark = 4f;
    [SerializeField] private float darkCastTime = 1.2f;

    [SerializeField] private AudioClip audioClip;

    private AudioSource _audioSource;

    //DisciplineTalent_4
    private bool _talentPhysicalShieldBoostActive = false;
    private const float MaxPhysicalBoostPercentage = 0.5f;
    private const float PhysicalBoostPerDamageUnit = 1f;
    private float _physDamageAccumulator = 0f;
    private float _lastPhysDamageTime = -999f;
    private const float PhysicUnit = 10f;
    private const float PhysBoostTimeWindow = 5f;

    //---------------- Talent 2 (Discipline Shield Boost)
    private bool _disciplineShieldBoostActive = false;
    private int _disciplineStacks = 0;
    private const int MaxDisciplineStacks = 3;
    private const float DisciplineBoostPercentage = 0.1f;

    //---------------- Talent 3 (Dark Magic Damage Boost)
    private bool _talentDarkMagicBoostActive = false;
    private const float MaxDarkMagicBoostPercentage = 0.5f;
    private const float DarkMagicBoostPerUnit = 1f;
    private const float DarkMagicUnit = 10f;
    private float _accumulatedDarkDamage = 0f;
    private float _lastDarkDamageTime = -999f;
    private const float DarkDamageResetTime = 5f;

    //---------------- Talent 4 (Healing Boost)
    private bool _talentHealingBoostActive = false;
    private const float MaxHealingBoostPercentage = 0.5f;
    private const float HealingBoostPerUnit = 1f;
    private float _healingAccumulator = 0f;
    private float _lastHealingTime = -999f;
    private const float HealingUnit = 10f;
    
    //---------------- Talent 5 (Tired Soul Evade)
    private bool _talentTiredSoulActive = false;
    private const float TiredSoulEffectPercentage = 0.5f;

    private float _absorbBonus = 0;
    private float _damagePerTickBonus = 0;
    //private IDamageable _target;
    //private Character _targetCharacter;
    private float _nextAvailableTime;
    public bool isLightMode = true;

    protected override bool IsCanCast => IsCanCastCheck();

    protected override int AnimTriggerCastDelay => Animator.StringToHash("PriestShield");
    protected override int AnimTriggerCast => 0;

    private bool IsCanCastCheck()
    {
        if (GetTargetCharacter() == null || Time.time < _nextAvailableTime) return false;
        return Vector3.Distance(transform.position, GetTargetCharacter().transform.position) <= Radius;
    }

    public event Action OnModeChange;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        OnModeChange += HandleModeChange;
        Hero.DamageTracker.OnDamageTracked += TrackDarkDamage;
        Hero.Health.DamageTaken += TrackPhysDamage;
        Hero.DamageTracker.OnHealTracked += TrackHealDone;
        UpdateMode();

        foreach (var skill in Hero.Abilities.Abilities.Where(skill => skill.School == Schools.Discipline))
        {
            skill.CastEnded += AddDisciplineStack;
        }
    }

    private void OnDisable()
    {
        OnModeChange -= HandleModeChange;
        Hero.DamageTracker.OnDamageTracked -= TrackDarkDamage;
        Hero.Health.DamageTaken -= TrackPhysDamage;
        Hero.DamageTracker.OnHealTracked -= TrackHealDone;

        foreach (var skill in Hero.Abilities.Abilities.Where(skill => skill.School == Schools.Discipline))
        {
            skill.CastEnded -= AddDisciplineStack;
        }
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        SetTarget((Character)targetInfo.GetTargets()[0]);
    }

    public void SwitchMode()
    {
        isLightMode = !isLightMode;
        OnModeChange?.Invoke();
    }

    private void HandleModeChange()
    {
        UpdateMode();
    }

    #region Track bonus
    private void TrackDarkDamage(Damage damage, GameObject target)
    {
        if (damage.School != Schools.Dark) return;

        if (Time.time - _lastDarkDamageTime > DarkDamageResetTime)
        {
            _accumulatedDarkDamage = 0f;
        }

        _accumulatedDarkDamage += damage.Value;
        _lastDarkDamageTime = Time.time;
    }

    private void TrackPhysDamage(Damage damage, Skill skill)
    {
        if (damage.School != Schools.Physical) return;

        if (Time.time - _lastPhysDamageTime > PhysBoostTimeWindow)
        {
            _physDamageAccumulator = 0f;
        }

        _physDamageAccumulator += damage.Value;
        _lastPhysDamageTime = Time.time;
    }

    private void TrackHealDone(Heal heal)
    {
        if (heal.DamageableSkill == null) return;
        if (heal.DamageableSkill.School != Schools.Light) return;

        if (Time.time - _lastHealingTime > PhysBoostTimeWindow)
        {
            _healingAccumulator = 0f;
        }

        _healingAccumulator += heal.Value;
        _lastHealingTime = Time.time;
    }
    #endregion

    private float GetAccumulated(float lastTime, float resetTime, float accumulated)
    {
        if (Time.time - lastTime > resetTime) accumulated = 0f;
        return accumulated;
    }

    private void UpdateMode()
    {
        CastDeley = isLightMode ? allyCastTime : darkCastTime;
        CooldownTime = isLightMode ? cooldownLight : cooldownDark;
        School = isLightMode ? Schools.Light : Schools.Dark;
        TargetsLayers = isLightMode ? LayerMask.GetMask("Allies") : LayerMask.GetMask("Enemy");
    }

    //DisciplineTalent_4
    public void EnableTalentPhysicalShieldBoost(bool value)
    {
        _talentPhysicalShieldBoostActive = value;
        if (!value)
        {
            _physDamageAccumulator = 0;
        }
    }

    //---------------- Talent 2 Logic: Discipline Shield Boost ----------------
    public void EnableDisciplineShieldBoost(bool value)
    {
        _disciplineShieldBoostActive = value;
        if (!value)
        {
            _disciplineStacks = 0;
        }
    }

    private void AddDisciplineStack()
    {
        if (_disciplineShieldBoostActive && _disciplineStacks < MaxDisciplineStacks)
        {
            _disciplineStacks++;
            Debug.Log($"Discipline stack added. Current stacks: {_disciplineStacks}");
        }
    }

    //---------------- Talent 3 Logic: Dark Magic Damage Boost ----------------
    public void EnableDarkMagicBoost(bool value)
    {
        _talentDarkMagicBoostActive = value;
        
        if (_talentDarkMagicBoostActive) return;
        
        _damagePerTickBonus = 0;
    }

    //---------------- Talent 4 Logic: Healing Boost ----------------
    public void EnableHealingBoost(bool value)
    {
        _talentHealingBoostActive = value;
        
        if(Hero == null || Hero.DamageTracker == null) return;
        
        Hero.DamageTracker.RemoveOldLocalEntries();
    }

    public void EnableTiredSoulEvade(bool value)
    {
        _talentTiredSoulActive = value;
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
       // _targetCharacter = null;

        while (GetTargetCharacter() == null)
        {
            if (Input.GetMouseButton(0))
            {
                FindTargetCharacter(true);
                //_target = GetRaycastTarget(true);

                if (GetTargetCharacter() is Character character && character == transform.GetComponentInParent<Character>())
                {
                   // _targetCharacter = character;
                    _absorbBonus = 0;
                    CastDeley = selfCastTime;
                }
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
        Cast();

        yield return null;
    }

    private void Cast()
    {
        _nextAvailableTime = Time.time + CooldownTime;

        CmdPlayShootSound();

        if (isLightMode)
        {
            HandleLightShield();
        }
        else
        {
            HandleDarkShield();
        }
    }

    private void HandleLightShield()
    {
        if (GetTargetCharacter() == null) return;
        if (!TryPayCost(manaCostLight)) return;

        _absorbBonus = CalculateTotalAbsorbBonus();

        var characterState = GetTargetCharacter().GetComponent<CharacterState>();
        var duration = _talentTiredSoulActive && characterState.CheckForState(States.TiredSoul)
            ? lightShieldDuration * TiredSoulEffectPercentage
            : lightShieldDuration;

        var absorbDamage = _talentTiredSoulActive && characterState.CheckForState(States.TiredSoul)
            ? (absorbAmount + _absorbBonus) * TiredSoulEffectPercentage
            : absorbAmount + _absorbBonus;

        CmdAddDebaff(States.LightShield, States.TiredSoul, duration, tiredSoulDuration, absorbDamage, GetTargetCharacter().gameObject, Name);

        Debug.Log($"[PriestShield] Final Absorb = {absorbDamage} (Base: {absorbAmount}, Bonus: {_absorbBonus})");
    }

    private void HandleDarkShield()
    {
        if (GetTargetCharacter() == null) return;

        if (!TryPayCost(manaCostDark)) return;

        CmdAddBaff(States.DarkShield, darkShieldDuration, maxDamagePerTick + _damagePerTickBonus, GetTargetCharacter().gameObject, Name);
        Debug.Log("Dark Shield applied to " + GetTargetCharacter().name);
    }

    private float CalculateTotalAbsorbBonus()
    {
        float bonus = 0f;

        if (_disciplineShieldBoostActive && _disciplineStacks > 0)
        {
            float disciplineBonus = absorbAmount * DisciplineBoostPercentage * _disciplineStacks;
            bonus += disciplineBonus;
            Debug.Log($"[ShieldBonus] Discipline: +{disciplineBonus}");
            _disciplineStacks = 0;
        }

        return bonus;
    }

    private float BoostActive(float amount, float unit, float boostPerUnit, float maxBoostPercentage)
    {
        float boost = Mathf.Min(Mathf.Floor(amount / unit) * boostPerUnit, absorbAmount * maxBoostPercentage);
        return boost;
    }


    public void PriestShieldCast()
    {
        AnimStartCastCoroutine();
    }

    public void PriestShieldEnd()
    {
        AnimCastEnded();
    }

    [Command]
    private void CmdAddDebaff(States lightState, States tiredState, float duration, float tiredDuration,
     float damageToExit, GameObject target, string skillName)
    {
        var characterState = target.GetComponent<CharacterState>();
        float finalAbsorb = damageToExit;

        if (_talentDarkMagicBoostActive)
        {
            float darkDamage = GetAccumulated(_lastDarkDamageTime, DarkDamageResetTime, _accumulatedDarkDamage);
            finalAbsorb += BoostActive(darkDamage, DarkMagicUnit, DarkMagicBoostPerUnit, MaxDarkMagicBoostPercentage);
            _accumulatedDarkDamage = 0f;
            _lastDarkDamageTime = -999f;
        }

        if (_talentPhysicalShieldBoostActive)
        {
            float physicalDamage = GetAccumulated(_lastPhysDamageTime, DarkDamageResetTime, _physDamageAccumulator);
            finalAbsorb += BoostActive(physicalDamage, PhysicUnit, PhysicalBoostPerDamageUnit, MaxPhysicalBoostPercentage);
            _physDamageAccumulator = 0f;
            _lastPhysDamageTime = -999f;
        }

        if (_talentHealingBoostActive)
        {
            float healingAmount = GetAccumulated(_lastHealingTime, DarkDamageResetTime, _healingAccumulator);
            finalAbsorb += BoostActive(healingAmount, HealingUnit, HealingBoostPerUnit, MaxHealingBoostPercentage);
            _healingAccumulator = 0f;
            _lastHealingTime = -999f;
        }

        if (!_talentTiredSoulActive)
        {
            if (characterState.CheckForState(tiredState))
            {
                Debug.Log("Cannot apply Light Shield, target already has TiredSoul and talent is inactive.");
                return;
            }

            Debug.Log("Talent is inactive, applying LightShield and TiredSoul.");
            characterState.AddState(lightState, duration, finalAbsorb, target, skillName);
            characterState.AddState(tiredState, tiredDuration, finalAbsorb, target, skillName);
        }
        else
        {
            if (characterState.CheckForState(tiredState))
            {
                int tiredSoulStacks = characterState.CheckStateStacks(tiredState);

                Debug.Log($"Talent is active. TiredSoul stacks: {tiredSoulStacks}");

                if (tiredSoulStacks >= 2)
                {
                    Debug.Log("TiredSoul has 2 or more stacks, exiting without applying LightShield.");
                    return;
                }

                Debug.Log("TiredSoul has less than 2 stacks, applying LightShield and TiredSoul.");
                characterState.AddState(lightState, duration, damageToExit, target, skillName);
                characterState.AddState(tiredState, tiredDuration, damageToExit, target, skillName);
            }
            else
            {
                Debug.Log("Talent is active, but target does not have TiredSoul. Applying LightShield and TiredSoul.");
                characterState.AddState(lightState, duration, damageToExit, target, skillName);
                characterState.AddState(tiredState, tiredDuration, damageToExit, target, skillName);
            }
        }
    }

    [Command]
    private void CmdAddBaff(States darkState, float duration, float damagePerTick, GameObject target, string skillName)
    {
        var characterState = target.GetComponent<CharacterState>();
        characterState.AddState(darkState, duration, damagePerTick, target, skillName);
    }


    [Command]
    private void CmdPlayShootSound()
    {
        RpcPlayShotSound();
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
        _damagePerTickBonus = 0;
        _absorbBonus = 0;
    }
}