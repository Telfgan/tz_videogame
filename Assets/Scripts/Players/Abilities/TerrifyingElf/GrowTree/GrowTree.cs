using UnityEngine.SceneManagement;
using Mirror;
using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class GrowTree : Skill
{
    [Header("GrowTree Settings")]
    [SerializeField] private float extendedRadius = 8f;
    [SerializeField] private float _moveDuration = 0.5f;
    [SerializeField] private float arrowEffectLifetime = 2;
    [SerializeField] private GrowTreeAura _treePrefab;
    [SerializeField] private MoveComponent moveComponent;
    [SerializeField] private List<GrowTreeAura> _activeTrees;
    [SerializeField] private ObjectData treeData;
    [SerializeField] private DrawCircle _extendedRadiusCircle;
    [SerializeField] private Color extendedRadiusColor = new Color(0.8f, 0.3f, 0f);
    [SerializeField] private ShotsIntoSky shotsIntoSky;
    [SerializeField] private ShotIntoSky shotIntoSky;
    [SerializeField] private GameObject arrowWithTreeEffect;
    [SerializeField] private ParticleSystem arrowIntoSkyEffect;

    [Header("Talents")]
    //[SerializeField] private bool treeHealthTalent; // Созданное дерево каждые 0,3 сек увеличивает максималньый запас здоровья на 1 ед. Вплоть до 60 сек.
    private bool growTreeIncreasesMaxHealth;
    private bool treeMagicEvadeTalent;
    private bool treeShotCooldownTalent;
    private bool _isGrowTreeArrowIntoSkyRadiusTalent;

    [Header("Raycast masks")]
    [SerializeField] private LayerMask groundLayer;

    private Vector3 _targetPoint = Vector3.positiveInfinity;
    private Vector3 point = Vector3.positiveInfinity;
    private GrowTreeAura _currentTree;
    private ObjectHealth _healthTree;
    private float baseHealth;
    private float baseCastStreamDuration;
    private float _baseCastDelay;
    private Coroutine _treeHealthCoroutine;
    private Coroutine _rangeWatch;
    private Coroutine _checkExtendedRadiusCoroutine;
    private Coroutine _arrowFxRoutine;
    private bool _isSpawnHero;
    private bool _arrowFxPressLatch;
    private bool _castFromExtendedRadius;

    protected override bool IsCanCast => !float.IsPositiveInfinity(_targetPoint.x) && IsPointInRadius(extendedRadius, _targetPoint);

    private int _growHash = Animator.StringToHash("GrowTreeCastDelay");
    private int _shotHash = Animator.StringToHash("ShotSkyWithTreeCastDelay");

    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerCast => 0;

    public void ArrowIntoSkyWithTreeEffectPlay() => arrowIntoSkyEffect.Play();

    private void Start()
    {
        _baseCastDelay = CastDeley;
        baseHealth = treeData.MaxHealth;
        baseCastStreamDuration = CastStreamDuration;
    }

    private void OnEnable() => OnSkillCanceled += HandleSkillCanceled;
    private void OnDestroy() => OnSkillCanceled -= HandleSkillCanceled;

    private void ShowExtendedRadius()
    {
        if (_extendedRadiusCircle == null) _extendedRadiusCircle = GetComponentInChildren<DrawCircle>(true);
    }

    private void HideExtendedRadius()
    {
        if (_extendedRadiusCircle != null) _extendedRadiusCircle.Clear();
    }

    private void StopRangeWatch()
    {
        if (_rangeWatch != null)
        {
            StopCoroutine(_rangeWatch);
            _rangeWatch = null;
        }
    }

    private void SpawnArrowWithTreeEffect(Vector3 point)
    {
        if (!arrowWithTreeEffect) return;

        Vector3 direction = point - transform.position;
        direction.y = 0f;
        Quaternion rotation = direction.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(direction) : Quaternion.identity;

        var effect = Instantiate(arrowWithTreeEffect, point, rotation);
        SceneManager.MoveGameObjectToScene(effect, gameObject.scene);
        Destroy(effect, arrowEffectLifetime);
    }

    private void ResetData()
    {
        _isSpawnHero = false;
        _currentTree = null;
        _castFromExtendedRadius = false;
        CastDeley = _baseCastDelay;
        point = Vector3.positiveInfinity;

        if (_arrowFxRoutine != null)
        {
            StopCoroutine(_arrowFxRoutine);
            _arrowFxRoutine = null;
        }
    }

    private IEnumerator ISpawnArrowWithTreeEffect()
    {
        yield return new WaitForSeconds(CastStreamDuration / 5);
        if (_castFromExtendedRadius) SpawnArrowWithTreeEffect(point);
    }

    private IEnumerator CheckExtendedRadiusJob()
    {
        while (true)
        {
            if (_extendedRadiusCircle == null)
            {
                yield return null;
                continue;
            }

            //float extRadius = (_shotIntoSky != null) ? _shotIntoSky.Radius : 0f;

            //if (extRadius <= 0f)
            //{
            //    _extendedRadiusCircle.Clear();
            //    yield return null;
            //    continue;
            //}

            Vector3 mousePoint = GetMousePointOnLayer(groundLayer);
            bool cursorInside = !float.IsPositiveInfinity(mousePoint.x) && Vector3.Distance(mousePoint, transform.position) <= extendedRadius;

            _extendedRadiusCircle.SetColor(cursorInside ? Color.green : extendedRadiusColor);
            _extendedRadiusCircle.Draw(extendedRadius);

            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator CastDistanceWatcher()
    {
        const float checkInterval = 0.1f;
        var wait = new WaitForSeconds(checkInterval);

        try
        {
            while (true)
            {
                if (_hero == null) yield break;

                float allowed = _castFromExtendedRadius ? extendedRadius : Radius;
                float allowedSqr = allowed * allowed;

                Vector3 heroPos = _hero.transform.position;
                Vector3 anchor = _currentTree != null ? _currentTree.transform.position : _targetPoint;

                if ((heroPos - anchor).sqrMagnitude > allowedSqr)
                {
                    TryCancel();
                    ResetData();
                    break;
                }

                yield return wait;
            }
        }
        finally
        {
            _rangeWatch = null;
        }
    }


    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        TreeHealthTalentEnter();

        _activeTrees.RemoveAll(tree => tree == null);
        CmdRemoveTree();

        if (_isGrowTreeArrowIntoSkyRadiusTalent)
        {
            ShowExtendedRadius();
            if (_checkExtendedRadiusCoroutine != null) StopCoroutine(_checkExtendedRadiusCoroutine);
            _checkExtendedRadiusCoroutine = StartCoroutine(CheckExtendedRadiusJob());
        }

        int treeCount = _activeTrees.Count;
        CastStreamDuration = treeCount == 0 ? baseCastStreamDuration : baseCastStreamDuration * Mathf.Pow(2, treeCount);

        while (float.IsPositiveInfinity(_targetPoint.x) && !_disactive)
        {
            if (!GetMouseButton) _arrowFxPressLatch = false;

            if (GetMouseButton && !_arrowFxPressLatch)
            {
                _arrowFxPressLatch = true;

                var clickedCharacter = GetClickedCharacter(Hero);

                if (clickedCharacter != null && clickedCharacter == _hero)
                {
                    _targetPoint = _hero.transform.position;
                    _isSpawnHero = true;
                }
                else
                {
                    point = GetMousePointOnLayer(groundLayer);

                    if (!float.IsPositiveInfinity(point.x))
                    {
                        float dist = Vector3.Distance(transform.position, point);

                        if (dist <= Radius)
                        {
                            _targetPoint = point;
                            _castFromExtendedRadius = false;
                        }        

                        else if (dist <= extendedRadius && _isGrowTreeArrowIntoSkyRadiusTalent)
                        {
                            if (shotIntoSky != null && !shotIntoSky.IsCooldowned && !shotIntoSky.Disactive)
                            {
                                yield return null;
                                continue;
                            }

                            _targetPoint = point;
                            _castFromExtendedRadius = true;
                            CastDeley += arrowEffectLifetime;
                            SpawnArrowWithTreeEffect(point);

                            if (shotIntoSky != null && shotIntoSky.IsUseCharges) shotIntoSky.TryUseCharge();
                            else if (shotIntoSky != null) shotIntoSky.IncreaseSetCooldown(shotIntoSky.CooldownTime);
                        }
                    }
                }
            }

            yield return null;
        }

        int nearCount = _activeTrees.Count(tree => tree != null && Vector3.Distance(tree.transform.position, _targetPoint) <= Radius);
        CastStreamDuration = nearCount == 0 ? baseCastStreamDuration : baseCastStreamDuration * Mathf.Pow(2, nearCount);

        CmdSetCastStreamDurationByProximity(_targetPoint, Radius);

        if (_checkExtendedRadiusCoroutine != null)
        {
            StopCoroutine(_checkExtendedRadiusCoroutine);
            _checkExtendedRadiusCoroutine = null;
        }
        HideExtendedRadius();

        DrawDamageZone(_targetPoint);

        if (_castFromExtendedRadius)
        {
            _hero.Animator.SetTrigger(_shotHash);
            _hero.NetworkAnimator.SetTrigger(_shotHash);
        }

        TargetInfo targetInfo = new TargetInfo();
        targetInfo.Points.Add(_targetPoint);
        callbackDataSaved(targetInfo);
    }

    protected override IEnumerator CastJob()
    {
        if (_treePrefab == null) yield break;

        if (_rangeWatch != null)
        {
            StopCoroutine(_rangeWatch);
            _rangeWatch = null;
        }

        Vector3 spawnPos = _targetPoint;


        if (!_castFromExtendedRadius)
        {
            _hero.Animator.SetTrigger(_growHash);
            _hero.NetworkAnimator.SetTrigger(_growHash);

            yield return new WaitForSeconds(CastStreamDuration / 3);
        }

        StopDamageZone();

        if (_isSpawnHero) CmdSpawnTreeAndTeleport(_hero.transform.position);
        else CmdSpawnTree(spawnPos, _castFromExtendedRadius);

        if (!_castFromExtendedRadius) yield return new WaitForSeconds(CastStreamDuration / 1.5f);
        else yield return new WaitForSeconds(CastStreamDuration);

        if (_castFromExtendedRadius)
        {
            _hero.Animator.ResetTrigger(_shotHash);
            _hero.NetworkAnimator.ResetTrigger(_shotHash);
        }
        else
        {
            _hero.Animator.ResetTrigger(_growHash);
            _hero.NetworkAnimator.ResetTrigger(_growHash);

            CmdCrossFade();
            _hero.Animator.CrossFade("GrowTreeCastDelayExit", 0.1f);
        }

        ResetData();
        StopRangeWatch();   
    }

    protected Vector3 GetMousePointOnLayer(LayerMask layer, float y = 0f)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, layer))
        {
            Vector3 point = hit.point;
            point.y = y;
            return point;
        }

        return Vector3.positiveInfinity;
    }

    #region Canceling a skill
    private void HandleSkillCanceled()
    {
        StopDamageZone();
        StopRangeWatch();

        if (_checkExtendedRadiusCoroutine != null)
        {
            StopCoroutine(_checkExtendedRadiusCoroutine);
            _checkExtendedRadiusCoroutine = null;
        }
        HideExtendedRadius();

        if (_hero != null && _hero.Move != null) Hero.Animator.speed = 1;
        TreeHealthTalentExit();

        if (_currentTree != null) CmdRequestInterruptTree(_currentTree.netId);

        ResetData();
    }
    #endregion

    #region Auxiliary methods
    private Character GetClickedCharacter(Character hero)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            Vector3 clickPoint = hit.point;
            Collider[] hits = Physics.OverlapSphere(clickPoint, Area, TargetsLayers);
            if (hits.Length == 0) return null;
            foreach (Collider target in hits) if (target.TryGetComponent(out Character character) && character == hero) return character;
        }

        return null;
    }
    #endregion

    #region [Command] / Spawn
    [Command] private void CmdSetMaxHealth(float maxHealth) => treeData.MaxHealth = maxHealth;
    [Command] private void CmdRemoveTree() => _activeTrees.RemoveAll(tree => tree == null);

    [Command]
    private void CmdSpawnTree(Vector3 position, bool castFromExtendedRadius)
    {
        var tree = Instantiate(_treePrefab, position, Quaternion.identity);
        _currentTree = tree;

        SceneManager.MoveGameObjectToScene(_currentTree.gameObject, Hero.NetworkSettings.MyRoom);
        NetworkServer.Spawn(_currentTree.gameObject, connectionToClient);

        _healthTree = tree.GetComponentInChildren<ObjectHealth>();
        if (_healthTree != null)
        {
            float regenDuration = 0;
            if (!castFromExtendedRadius) regenDuration = CastStreamDuration - CastStreamDuration / 3f;
            else regenDuration = CastStreamDuration;

            _healthTree.InitializeObject(treeData);
            if (treeData.MinEndurance) _healthTree.ServerStartFillHP(_healthTree.ObjectData.MaxHealth, regenDuration);

            if (treeMagicEvadeTalent) _healthTree.SetMagicEvade(100);

        }
        ResetShotCooldowns();

        _activeTrees.Add(tree);
        _currentTree.GrowTreeIncreasesMaxHealth = growTreeIncreasesMaxHealth;
        RpcClientAddTree(tree.GetComponent<NetworkIdentity>().netId, _currentTree);
    }

    [Command]
    private void CmdSpawnTreeAndTeleport(Vector3 position)
    {
        Debug.Log($"TargetPoint: {_targetPoint.x}");
        Vector3 spawnPosition = position + Vector3.down;

        var tree = Instantiate(_treePrefab, spawnPosition, Quaternion.identity);
        _currentTree = tree;
        NetworkServer.Spawn(_currentTree.gameObject, connectionToClient);
        SceneManager.MoveGameObjectToScene(_currentTree.gameObject, Hero.NetworkSettings.MyRoom);

        RpcTeleportToTree(_currentTree.gameObject);

        _healthTree = _currentTree.GetComponentInChildren<ObjectHealth>();
        if (_healthTree != null)
        {
            _healthTree.InitializeObject(treeData);

            float regenDuration = CastStreamDuration - CastStreamDuration / 3f;

            if (treeData.MinEndurance) _healthTree.ServerStartFillHP(_healthTree.ObjectData.MaxHealth, regenDuration);

            if (treeMagicEvadeTalent) _healthTree.SetMagicEvade(100);
        }
        ResetShotCooldowns();

        _activeTrees.Add(tree);
        _currentTree.GrowTreeIncreasesMaxHealth = growTreeIncreasesMaxHealth;
        RpcClientAddTree(tree.GetComponent<NetworkIdentity>().netId, _currentTree);
    }

    [Command]
    private void CmdRequestInterruptTree(uint treeNetId)
    {
        if (NetworkServer.spawned.TryGetValue(treeNetId, out NetworkIdentity networkIdentity))
        {
            var health = networkIdentity.GetComponentInChildren<ObjectHealth>();
            health.ServerInterruptFillHP();
        }
    }

    [Command]
    private void CmdCrossFade()
    {
        _hero.Animator.CrossFade("GrowTreeCastDelayExit", 0.1f);
    }

    [Command]
    private void CmdSetCastStreamDurationByProximity(Vector3 plannedPos, float checkRadius)
    {
        _activeTrees.RemoveAll(tree => tree == null);

        int nearCount = 0;
        foreach (var tree in _activeTrees) if (tree != null && Vector3.Distance(tree.transform.position, plannedPos) <= checkRadius) nearCount++;
        CastStreamDuration = nearCount == 0 ? baseCastStreamDuration : baseCastStreamDuration * Mathf.Pow(2, nearCount);
    }

    [ClientRpc]
    private void RpcClientAddTree(uint netId, GrowTreeAura currentTree)
    {
        _currentTree = currentTree;
        _currentTree.GrowTreeIncreasesMaxHealth = growTreeIncreasesMaxHealth;
        if (NetworkClient.spawned.TryGetValue(netId, out var networkIdentity)) _activeTrees.Add(networkIdentity.GetComponent<GrowTreeAura>());
    }

    [ClientRpc]
    private void RpcTeleportToTree(GameObject tree)
    {
        if (tree != null)
        {
            Vector3 topOfTree = tree.transform.position + Vector3.up * 5f;
            moveComponent.TeleportToPositionSmooth(topOfTree, _moveDuration);
        }
    }

    [ClientRpc]
    private void ResetShotCooldowns()
    {
        if (!treeShotCooldownTalent) return;

        if (shotsIntoSky != null && !shotsIntoSky.IsCooldowned) shotsIntoSky.ForceCooldownEnd();
        if (shotIntoSky != null && !shotIntoSky.IsCooldowned) shotsIntoSky.ForceCooldownEnd();
    }
    #endregion

    #region Talent
    public void ShotTreeCooldownTalent(bool value) => treeShotCooldownTalent = value;
    public void GrowTreeArrowIntoSkyRadius(bool value) => _isGrowTreeArrowIntoSkyRadiusTalent = value;
    #endregion

    #region Talent for doubling HP
    public void treeHealthTalentActive(bool value)
    {
        //treeHealthTalent = value;
        growTreeIncreasesMaxHealth = value;
    }

    private void TreeHealthTalentEnter()
    {
        //if (treeHealthTalent && _currentTree != null) _treeHealthCoroutine = StartCoroutine(IncreaseTreeMaxHealthOverTime());
    }

    private void TreeHealthTalentExit()
    {
        if (_treeHealthCoroutine != null)
        {
            StopCoroutine(_treeHealthCoroutine);
            _treeHealthCoroutine = null;
        }

        treeData.MaxHealth = baseHealth;
        CastStreamDuration = baseCastStreamDuration;

        CmdSetMaxHealth(treeData.MaxHealth);
    }

    private IEnumerator IncreaseTreeMaxHealthOverTime()
    {
        float increaseDuration = 60f;
        float interval = 0.3f;
        int steps = Mathf.FloorToInt(increaseDuration / interval);

        for (int i = 0; i < steps; i++)
        {
            treeData.MaxHealth += 1;
            CmdSetMaxHealth(treeData.MaxHealth);

            if (_healthTree != null) _healthTree.ObjectData.MaxHealth = treeData.MaxHealth;

            yield return new WaitForSeconds(interval);
        }
    }
    #endregion

    #region Talent for Magical abbilities evade

    public void treeMagicEvadeTalentActive(bool value)
    {
        treeMagicEvadeTalent = value;
    }

    #endregion

    protected override void ClearData() => _targetPoint = Vector3.positiveInfinity;
    public override void LoadTargetData(TargetInfo targetInfo) => _targetPoint = targetInfo.Points[0];
}