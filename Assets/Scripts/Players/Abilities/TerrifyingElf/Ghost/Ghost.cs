using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Ghost : Skill
{
    [Header("Ghost Settings")]
    [SerializeField] private float extendedRadius = 5f;
    [SerializeField] private float baseRadius = 3f;
    [SerializeField] private float teleportManaUse = 6f;
    [SerializeField] private int maxGhosts = 2;
    [SerializeField] private MinionComponent ghostPrefab;
    [SerializeField] private GameObject ghostPrefabPreview;
    [SerializeField] private GameObject way;
    [SerializeField] private AudioClip aCTeleportToGhost;
    [SerializeField] private AudioClip aC—ontrolGhostToTarget;
    [SerializeField] private AudioClip aCSummoningGhost;
    [SerializeField] private DrawCircle _extendedRadiusCircle;
    [SerializeField] private Color extendedRadiusColor = new Color(0.8f, 0.3f, 0f);
    [SerializeField] private List<Character> _ghosts;
    [SerializeField] private VisionComponent treeVisionComponent;
    [SerializeField] private VisionComponent heroVisionComponent;
    [SerializeField] private SkillQueue skillQueue;

    private GameObject _ghostPrefabPreview;
    private AudioSource _audioSource;
    private SpawnComponent _spawnComponent;
    private float _baseCastDelay;
    private float _treeVisionRadius;
    private float _heroVisionRadius;
    private float _infinityDistance = 999;
    private bool _isPreviewHiddenOverGhost;
    private bool _ghostMoveToTarget;
    private bool _shouldSpawnGhost;
    private bool _teleportGhost;
    private bool _isSpawningGhostVisual;
    private Vector3 _spawnPosition;
    private Character _ghostToMove;
    private Character _targetCharacter;
    private Character _ghostToTeleport;
    private Coroutine _checkExtendedRadiusCoroutine;
    private Coroutine _teleportAnimationCoroutine;
    private Coroutine _waitTeleportJob;
    private Coroutine _boostWindow;
    private List<GrowTreeAura> _allGrowTrees = new();

    private readonly Queue<Vector3> _pendingSpawn = new();
    private readonly Queue<Character> _teleportQueue = new();

    #region Talent
    private bool _sendingGhostTargetTalentActive;
    private bool _cooldownGhostShotActive;
    private bool _effectsInnerDarknessTalent;
    private bool _movingToGhostWithZeroMana;
    private bool _passingThroughGhost;
    private bool _isPullingHealthGostTeleport;
    private bool _isGhostSpawnInRadiusTree;
    #endregion

    private bool _isWaitingTeleport;
    private bool isSkillEnableBoostLogic;

    public event Action<Character, Vector3> Teleported;

    protected override int AnimTriggerCastDelay => Animator.StringToHash("GhostCastDelay");
    protected override int AnimTriggerCast => 0;

    protected override bool IsCanCast
    {
        get
        {
            if (_shouldSpawnGhost) return IsHaveCharge && (_chargesHaveSeparateCooldown || IsCooldowned);

            if (_ghostMoveToTarget) return true;

            if (_teleportGhost && _ghostToTeleport != null)
            {
                if (!_isGhostSpawnInRadiusTree) return IsWithinRadius(_ghostToTeleport.transform.position, extendedRadius);
                if (_isGhostSpawnInRadiusTree) return IsWithinRadius(_ghostToTeleport.transform.position, _infinityDistance); 
            }

            return false;
        }
    }

    public bool CooldownGhostShotActive => _cooldownGhostShotActive;
    public List<Character> GhostTarget { get => _ghosts; set => _ghosts = value; }

    #region Talents

    public void EffectsInnerDarknessTalentActive(bool value) => _effectsInnerDarknessTalent = value;
    public void SendingGhostTargetTalentActive(bool value) => _sendingGhostTargetTalentActive = value;
    public void CooldownGhostShotActiveTalent(bool value) => _cooldownGhostShotActive = value;
    public void MovingToGhostWithZeroMana(bool value) => _movingToGhostWithZeroMana = value;
    public void PassingThroughGhost(bool value) => _passingThroughGhost = value;
    public void PullingHealthGostTeleport(bool value) => _isPullingHealthGostTeleport = value;
    public void GhostSpawnInRadiusTree(bool value)
    {
        _isGhostSpawnInRadiusTree = value;

        if (_isGhostSpawnInRadiusTree) Radius = 0;
        else Radius = baseRadius;
    }

    #endregion

    protected override void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _treeVisionRadius = treeVisionComponent.VisionRange;
        _heroVisionRadius = Hero.VisionComponent.VisionRange;

        base.Awake();
        InitializeFields();
        RegisterSpawnEvents();

        if (_isGhostSpawnInRadiusTree) Radius = 0;
        else Radius = baseRadius;

        if (_extendedRadiusCircle == null) _extendedRadiusCircle = GetComponentInChildren<DrawCircle>(true);
    }

    protected override void SkillEnableBoostLogic()
    {
        isSkillEnableBoostLogic = true;
    }
    protected override void SkillDisableBoostLogic()
    {
        isSkillEnableBoostLogic = false;
    }

    private void OnDestroy()
    {
        UnregisterSpawnEvents();
        if (_checkExtendedRadiusCoroutine != null)
        {
            StopCoroutine(_checkExtendedRadiusCoroutine);
            _checkExtendedRadiusCoroutine = null;
        }
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        if (targetInfo != null && targetInfo.Points.Count > 0)
        {
            _spawnPosition = targetInfo.Points[0];
            _shouldSpawnGhost = true;
        }
    }

    public void TryStartGhostBoostWindow() => _boostWindow = StartCoroutine(GhostBoostWindow());

    private void HideExtendedRadius()
    {
        if (_extendedRadiusCircle != null) _extendedRadiusCircle.Clear();
    }

    private void InitializeFields()
    {
        _baseCastDelay = CastDeley;
        _ghosts = new List<Character>();
        _spawnComponent = GetComponent<SpawnComponent>();
    }

    private bool IsNearGrowTree(Vector3 point, float radius = 1f)
    {
        var hits = Physics.OverlapSphere(point, radius);
        for (int i = 0; i < hits.Length; i++) if (hits[i].GetComponentInParent<GrowTreeAura>() != null) return true;
        return false;
    }

    private void RegisterSpawnEvents()
    {
        if (_spawnComponent != null) _spawnComponent.UnitAdded += OnGhostSpawned;
    }

    private void UnregisterSpawnEvents()
    {
        if (_spawnComponent != null) _spawnComponent.UnitAdded -= OnGhostSpawned;
    }

    private IEnumerator GhostBoostWindow()
    {
        EnableSkillBoost();
        yield return new WaitForSeconds(3f);
        DisableSkillBoost();
        _boostWindow = null;
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {

        if (_checkExtendedRadiusCoroutine != null) StopCoroutine(_checkExtendedRadiusCoroutine);
        if (!_isGhostSpawnInRadiusTree) _checkExtendedRadiusCoroutine = StartCoroutine(CheckExtendedRadiusJob());
        else _allGrowTrees = FindObjectsOfType<GrowTreeAura>().ToList();

        Vector3 mousePositionStart = GetMousePoint();
        _ghostPrefabPreview = Instantiate(ghostPrefabPreview, mousePositionStart, Quaternion.identity);
        _isPreviewHiddenOverGhost = false;

        while (!Disactive)
        {
            Vector3 mousePosition = GetMousePoint();
            _teleportGhost = false;
            bool isHoveringGhost = IsMouseOverGhost(out Character ghostPreview) && ghostPreview.GetComponent<GhostAura>();

            if (_ghostPrefabPreview)
            {

                if (isHoveringGhost && !_isPreviewHiddenOverGhost)
                {
                    _ghostPrefabPreview.SetActive(false);
                    _isPreviewHiddenOverGhost = true;
                }

                else if (!isHoveringGhost && _isPreviewHiddenOverGhost)
                {
                    _ghostPrefabPreview.SetActive(true);
                    _isPreviewHiddenOverGhost = false;
                }

                if (_ghostPrefabPreview.activeSelf) _ghostPrefabPreview.transform.position = mousePosition;
            }

            if (_sendingGhostTargetTalentActive && IsMouseOverTarget(out Character character) && character.CharacterState.CheckForState(States.InnerDarkness))
            {
                if (Input.GetMouseButtonDown(0) && IsWithinRadius(character.transform.position, Radius) && !GetComponent<GhostAura>())
                {
                    if (_ghosts.Count > 0)
                    {
                        _ghostToMove = _ghosts.Count > 1 ? _ghosts[_ghosts.Count - 2] : _ghosts[0];
                        _targetCharacter = character;
                        _ghostMoveToTarget = true;
                    }

                    yield break;
                }
            }

            else if (isHoveringGhost && !Hero.CharacterState.CheckForState(States.Bound))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (!_isGhostSpawnInRadiusTree && !IsWithinRadius(ghostPreview.transform.position, extendedRadius))
                    {
                        _teleportQueue.Enqueue(ghostPreview);
                        if (!_isWaitingTeleport) StartCoroutine(WaitTeleportQueueCoroutine());
                        ClearData();
                        yield break;
                    }

                    _teleportGhost = true;
                    _ghostToTeleport = ghostPreview;
                    TeleportToGhost(_ghostToTeleport);
                    ClearData();
                    TryCancel();
                    yield return new WaitForSeconds(0.1f);
                    yield break;
                }
            }

            else
            {
                if (Input.GetMouseButtonDown(0) && !_teleportGhost && !IsMouseOverTarget(out _))
                {
                    var point = GetMousePoint();
                    if (point == Vector3.zero) { yield return null; continue; }

                    bool heroCanSee = IsWithinRadius(point, _heroVisionRadius);
                    bool treeCanSee = _allGrowTrees.Any(tree => IsWithinRadius(tree.transform.position, point, _treeVisionRadius));
                    bool canSpawnHere = (_isGhostSpawnInRadiusTree && (IsNearGrowTree(point, 1f) || IsVisibleToHero(point))) || (!_isGhostSpawnInRadiusTree && IsMouseInRadius(Radius));

                    if (!canSpawnHere) { yield return null; continue; }

                    if (isSkillEnableBoostLogic)
                    {
                        if (_isSpawningGhostVisual) _pendingSpawn.Enqueue(point);
                        else StartCoroutine(SpawnGhostVisualEffect(point));

                        yield return new WaitForSeconds(0.1f);
                        yield return null;
                    }

                    else if (IsHaveCharge && (_chargesHaveSeparateCooldown || IsCooldowned))
                    {
                        if (_isSpawningGhostVisual) _pendingSpawn.Enqueue(point);
                        else
                        {
                            _spawnPosition = point;
                            _shouldSpawnGhost = true;
                            AdjustCastDelay();
                            yield break;
                        }
                    }
                }
            }

            yield return null;
        }

        if (_ghostPrefabPreview != null) Destroy(_ghostPrefabPreview);

        TargetInfo targetInfo = new TargetInfo();
        targetInfo.Points.Add(_spawnPosition);
        callbackDataSaved(targetInfo);
    }

    #region SendingGhostTarget

    private bool IsMouseOverTarget(out Character target)
    {
        target = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _targetsLayers))
        {
            target = hit.collider.GetComponent<Character>();
            return target != null;
        }

        return false;
    }

    private IEnumerator MoveGhostToCharacter(Character ghost, Character target)
    {
        if (ghost == null || target == null) yield break;
        if (!(ghost is MinionComponent minion)) yield break;
        if (!ghost.TryGetComponent<NavMeshAgent>(out var agent)) yield break;
        if (!TryConsumeMana(teleportManaUse)) yield break;

        agent.stoppingDistance = 1.5f;
        agent.updateRotation = true;

        agent.SetDestination(target.transform.position);

        while (true)
        {
            if (target == null)
            {
                agent.ResetPath();
                yield break;
            }

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    OnGhostReachedTarget(minion, target);
                    yield break;
                }
            }

            agent.SetDestination(target.transform.position);
            _ghostMoveToTarget = false;

            yield return null;
        }
    }

    private void OnGhostReachedTarget(Character ghost, Character target)
    {
        if (ghost == null || target == null) return;

        if (!(ghost is MinionComponent minion)) return;

        var characterState = target.GetComponent<CharacterState>();
        if (characterState != null)
        {
            int innerDarknessStacks = characterState.CheckStateStacks(States.InnerDarkness);

            if (innerDarknessStacks > 0)
            {
                float randomFearDuration = UnityEngine.Random.Range(0.4f, 0.6f) * innerDarknessStacks;
                characterState.AddState(States.Fear, randomFearDuration, 0, gameObject, "Ghost");
                //CmdAddFear(characterState, randomFearDuration);
            }

            else if (innerDarknessStacks == 0) characterState.AddState(States.Fear, UnityEngine.Random.Range(0.4f, 0.6f), 0, gameObject, "Ghost");
        }

        CmdAc—ontrolGhostToTarget();
        _ghosts.Remove(ghost);
        Destroy(ghost.gameObject);
    }


    //[Command]
    //private void CmdAddFear(CharacterState characterState, float randomFearDuration)
    //{
    //    characterState.AddState(States.Fear, randomFearDuration, 0, gameObject, "Ghost");
    //}


    #endregion

    private void AdjustCastDelay()
    {
        if (_ghosts.Count == 0) _castDeley = _baseCastDelay * 0.5f;

        else if (_ghosts.Count >= maxGhosts) _castDeley = _baseCastDelay * 2f;
    }

    private void TeleportToGhost(Character ghost)
    {
        if (ghost == null || !(ghost is MinionComponent)) return;

        CmdAcTeleportToGhost();
        ReduceSkillCosts();
        ActivateWayIndicator();

        if (ghost.TryGetComponent<GhostAura>(out GhostAura ghostAura)) PerformTeleport(ghost.transform.position);
        if (manaTeleportToGhost() || !_movingToGhostWithZeroMana) RemoveGhost(ghost);
        RestoreSkillCosts();
    }

    private void ReduceSkillCosts()
    {
        foreach (var skillCost in _skillEnergyCosts)
        {
            skillCost.resourceCost *= 0.5f;
        }
    }

    private void RestoreSkillCosts()
    {
        foreach (var skillCost in _skillEnergyCosts)
        {
            skillCost.resourceCost *= 2f;
        }
    }

    private void ActivateWayIndicator()
    {
        way.SetActive(true);
    }

    private void PerformTeleport(Vector3 targetPosition)
    {
        var moveComponent = GetComponent<MoveComponent>();
        moveComponent?.TeleportToPositionSmooth(targetPosition, 0.5f);

        if (_isPullingHealthGostTeleport) Teleported?.Invoke(Hero, targetPosition);

        if (_teleportAnimationCoroutine != null) StopCoroutine(_teleportAnimationCoroutine);
        _teleportAnimationCoroutine = StartCoroutine(PlayTeleportMoveAnimation(targetPosition));

        StartCoroutine(DisableWayAfterTeleport(moveComponent, targetPosition));
    }

    private void RemoveGhost(Character ghost)
    {
        if (!(ghost is MinionComponent minion)) return;

        _spawnComponent.CmdRemoveUnit(minion);
        _ghosts.Remove(ghost);
    }

    private IEnumerator DisableWayAfterTeleport(MoveComponent moveComponent, Vector3 targetPosition)
    {
        while (Vector3.Distance(moveComponent.transform.position, targetPosition) > 0.1f)
        {
            yield return null;
        }

        way.SetActive(false);
    }

    private void SpawnGhost(Vector3 position, Quaternion LookRotation)
    {
        if (_spawnComponent == null) return;
        Vector3 spawnPosition = position + Vector3.up * 1f;
        _spawnComponent.CmdSpawnAliesPoint(spawnPosition, LookRotation, null,  0, false, Hero);
    }

    private void RemoveOldestGhostIfNeeded()
    {
        if (_ghosts.Count >= maxGhosts)
        {
            var oldestGhost = _ghosts.FirstOrDefault();
            if (oldestGhost != null)
            {
                _ghosts.Remove(oldestGhost);
                _spawnComponent.CmdRemoveUnit(oldestGhost);
            }
            else
            {
                Debug.LogWarning("SpawnGhost: No valid ghost found to remove.");
            }
        }
    }

    private void OnGhostSpawned(Character ghost)
    {
        if (ghost == null || _ghosts.Contains(ghost) || !(ghost is MinionComponent)) return;
        _ghosts.Add(ghost);

        if (ghost.TryGetComponent<GhostAura>(out var ghostAura))
        {
            if (_effectsInnerDarknessTalent) ghostAura.EffectsInnerDarknessTalent = true;
            if (_passingThroughGhost) ghostAura.PassingThroughGhost = true;
        }

    }

    private bool IsMouseOverGhost(out Character ghost)
    {
        ghost = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _targetsLayers))
        {
            ghost = _ghosts.FirstOrDefault(unit => unit != null && unit.gameObject == hit.collider.gameObject);
            return ghost != null;
        }

        return false;
    }

    private bool IsWithinRadius(Vector3 targetPosition, float radius) => Vector3.Distance(transform.position, targetPosition) <= radius;
    private bool IsWithinRadius(Vector3 center, Vector3 point, float radius) => Vector3.Distance(center, point) <= radius;

    private bool IsVisibleToHero(Vector3 point)
    {
        Vector3 direction = point - transform.position;
        if (Physics.Raycast(transform.position + Vector3.up, direction.normalized, out var hit, direction.magnitude)) if (hit.collider.GetComponent<Character>() == Hero) return true;
        return false;
    }

    private bool TryConsumeMana(float amount)
    {
        var manaResource = _hero.Resources.FirstOrDefault(r => r.Type == ResourceType.Mana);
        if (manaResource != null && manaResource.CurrentValue >= amount)
        {
            manaResource.CmdUse(amount);
            return true;
        }

        return false;
    }

    private IEnumerator SpawnGhostVisualEffect(Vector3 targetPosition)
    {
        _isSpawningGhostVisual = true;

        RemoveOldestGhostIfNeeded();
        CmdAcSummoningGhost();

        Vector3 spawnDirection = (targetPosition - transform.position).normalized;
        float offsetDistance = Radius - 1;
        Vector3 spawnStartPosition = targetPosition - spawnDirection * offsetDistance;

        var ghostVisual = Instantiate(ghostPrefabPreview, spawnStartPosition, Quaternion.identity);

        if (ghostVisual.TryGetComponent<Collider>(out var collider)) collider.enabled = false;
        yield return MoveGhostVFXToPoint(ghostVisual.transform, targetPosition);
        Destroy(ghostVisual.gameObject);

        SpawnGhost(targetPosition, ghostVisual.transform.rotation);

        _shouldSpawnGhost = false;

        if (_pendingSpawn.Count > 0 && IsHaveCharge && (isSkillEnableBoostLogic || _chargesHaveSeparateCooldown || IsCooldowned)) StartCoroutine(SpawnGhostVisualEffect(_pendingSpawn.Dequeue()));
        else _isSpawningGhostVisual = false;
    }

    private IEnumerator MoveGhostVFXToPoint(Transform vfx, Vector3 target)
    {
        const float moveDuration = 1f;
        float time = 0;
        Vector3 start = vfx.position;
        Vector3 end = target + Vector3.up;

        while (time < moveDuration)
        {
            time += Time.deltaTime;
            vfx.position = Vector3.Lerp(start, end, time / moveDuration);
            yield return null;
        }
    }

    private IEnumerator PlayTeleportMoveAnimation(Vector3 targetPosition)
    {
        var moveComponent = GetComponent<MoveComponent>();
        if (moveComponent == null)
            yield break;

        Vector3 lastPosition = transform.position;

        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            Vector3 currentPosition = transform.position;
            Vector3 fakeVelocity = (currentPosition - lastPosition) / Time.deltaTime;
            lastPosition = currentPosition;

            moveComponent.SetAnimationMovement(fakeVelocity);

            yield return null;
        }

        moveComponent.SetAnimationMovement(Vector3.zero);
        _teleportAnimationCoroutine = null;
    }

    private IEnumerator CheckExtendedRadiusJob()
    {
        while (true)
        {
            bool ghostWithAuraInExtendedRadius = _ghosts.Any(ghost =>
                ghost != null &&
                ghost.GetComponent<GhostAura>() != null &&
                IsWithinRadius(ghost.transform.position, extendedRadius));

            if (_extendedRadiusCircle != null)
            {
                var color = ghostWithAuraInExtendedRadius ? Color.green : extendedRadiusColor;
                _extendedRadiusCircle.SetColor(color);
                _extendedRadiusCircle.Draw(extendedRadius);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator WaitTeleportQueueCoroutine()
    {
        _isWaitingTeleport = true;

        while (_teleportQueue.Count > 0)
        {
            Character character = _teleportQueue.Count > 0 ? _teleportQueue.Dequeue() : null;

            if (character == null) continue;

            if (_isGhostSpawnInRadiusTree)
            {
                TeleportToGhost(character);
                skillQueue.TryCancel();
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            while (character != null && !IsWithinRadius(character.transform.position, extendedRadius))
                yield return null;

            if (character != null) TeleportToGhost(character);
            skillQueue.TryCancel();
            yield return new WaitForSeconds(0.2f);
        }

        _isWaitingTeleport = false;
    }

    protected override IEnumerator CastJob()
    {
        if (_shouldSpawnGhost && _spawnPosition != Vector3.zero && TryConsumeMana(12)) StartCoroutine(SpawnGhostVisualEffect(_spawnPosition));
        else if (_ghostMoveToTarget && _ghostToMove != null && _targetCharacter != null) StartCoroutine(MoveGhostToCharacter(_ghostToMove, _targetCharacter));

        yield return null;
    }

    protected override void ClearData()
    {
        _teleportQueue.Clear();
        _isWaitingTeleport = false;
        HideExtendedRadius();

        if (_checkExtendedRadiusCoroutine != null)
        {
            StopCoroutine(_checkExtendedRadiusCoroutine);
            _checkExtendedRadiusCoroutine = null;
        }

        if (_waitTeleportJob != null)
        {
            StopCoroutine(_waitTeleportJob);
            _waitTeleportJob = null;
        }

        _pendingSpawn.Clear();
        _isSpawningGhostVisual = false;

        if (_ghostPrefabPreview != null)
        {
            Destroy(_ghostPrefabPreview);
            _ghostPrefabPreview = null;
        }
    }

    private bool manaTeleportToGhost()
    {
        var manaResource = Hero.TryGetResource(ResourceType.Mana);
        return manaResource.CurrentValue > 0;
    }

    [Command]
    private void CmdAcSummoningGhost()
    {
        RpcAcSummoningGhost();
    }

    [Command]
    private void CmdAcTeleportToGhost()
    {
        RpcAcTeleportToGhost();
    }

    [Command]
    private void CmdAc—ontrolGhostToTarget()
    {
        RpcAc—ontrolGhostToTarget();
    }


    [ClientRpc]
    private void RpcAcSummoningGhost()
    {
        if (_audioSource != null && aCSummoningGhost != null) _audioSource.PlayOneShot(aCSummoningGhost);
    }

    [ClientRpc]
    private void RpcAcTeleportToGhost()
    {
        if (_audioSource != null && aCTeleportToGhost != null) _audioSource.PlayOneShot(aCTeleportToGhost);
    }

    [ClientRpc]
    private void RpcAc—ontrolGhostToTarget()
    {
        if (_audioSource != null && aC—ontrolGhostToTarget != null) _audioSource.PlayOneShot(aC—ontrolGhostToTarget);
    }
}