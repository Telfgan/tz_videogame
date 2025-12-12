 using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PullingHealth : Skill
{
    [Header("Pulling Health Settings")]
    [SerializeField] private GameObject _pullingHealthPrefab;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Health health;
    [SerializeField] private List<GameObject> ghost = new List<GameObject>();
    [SerializeField] private float tickInterval;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private Ghost ghostSkill;

    private AudioSource _audioSource;
    private GameObject _activeEffect;
    private List<GameObject> _activeGhostEffects = new List<GameObject>();
    private List<GameObject> _allActiveEffects = new List<GameObject>();
    private IDamageable _target;
    private float _baseRadius;
    private float _baseTickInterval;
    private float _baseCastStreamDuration;
    private float _ignoreMoveTimeLeft;
    private bool _ignoreMoveCheck;

    private const float _teleportTime = 0.3f;

    private readonly List<IDamageable> _extraTargets = new();
    private readonly List<GameObject> _extraEffects = new();

    #region Talent
    private bool _pullingHealthThroughGhosts;
    private bool pullingHealthGhostTalent;
    private bool _pullingHealthSpeedWithFearTalent;
    #endregion

    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerCast => Animator.StringToHash("PullingHealthCastDelay");

    protected override bool IsCanCast
    {
        get
        {
            if (_target != null) return Vector3.Distance(_target.transform.position, transform.position) <= Radius;
            return false;
        }
    }

    public event Action<Transform, IDamageable, int> OnInnerDarknessTriggered;

    public void PullingHealthCast() => AnimStartCastCoroutine();
    public void PullingHealthEnd() => AnimCastEnded();

    public void MovePullingHealth()
    {
        _hero.Move.CanMove = false;
        _hero.Move.StopMoveAndAnimationMove();
    }

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _baseRadius = Radius;
        _baseCastStreamDuration = CastStreamDuration;
        _baseTickInterval = tickInterval;
    }

    private void OnDestroy()
    {
        ghostSkill.Teleported -= OnGhostTeleport;
        OnSkillCanceled -= HandleSkillCanceled;
    }

    private void OnEnable()
    {
        OnSkillCanceled += HandleSkillCanceled;
        ghostSkill.Teleported += OnGhostTeleport;
    }

    private void OnGhostTeleport(Character character, Vector3 _)
    {
        if (character == Hero)
        {
            _ignoreMoveCheck = true;
            _ignoreMoveTimeLeft = _teleportTime;
        }
    }
    public override void LoadTargetData(TargetInfo targetInfo)
    {
        if (targetInfo.GetTargets().Count > 0) _target = targetInfo.GetTargets()[0] as IDamageable;

        if (_target is Character character)
        {
            var multiMagic = Hero.CharacterState.GetState(States.MultiMagic) as MultiMagic;
            if (multiMagic != null) multiMagic.LastTarget = character;
        }

        if (_pullingHealthThroughGhosts) UpdateRadiusBasedOnGhosts();
    }
    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        ITargetable target = null;

        while (target == null)
        {
            //if (GetMouseButton) if (GetRaycastTarget() is ITargetable targetable) target = targetable;
            yield return null;
        }

        TargetInfo targetInfo = new TargetInfo();
        targetInfo.AddTarget(target);
        callbackDataSaved(targetInfo);
    }

    private void UpdateRadiusBasedOnGhosts()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, Radius);
        int ghostCount = 0;
        foreach (var collider in hitColliders) if (collider.TryGetComponent<GhostAura>(out var ghostAura)) ghostCount++;
        Radius = _baseRadius + ghostCount * 2;
        Radius = Mathf.Clamp(Radius, _baseRadius, _baseRadius + 4);
        if (_skillRender != null) _skillRender.DrawRadius(Radius);
    }

    protected override IEnumerator CastJob()
    {
        if (_target == null) yield return null;

        Debug.Log("1");
        int innerDarknessStacks;

        #region Work with InnerDarkness
        if (_target is Character character)
        {
            var targetComponentState = character.GetComponent<CharacterState>();

            if (pullingHealthGhostTalent && targetComponentState.CheckForState(States.InnerDarkness))
            {
                innerDarknessStacks = targetComponentState.CheckStateStacks(States.InnerDarkness);
                Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, Radius);

                int ghostsToAdd = innerDarknessStacks == 2 ? 1 : innerDarknessStacks == 4 ? 2 : 0;
                int addedGhosts = 0;

                foreach (var obj in nearbyObjects)
                {
                    if (addedGhosts >= ghostsToAdd) break;

                    if (obj.TryGetComponent<GhostAura>(out GhostAura ghostAura))
                    {
                        float distanceToTarget = Vector3.Distance(obj.transform.position, character.transform.position);
                        if (distanceToTarget <= Radius && !ghost.Contains(obj.gameObject))
                        {
                            ghost.Add(obj.gameObject);
                            CmdSyncGhosts(obj.gameObject);
                            addedGhosts++;
                        }
                    }
                }

                CmdSpawnPullingHealthEffectGhost(character.gameObject);

                if (innerDarknessStacks > 0)
                {
                    float durationMultiplier = 1.4f + 0.1f * (innerDarknessStacks - 1);
                    CastStreamDuration = _baseCastStreamDuration * durationMultiplier;
                }
            }

            if (_pullingHealthSpeedWithFearTalent && targetComponentState.CheckForState(States.Fear))
            {
                float speedModifier = 0.5f;
                tickInterval *= speedModifier;
            }
        }
        #endregion

        _hero.Animator.SetTrigger(AnimTriggerCastDelay);
        _hero.NetworkAnimator.SetTrigger(AnimTriggerCastDelay);

        var multiMagic = Hero.CharacterState.GetState(States.MultiMagic) as MultiMagic;
        if (multiMagic != null)
        {
            foreach (var characterTarget in multiMagic.PopPendingTargets())
            {
                if (characterTarget == _target as Character)
                {
                    _extraTargets.Add(characterTarget);

                    TryPayCost();
                    CmdSpawnExtraPullingEffect(gameObject, characterTarget.gameObject);
                }
            }
        }

        AfterCastJob();
        yield return StartCoroutine(StreamDuration());
    }


    private IEnumerator StreamDuration()
    {
        float elapsed = 0f;
        float damageTickElapsed = 0f;
        float positionThreshold = 1f;
        var manaResource = Hero.TryGetResource(ResourceType.Mana);

        if (manaResource == null || manaResource.CurrentValue < 2)
        {
            CmdDestroyEffect();
            TryCancel();
            yield break;
        }

        Vector3 initialPosition = transform.position;

        CmdPlayShotSound();

        #region Pulling through Ghosts (Length)
        if (_pullingHealthThroughGhosts)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, Radius);
            List<GhostAura> ghostsInZone = new List<GhostAura>();

            foreach (var collider in hitColliders) if (collider.TryGetComponent<GhostAura>(out var ghostAura)) ghostsInZone.Add(ghostAura);

            ghostsInZone.Sort((a, b) => Vector3.Distance(transform.position, a.transform.position).CompareTo(Vector3.Distance(transform.position, b.transform.position)));

            float targetDistance = Vector3.Distance(transform.position, _target.transform.position);
            if (targetDistance <= _baseRadius) CmdSpawnPullingHealthEffect(gameObject, _target.gameObject);

            if (targetDistance <= _baseRadius + 3 && ghostsInZone.Count == 1)
            {
                GameObject nearestGhost = ghostsInZone[0].gameObject;
                CmdSpawnPullingHealthEffect(gameObject, nearestGhost);
                CmdSpawnPullingHealthEffect(nearestGhost, _target.transform.gameObject);
            }

            else if (targetDistance <= _baseRadius + 6 && ghostsInZone.Count == 2)
            {
                GameObject ghost1 = ghostsInZone[0].gameObject;
                GameObject ghost2 = ghostsInZone[1].gameObject;

                CmdSpawnPullingHealthEffect(gameObject, ghost1);
                CmdSpawnPullingHealthEffect(ghost1, ghost2);
                CmdSpawnPullingHealthEffect(ghost2, _target.gameObject);
            }
        }
        #endregion

        else
        {
            float targetDistance = Vector3.Distance(transform.position, _target.transform.position);
            if (targetDistance <= _baseRadius) CmdSpawnPullingHealthEffect(gameObject, _target.gameObject);
        }

        while (elapsed < CastStreamDuration)
        {
            if (_target == null)
            {
                TryCancel();
                CmdDestroyEffect();
                yield break;
            }

            if (_ignoreMoveCheck)
            {
                initialPosition = transform.position;
                _ignoreMoveTimeLeft -= Time.deltaTime;
                if (_ignoreMoveTimeLeft <= 0f) _ignoreMoveCheck = false;
            }

            if (_target != null && (Input.GetMouseButtonDown(1) || ( Vector3.Distance(transform.position, _target.transform.position) > Radius))           || Vector3.Distance(initialPosition, transform.position) > positionThreshold && !_ignoreMoveCheck)
            {
                _hero.Animator.ResetTrigger(Animator.StringToHash("PullingHealthCastDelay"));
                _hero.NetworkAnimator.ResetTrigger(Animator.StringToHash("PullingHealthCastDelay"));

                CmdCrossFade();
                _hero.Animator.CrossFade("PullingHealthCastDelayExit", 0.1f);

                TryCancel();
                CmdDestroyEffect();
                yield break;
            }

            if (_target != null)
            {
                Vector3 directionToTarget = (_target.transform.position - transform.position).normalized;
                directionToTarget.y = 0;
                transform.rotation = Quaternion.LookRotation(directionToTarget);
            }

            if (damageTickElapsed >= tickInterval)
            {
                ApplyDamageToTarget();
                HealPlayer();

                foreach (var ghost in ghost) ApplyDamageThroughGhost(ghost);
                damageTickElapsed = 0f;
            }

            if (manaResource.CurrentValue < 2)
            {
                CmdDestroyEffect();
                TryCancel();
                yield break;
            }

            elapsed += Time.deltaTime;
            damageTickElapsed += Time.deltaTime;
            yield return null;
        }

        CastStreamDuration = _baseCastStreamDuration;
        tickInterval = _baseTickInterval;
        TryCancel();

        CmdDestroyEffect();
    }

    private void ApplyDamageThroughGhost(GameObject ghost)
    {
        if (ghost.TryGetComponent<Health>(out Health ghostHealth))
        {
            float ghostBaseDamage = Damage * 0.3f;

            Damage damage = new Damage
            {
                Value = ghostBaseDamage,
                Type = DamageType,
            };

            if (_target != null)
            {
                CmdApplyDamage(damage, _target.gameObject);
            }

            float ghostHealValue = Damage * 0.70f;
            ghostHealth.CmdAdd(ghostHealValue);

        }
    }

    private void ApplyDamageToTarget()
    {
        Damage damage = new Damage
        {
            Value = Damage,
            Type = DamageType,
        };

        if (_target != null) CmdApplyDamage(damage, _target.gameObject);

        foreach (var damageble in _extraTargets) CmdApplyDamage(damage, damageble.gameObject);
    }
    private void HealPlayer()
    {
        if (health == null) return;

        Heal heal = new Heal
        {
            Value = Damage,
        };

        health.CmdAdd(heal.Value);
    }

    #region Talents
    public void SetPullingHealthGhostTalentActive(bool value) => pullingHealthGhostTalent = value;
    public void PullingHealthSpeedWithFearTalentActive(bool value) => _pullingHealthSpeedWithFearTalent = value;
    public void PullingHealthThroughGhosts(bool value) => _pullingHealthThroughGhosts = value;
    #endregion

    private void HandleSkillCanceled()
    {
        if (_hero != null && _hero.Move != null)
        {
            Hero.Move.CanMove = true;
            Hero.Move.StopLookAt();
           Hero.Animator.speed = 1;
        }

        _target = null;
        _extraTargets.Clear();
        _extraEffects.Clear();
        AfterCastJob();
        CmdStopShotSound();
    }
    [Command] private void CmdSyncGhosts(GameObject ghostObj) => ghost.Add(ghostObj);

    [Command]
    private void CmdSpawnPullingHealthEffect(GameObject startPoint, GameObject targetPoint)
    {
        if (_pullingHealthPrefab == null || startPoint == null || targetPoint == null) return;

        GameObject effectInstance = Instantiate(_pullingHealthPrefab, startPoint.transform.position, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(effectInstance, _hero.NetworkSettings.MyRoom);
        NetworkServer.Spawn(effectInstance);
        RpcInitEffects(effectInstance, startPoint, targetPoint);

        _allActiveEffects.Add(effectInstance);
        _activeEffect = effectInstance;
    }

    [Command]
    private void CmdSpawnPullingHealthEffectGhost(GameObject targetPoint)
    {
        if (_pullingHealthPrefab == null || targetPoint == null) return;

        for (int i = 0; i < ghost.Count; i++)
        {
            GameObject ghostEffectInstance = Instantiate(_pullingHealthPrefab, ghost[i].transform.position, Quaternion.identity);
            _activeGhostEffects.Add(ghostEffectInstance);
            SceneManager.MoveGameObjectToScene(ghostEffectInstance, _hero.NetworkSettings.MyRoom);
            NetworkServer.Spawn(ghostEffectInstance);
            RpcInitEffects(ghostEffectInstance, ghost[i], targetPoint);
        }
    }

    [Command]
    private void CmdSpawnExtraPullingEffect(GameObject start, GameObject target)
    {
        if (_pullingHealthPrefab == null || start == null || target == null) return;

        var effect = Instantiate(_pullingHealthPrefab, start.transform.position, Quaternion.identity);
        _extraEffects.Add(effect);
        SceneManager.MoveGameObjectToScene(effect, _hero.NetworkSettings.MyRoom);
        NetworkServer.Spawn(effect);
        RpcInitEffects(effect, start, target);
    }

    [Command]
    private void CmdDestroyEffect()
    {
        if (_activeEffect != null)
        {
            Debug.Log($"Destroying active effect: {_activeEffect.name}");
            NetworkServer.Destroy(_activeEffect);
            RpcDestroyClientEffect(_activeEffect);
            _activeEffect = null;
        }

        for (int i = 0; i < _activeGhostEffects.Count; i++)
        {
            if (_activeGhostEffects.Count > 0)
            {
                NetworkServer.Destroy(_activeGhostEffects[i]);
                RpcDestroyClientEffect(_activeGhostEffects[i]);
            }
        }

        foreach (var effect in _extraEffects) if (effect != null) NetworkServer.Destroy(effect);

        _activeGhostEffects.Clear();

        ghost.Clear();

        for (int i = 0; i < _allActiveEffects.Count; i++)
        {
            if (_allActiveEffects[i] != null)
            {
                Debug.Log($"Destroying additional effect: {_allActiveEffects[i].name}");
                NetworkServer.Destroy(_allActiveEffects[i]);
                RpcDestroyClientEffect(_allActiveEffects[i]);
            }
        }
        _allActiveEffects.Clear();
    }
    [Command] private void CmdCrossFade() => _hero.Animator.CrossFade("PullingHealthCastDelayExit", 0.1f);

    [ClientRpc]
    private void RpcInitEffects(GameObject effectGameObject, GameObject startPoint, GameObject targetPoint)
    {
        if (effectGameObject == null) return;

        PullingHealthEffect[] effects = effectGameObject.GetComponentsInChildren<PullingHealthEffect>();

        foreach (var effect in effects)
        {
            effect.Initialize(startPoint, targetPoint);
            effect.Activate();
        }
    }

    [ClientRpc]
    private void RpcDestroyClientEffect(GameObject effect)
    {
        if (effect != null)
        {
            Debug.Log($"Destroying effect on client: {effect.name}");
            Destroy(effect);
        }

        _activeGhostEffects.Clear();
        ghost.Clear();
    }

    [Command] private void CmdPlayShotSound() => RpcPlayShotSound();
    [Command] private void CmdStopShotSound() => RpcStopShotSound();

    [ClientRpc]
    private void RpcPlayShotSound()
    {
        if (_audioSource != null && audioClip != null) _audioSource.PlayOneShot(audioClip);
    }

    [ClientRpc]
    private void RpcStopShotSound()
    {
        if (_audioSource != null) _audioSource.Stop();
    }
    protected override void ClearData()
    {
        _extraTargets.Clear();
        _extraEffects.Clear();
        Radius = _baseRadius;
    }
}