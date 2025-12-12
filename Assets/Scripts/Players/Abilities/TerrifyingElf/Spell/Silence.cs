using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Silence : Skill
{
    [SerializeField] private float _duration;
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private bool _reducedCooldown;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private int _maxAdditionalManaUsage = 7;
    [SerializeField] private Ghost ghost;
    [SerializeField] private float damageMinoin = 60;

    private float _baseDuration;
    private AudioSource audioSource;
    private Vector3 _targetPoint = Vector3.positiveInfinity;

    private bool _effectsDarknessTalent;
    private bool _canAttackMinions;
    private bool _isSilenceEffectsOnMinionMagic;
    private bool _isSilenceEffectGhostCast;
    private bool _isSilenceAddAllCharacterWithDeabaffElf;

    public bool IsSilenceAddAllCharacterWithDeabaffElf { get => _isSilenceAddAllCharacterWithDeabaffElf; }

    protected override bool IsCanCast
    {
        get
        {
            if (_disactive) return false;

            if (TargetInfoQueue.Count > 0 && TargetInfoQueue.TryPeek(out var target) && target != null && target.Points.Count > 0)
            {
                var point = target.Points[0];
                if (float.IsInfinity(point.x)) return false;
                return IsPointInRadius(Radius, point);
            }

            return IsPointInRadius(Radius, _targetPoint);
        }
    }

    protected override int AnimTriggerCastDelay => Animator.StringToHash("SpellCastDelayAnimTrigger");
    protected override int AnimTriggerCast => 0;

    private void Start()
    {
        _baseDuration = _duration;
        _baseCooldownTime = CooldownTime;
        audioSource = GetComponent<AudioSource>();
    }
    public override void LoadTargetData(TargetInfo targetInfo) => _targetPoint = targetInfo.Points[0];
    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        Vector3 targetPoint = Vector3.positiveInfinity;

        while (float.IsPositiveInfinity(targetPoint.x))
        {
            if (GetMouseButton)
            {
                Vector3 clickedPoint = GetMousePoint();

                if (IsPointInRadius(Radius, clickedPoint))
                {
                    _targetPoint = clickedPoint;
                    DrawDamageZone(_targetPoint);

                    break;
                }
            }
            yield return null;
        }

        TargetInfo targetInfo = new TargetInfo();
        targetInfo.Points.Add(_targetPoint);
        callbackDataSaved(targetInfo);
    }

    protected override IEnumerator CastJob()
    {
        if (_targetPoint == Vector3.positiveInfinity) yield return null;

        CmdAdditionalMana();
        SpawnEffectAtTargetPoint(_targetPoint);
        ApplyStateToEnemiesInZone(_targetPoint);
        StopDamageZone();
        yield return null;
    }


    private void SpawnEffectAtTargetPoint(Vector3 target)
    {
        if (effectPrefab != null) Instantiate(effectPrefab, target, Quaternion.identity);
        if (effectPrefab != null) Instantiate(effectPrefab, target, Quaternion.identity);
    }

    private void ApplyStateToEnemiesInZone(Vector3 target)
    {
        Collider[] hitColliders = Physics.OverlapSphere(target, Area, TargetsLayers);

        int minionHitCount = 0;
        int ghostAuraMinionHitCount = 0;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject != Hero.gameObject)
                ApplyEnemiesZone(hitCollider, ref minionHitCount, ref ghostAuraMinionHitCount);
        }

        if (minionHitCount > 0 && _isSilenceEffectsOnMinionMagic) DecreaseSetCooldown(4f * minionHitCount);
        if (ghostAuraMinionHitCount >= 2 && _isSilenceEffectGhostCast) CmdTriggerGhostFreeWindow();
    }

    private void ApplyEnemiesZone(Collider hitCollider, ref int minionHitCount, ref int ghostAuraMinionHitCount)
    {
        if (hitCollider.TryGetComponent<HeroComponent>(out HeroComponent enemy))
        {
            var targetState = enemy.GetComponent<CharacterState>();
            if (targetState != null)
            {
                CmdApplySilenceState(targetState);
            }
        }

        if (hitCollider.TryGetComponent<MinionComponent>(out MinionComponent minion))
        {
            var targetState = minion.GetComponent<CharacterState>();

            if (targetState != null)
            {
                CmdApplySilenceState(targetState);
                minionHitCount++;
            }

            if (minion.TryGetComponent<GhostAura>(out GhostAura ghostAura)) ghostAuraMinionHitCount++;

            if (_canAttackMinions) MinionDamage(minion);
        }
    }

    private void MinionDamage(MinionComponent minion)
    {
        ApplyDamage(damageMinoin, DamageType.Magical, minion);
        RewardMana();
    }

    private void RewardMana()
    {
        if (Hero.TryGetResource(ResourceType.Mana) is Mana manaResource)
        {
            manaResource.CmdAdd(damageMinoin);
            Debug.Log("Restored mana for hitting a magical creature.");
        }
    }

    private void ApplyDamage(float damage, DamageType damageType, MinionComponent target)
    {
        Damage _damage = new Damage
        {
            Value = damage,
            Type = damageType,
            PhysicAttackType = AttackRangeType.RangeAttack,
        };

        if (target is Component targetComponent)
        {
            CmdApplyDamage(_damage, targetComponent.gameObject);
            CmdReduceGhostCharge(target);
            StartCoroutine(IGhostHealthCheck(target));
        }
    }

    private IEnumerator IGhostHealthCheck(MinionComponent target)
    {
        yield return new WaitForSeconds(0.1f);
        if (target.TryGetComponent<GhostAura>(out var ghostAura))
        {
            if (ghostAura.TryGetComponent<Health>(out var health))
            {
                if (health.CurrentValue <= 0) ghost.ResetCurrentChargeCooldown(0);
            }
        }

    }

    [Server] private void ServerGhostHealthCheck(MinionComponent target) => StartCoroutine(IGhostHealthCheck(target));

    #region Talents

    public void SetCanAttackMinions(bool value) => _canAttackMinions = value;
    public void SetReducedCooldown(bool value) => _reducedCooldown = value;
    public void EffectsInnerDarknessTalentActive(bool value) => _effectsDarknessTalent = value;
    public void SilenceEffectsOnMinionMagic(bool value) => _isSilenceEffectsOnMinionMagic = value;
    public void SilenceEffectGhostCast(bool value) => _isSilenceEffectGhostCast = value;
    public void SilenceAddAllCharacterWithDeabaffElf(bool value) => _isSilenceAddAllCharacterWithDeabaffElf = value;

    #endregion
    [Command] private void CmdTriggerGhostFreeWindow() => RpcTriggerGhostFreeWindow();
    [Command] private void CmdReduceGhostCharge(MinionComponent target) => ServerGhostHealthCheck(target);

    [Command]
    private void CmdApplySilenceState(CharacterState targetState)
    {
        RpcPlayShotSound();

        if (_effectsDarknessTalent && targetState.CheckForState(States.InnerDarkness))
        {
            int innerDarknessStacks = targetState.CheckStateStacks(States.InnerDarkness);

            float durationMultiplier = 1.4f + 0.1f * (innerDarknessStacks - 1);
            _duration = durationMultiplier;
        }

        targetState.AddState(States.Silent, _duration, 0, Hero.gameObject, this.name);
    }

    [Command]
    private void CmdAdditionalMana()
    {
        var manaResource = Hero.TryGetResource(ResourceType.Mana);

        if (manaResource != null)
        {

            int availableMana = Mathf.Min((int)manaResource.CurrentValue - 1, _maxAdditionalManaUsage);
            if (availableMana > 1)
            {
                manaResource.TryUse(availableMana);
                _duration += 0.5f * availableMana;
            }
        }
    }

    [ClientRpc]
    private void RpcPlayShotSound()
    {
        if (audioSource != null && audioClip != null) audioSource.PlayOneShot(audioClip);
    }

    [ClientRpc]
    private void RpcTriggerGhostFreeWindow()
    {
        if (ghost != null) ghost.TryStartGhostBoostWindow();
    }

    protected override void ClearData()
    {
        _targetPoint = Vector3.positiveInfinity;
    }
}
