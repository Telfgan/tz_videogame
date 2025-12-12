using Mirror;
using System;
using System.Collections;
using UnityEngine;

public class GhostAura : Skill
{
    [Header("Ghost Aura Settings")]
    [SerializeField] private float fearChanceInRadius = 0.2f;
    [SerializeField] private float fearChanceInsideCollider = 0.8f;
    [SerializeField] private float tickInterval = 3f;
    [SerializeField] private States GhostState = States.Fear;
    [SerializeField] private float duration = 5f;
    [SerializeField] private float delayBeforeEffect = 5f;

    private Collider _auraCollider;
    private Coroutine _zoneEffectCoroutine;
    private Coroutine _colliderEffectCoroutine;
    private float _spawnTime;

    private bool _effectsDarknessTalent;
    private bool _passingThroughGhost;

    protected override int AnimTriggerCastDelay => throw new System.NotImplementedException();
    protected override int AnimTriggerCast => throw new System.NotImplementedException();
    protected override bool IsCanCast => throw new System.NotImplementedException();

    public bool EffectsInnerDarknessTalent { get => _effectsDarknessTalent; set => _effectsDarknessTalent = value; }
    public bool PassingThroughGhost { get => _passingThroughGhost; set => _passingThroughGhost = value; }

    private void Start()
    {
        _auraCollider = GetComponent<Collider>();
        if (_auraCollider == null)
        {
            Debug.LogError("GhostAura requires a Collider component!");
            enabled = false;
        }

        _spawnTime = Time.time;
    }

    [Server]
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & TargetsLayers) != 0 && Time.time > _spawnTime + delayBeforeEffect)
        {
            if (other.TryGetComponent<CharacterState>(out var characterState) && other.gameObject != gameObject)
            {
                float adjustedDuration = CalculateAdjustedSilentDuration(characterState);
                ApplySilent(characterState, fearChanceInsideCollider, adjustedDuration);

                if (_colliderEffectCoroutine == null && _passingThroughGhost) _colliderEffectCoroutine = StartCoroutine(ApplyEffectsInCollider());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_colliderEffectCoroutine != null && !HasObjectsInCollider())
        {
            StopCoroutine(_colliderEffectCoroutine);
            _colliderEffectCoroutine = null;
        }
    }

     protected override void Awake()
    {
        if (_zoneEffectCoroutine == null && _passingThroughGhost) _zoneEffectCoroutine = StartCoroutine(ApplyEffectsInZone());
    }

    private IEnumerator ApplyEffectsInZone()
    {
        while (true)
        {
            yield return new WaitForSeconds(tickInterval);

            bool hasObjectsInZone = false;
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, Radius, TargetsLayers);

            foreach (var collider in hitColliders)
            {
                if (collider.TryGetComponent<CharacterState>(out var characterState) && collider.gameObject != gameObject)
                {
                    float adjustedDuration = CalculateAdjustedSilentDuration(characterState);
                    ApplySilent(characterState, fearChanceInRadius, adjustedDuration);
                    hasObjectsInZone = true;
                }
            }

            if (!hasObjectsInZone)
            {
                StopCoroutine(_zoneEffectCoroutine);
                _zoneEffectCoroutine = null;
                yield break;
            }
        }
    }

    private IEnumerator ApplyEffectsInCollider()
    {
        while (true)
        {
            yield return new WaitForSeconds(tickInterval);

            bool hasObjectsInCollider = false;
            Collider[] colliders = Physics.OverlapBox(_auraCollider.bounds.center, _auraCollider.bounds.extents, Quaternion.identity, TargetsLayers);

            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent<CharacterState>(out var characterState) && collider.gameObject != gameObject)
                {
                    ApplySilent(characterState, fearChanceInsideCollider, CalculateAdjustedSilentDuration(characterState));
                    hasObjectsInCollider = true;
                }
            }

            if (!hasObjectsInCollider)
            {
                StopCoroutine(_colliderEffectCoroutine);
                _colliderEffectCoroutine = null;
                yield break;
            }
        }
    }

    private bool HasObjectsInCollider()
    {
        Collider[] colliders = Physics.OverlapBox(_auraCollider.bounds.center, _auraCollider.bounds.extents, Quaternion.identity, TargetsLayers);
        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent<CharacterState>(out var characterState) && collider.gameObject != gameObject)
            {
                return true;
            }
        }
        return false;
    }

    private void ApplySilent(CharacterState characterState, float chance, float duration)
    {
        if (UnityEngine.Random.value <= chance)
        {
            characterState.AddState(GhostState, duration, 0f, gameObject, Name);
        }
    }

    private float CalculateAdjustedSilentDuration(CharacterState characterState)
    {
        if (_effectsDarknessTalent && characterState.CheckForState(States.InnerDarkness))
        {
            int innerDarknessStacks = characterState.CheckStateStacks(States.InnerDarkness);

            float durationMultiplier = 1.4f + 0.1f * (innerDarknessStacks - 1);
            duration = durationMultiplier;
        }

        return duration;
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        throw new NotImplementedException();
    }

    #region Talents

    #endregion

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        throw new NotImplementedException();
    }

    protected override IEnumerator CastJob()
    {
        throw new System.NotImplementedException();
    }

    protected override void ClearData()
    {
        throw new System.NotImplementedException();
    }
}
