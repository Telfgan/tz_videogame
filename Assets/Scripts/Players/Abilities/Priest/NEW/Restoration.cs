using Mirror;
using System;
using System.Collections;
using UnityEngine;

public class Restoration : Skill
{
    [Header("Restoration (Light Mode) Settings")]
    [SerializeField] private float healPerTick = 6f;
    [SerializeField] private float lightRange = 4f;
    [SerializeField] private float lightDuration = 12.1f;
    [SerializeField] private float healInterval = 4f;
    [SerializeField] private float lightCastTime = 1.2f;
    [SerializeField] private float effectivenessIncreasePerHeal = 0.1f;
    [SerializeField] private AbilityInfo lightInfo;

    [Header("Restoration (Dark Mode) Settings")]
    [SerializeField] private float damagePerTick = 6f;
    [SerializeField] private float darkRange = 6f;
    [SerializeField] private float darkDuration = 12.1f;
    [SerializeField] private float damageInterval = 3f;
    [SerializeField] private float darkCastTime = 1.2f;
    [SerializeField] private AbilityInfo darkInfo;

    [SerializeField] private AudioClip audioClip;

    private AudioSource _audioSource;
    private float _accumulatedEffectiveness = 1f;
    private float _totalHealedInInterval = 0f;
    private bool _spiritEnergyTalent;
    //private IDamageable _target;
    //private Character characterTarget;

    public IDamageable Target => GetTargetCharacter();

    [SyncVar(hook = nameof(OnModeChanged))] public bool isLightMode = true;

    protected override bool IsCanCast => IsCanCastCheck();

    protected override int AnimTriggerCastDelay => Animator.StringToHash("Cast");
    protected override int AnimTriggerCast => 0;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private bool IsCanCastCheck()
    {
        if (GetTargetCharacter() == null) return false;
        return Vector3.Distance(transform.position, GetTargetCharacter().transform.position) <= Radius;
    }

    public event Action OnModeChange;

    private void OnEnable()
    {
        OnModeChange += UpdateMode;
        UpdateMode();
    }

    private void OnDisable()
    {
        OnModeChange -= UpdateMode;
        if (GetTargetCharacter() != null && GetTargetCharacter() is Character character)
        {
            var healthComponent = character.GetComponent<Health>();
            if (healthComponent != null)
            {
                healthComponent.HealTaked -= OnHealTaken;
            }
        }
    }


    public void SwitchMode()
    {
        CmdSwitchMode();
    }

    [Command]
    private void CmdSwitchMode()
    {
        UpdateMode();
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
        Radius = isLightMode ? lightRange : darkRange;
        School = isLightMode ? Schools.Light : Schools.Dark;
        CastDeley = isLightMode ? lightCastTime : darkCastTime;
        AbilityInfoHero = isLightMode ? lightInfo : darkInfo;
        TargetsLayers = isLightMode ? LayerMask.GetMask("Allies") : LayerMask.GetMask("Enemy");
        Hero.Abilities.SkillPanelUpdate();
    }

    private void HandleRestorationLight()
    {
        if (GetTargetCharacter() == null) return;

        bool isAlly = GetTargetCharacter().gameObject.layer == LayerMask.NameToLayer("Allies");

        if (isAlly && TryPayCost())
        {
            var healthComponent = GetTargetCharacter().GetComponent<Health>();
            if (healthComponent != null)
            {
                healthComponent.HealTaked += OnHealTaken;
            }

            CmdAddState(GetTargetCharacter(), States.Restoration, lightDuration);
            //StartCoroutine(ApplyHealOverTime(characterTarget));
        }
    }

    private float GetSpiritEnergyBonus(Character target)
    {
        var characterState = target?.GetComponent<CharacterState>();
        if (characterState == null) return 0f;

        var spiritEnergyState = characterState.GetState(States.SpiritEnergy) as SpiritEnergyState;
        return spiritEnergyState != null ? spiritEnergyState.GetHealBonus() : 0f;
    }

    private void HandleRestorationDark()
    {
        if (GetTargetCharacter() == null) return;

        bool isEnemy = GetTargetCharacter().gameObject.layer == LayerMask.NameToLayer("Enemy");

        if (isEnemy && TryPayCost())
        {
            CmdAddState(GetTargetCharacter(), States.Destruction, darkDuration);
        }
    }

    private void OnHealTaken(float healedAmount, Skill skill, string sourceName)
    {
        _totalHealedInInterval += healedAmount;
    }


    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        while (GetTargetCharacter() == null)
        {
            if (GetMouseButton)
            {
                FindTargetCharacter();
            }
            yield return null;
        }

        TargetInfo targetInfo = new();
        targetInfo.AddTarget(GetTargetCharacter());
        callbackDataSaved(targetInfo);
    }

    protected override IEnumerator CastJob()
    {
        if (GetTargetCharacter() == null) yield break;

        CmdPlayShootSound();

        if (isLightMode)
        {
            HandleRestorationLight();
        }
        else
        {
            HandleRestorationDark();
        }

        yield return null;
    }

    protected override void ClearData()
    {
        ClearTarget();
       // _target = null;
    }

    private void ResetAccumulatedEffectiveness()
    {
        _accumulatedEffectiveness = 1f;
    }

    [Command]
    private void CmdPlayShootSound()
    {
        RpcPlayShotSound();
    }

    [Command]
    private void CmdAddState(Character character, States states, float duration) => character.CharacterState.AddState(states, duration, 0, Hero.gameObject, name);

    [ClientRpc]
    private void RpcPlayShotSound()
    {
        if (_audioSource != null && audioClip != null) _audioSource.PlayOneShot(audioClip);
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        SetTarget((Character)targetInfo.GetTargets()[0]);
    }
} 