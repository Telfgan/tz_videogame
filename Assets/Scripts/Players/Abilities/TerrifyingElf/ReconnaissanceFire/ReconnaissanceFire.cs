using Mirror;
using System.Collections;
using UnityEngine;
using HeathenEngineering.UnityPhysics;
using HeathenEngineering.UnityPhysics.API;
using Unity.Mathematics;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class ReconnaissanceFire : Skill
{
    [Header("Reconnaissance Fire Settings")]
    [SerializeField] private TrickShot trickShot;
    [SerializeField] private ReconnaissanceFireAura fireAura;
    [SerializeField] private GameObject emitterObject;
    [SerializeField] private ObjectData fireData;
    [SerializeField] private float duration = 10;
    [SerializeField] private float baseArea = 3f;

    [Header("TrickShot Settings")]
    [SerializeField] private List<Vector3> globalConstants = new(new Vector3[] { new(0, -9.81f, 0) });
    [SerializeField] private List<Vector3> localConstants = new();
    [SerializeField] private float speed;

    [Header("Raycast settings")]
    [SerializeField] private LayerMask groundLayer;

    private ReconnaissanceFireAura currentFireAura;
    private Vector3 _targetPoint = Vector3.positiveInfinity;
    private float _baseDuration;
    private float _baseAnimSpeed;
    private float _baseCastDelay;
    private Coroutine _auraLifeCoroutine;
    private Coroutine _boostWindow;
    private bool isSkillEnableBoostLogic;

    #region Talent

    private bool fireDarkTalent;
    private bool fireHealthTalent;
    private bool partialBlindnessTalent;
    private bool fireWorshipperTalent;
    private bool isSkillEnableBoostLogicActiveTalent;

    #endregion

    public ReconnaissanceFireAura CurrentFireAura => currentFireAura;
    public float BaseArea { get => baseArea; set => baseArea = value; }

    protected override bool IsCanCast => !float.IsPositiveInfinity(_targetPoint.x) && IsPointInRadius(Radius, _targetPoint);
    protected override int AnimTriggerCastDelay => Animator.StringToHash("ThrowCastDelay");
    protected override int AnimTriggerCast => 0;

    protected override void SkillEnableBoostLogic()
    {
        CastDeley = 0;
        isSkillEnableBoostLogic = true;
        Debug.Log("SkillEnableBoostLogic");
    }
    protected override void SkillDisableBoostLogic()
    {
        CastDeley = _baseCastDelay;
        isSkillEnableBoostLogic = false;
    }

    private void Start()
    {
        _baseAnimSpeed = Hero.Animator.speed;
        trickShot.speed = speed;
        _baseDuration = duration;
    }

    private void OnEnable()
    {
        ArrowFireProjectile.OnProjectilePathEnd += HandleProjectilePathEnd;
        OnSkillCanceled += HandleSkillCanceled;

        _baseCastDelay = CastDeley;
    }

    private void OnDisable()
    {
        ArrowFireProjectile.OnProjectilePathEnd -= HandleProjectilePathEnd;
        OnSkillCanceled -= HandleSkillCanceled;
    }

    public void AnimationFireMove()
    {
        if (_hero == null || _hero.Move == null) return;

        _hero.Move.StopMoveAndAnimationMove();
        _hero.Move.CanMove = false;

        Vector3 direction = _targetPoint - _hero.transform.position;
        bool badDirection = float.IsInfinity(_targetPoint.x) || direction.sqrMagnitude < 0.0001f;

        if (badDirection)
        {
            _hero.Move.StopLookAt();
            return;
        }

        _hero.Move.LookAtPosition(_targetPoint);
    }

    public void TryStartElvenBoostWindow()
    {
        if (!isSkillEnableBoostLogicActiveTalent) return;
        if (_boostWindow != null) return;
        if (UnityEngine.Random.value > 0.30f) return;

        _boostWindow = StartCoroutine(ElvenBoostWindow());
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        Hero.Animator.speed = Hero.Animator.speed/CastDeley;

        if (emitterObject) emitterObject.SetActive(true);
        ReconnaissanceFireHealthTalentEnter();

        while (float.IsPositiveInfinity(_targetPoint.x))
        {
            if (GetMouseButton)
            {
                Vector3 clickedPoint = GetMousePoint();

                if (IsPointInRadius(Radius, clickedPoint) && NoObstacles(clickedPoint, transform.position, _obstacle))
                {
                    _targetPoint = clickedPoint;
                    Hero.Move.LookAtPosition(_targetPoint);
                }
            }

            UpdateTrickShotTrajectory();
            yield return null;
        }

        if (emitterObject) emitterObject.SetActive(false);
    }

    protected override IEnumerator CastJob()
    {
        if (trickShot == null || fireAura == null) yield break;

        trickShot.Shoot();

        _targetPoint = Vector3.positiveInfinity;

        Hero.Animator.speed = _baseAnimSpeed;
        Hero.Move.StopLookAt();
        Hero.Animator.speed = _baseAnimSpeed;
        Hero.Move.CanMove = true;
    }

    private IEnumerator ElvenBoostWindow()
    {
        EnableSkillBoost();
        yield return new WaitForSeconds(2f);
        DisableSkillBoost();
        _boostWindow = null;
    }

    private void HandleProjectilePathEnd(Vector3 position) => CmdSpawnFireAura(position);

    void UpdateTrickShotTrajectory()
    {
         if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, 200f, groundLayer))
        {
            float s = Vector3.Distance(trickShot.transform.position, hit.point); trickShot.distance = s + 1f;

            if (Ballistics.Solution(trickShot.transform.position, trickShot.speed, hit.point, trickShot.constantAcceleration, out Quaternion low, out _) > 0) trickShot.transform.rotation = low;
        }
    }

    private void HandleSkillCanceled()
    {
        if (_hero != null && _hero.Move != null) ReconnaissanceFireHealthTalentExit();
        Hero.Animator.speed = _baseAnimSpeed;
        Hero.Move.CanMove = true;
        Hero.Move.StopLookAt();
        _targetPoint = Vector3.positiveInfinity;
    }

    [Command]
    private void CmdSetMaxHealth(float maxHealth)
    {
        fireData.MaxHealth = maxHealth;
    }

    [Command]
    private void CmdSpawnFireAura(Vector3 position)
    {
        if (float.IsInfinity(position.x) || float.IsNaN(position.x)) return;

        if (!isSkillEnableBoostLogic)
        {
            if (_auraLifeCoroutine != null) StopCoroutine(_auraLifeCoroutine);
            if (currentFireAura != null) NetworkServer.Destroy(currentFireAura.gameObject);
        }

        position.y += 0.1f;
        var aura = Instantiate(fireAura, position, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(aura.gameObject, Hero.NetworkSettings.MyRoom);
        NetworkServer.Spawn(aura.gameObject, connectionToClient);

        currentFireAura = aura;
        currentFireAura.FireDarkTalent = fireDarkTalent;
        RpcSetCurrentFireAura(aura);

        float life = _baseDuration + (fireWorshipperTalent ? 6f : 0f);
        _auraLifeCoroutine = StartCoroutine(DestroyAuraAfter(life, aura));
    }

    [Server]
    private IEnumerator DestroyAuraAfter(float seconds, ReconnaissanceFireAura aura)
    {
        yield return new WaitForSeconds(seconds);
        if (aura != null) NetworkServer.Destroy(aura.gameObject);
    }

    [ClientRpc]
    private void RpcSetCurrentFireAura(ReconnaissanceFireAura fireAura)
    {
        currentFireAura = fireAura;
        currentFireAura.FireDarkTalent = fireDarkTalent;

        if (fireWorshipperTalent) currentFireAura.ApplyFireWorshipperTalentEffect(true);
    }

    protected override void ClearData()
    {
        if (emitterObject != null) emitterObject.SetActive(false);
    }

    #region ReconnaissanceFireAuraDarknesTalent
    public void ReconnaissanceFireAuraDarknesActive(bool value)
    {
        fireDarkTalent = value;

        if (currentFireAura != null) currentFireAura.FireDarkTalent = fireDarkTalent;
    }
    #endregion

    #region ReconnaissanceFireHealthTalent
    public void ReconnaissanceFireHealthTalentActive(bool value)
    {
        fireHealthTalent = value;
    }

    private void ReconnaissanceFireHealthTalentEnter()
    {
        if (fireHealthTalent)
        {
            CmdSetMaxHealth(65);
            fireData.MaxHealth = 65;
        }
    }

    private void ReconnaissanceFireHealthTalentExit()
    {
        CmdSetMaxHealth(6);
        fireData.MaxHealth = 6;
    }
    #endregion

    #region partialBlindnessTalent
    public void partialBlindnessTalentActive(bool value)
    {
        partialBlindnessTalent = value;
        if (currentFireAura != null) currentFireAura.FireDarkTalent = partialBlindnessTalent;
    }
    #endregion

    #region FireWorshipperTalent
    public void FireWorshipperTalentActive(bool value)
    {
        fireWorshipperTalent = value;
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        _targetPoint = targetInfo.Points[0];
        _targetPoint = Vector3.positiveInfinity;
    }
    #endregion

    #region SkillEnableBoostLogicActiveTalent

    public void SkillEnableBoostLogicActiveTalent(bool value) => isSkillEnableBoostLogicActiveTalent = value;

    #endregion
}
