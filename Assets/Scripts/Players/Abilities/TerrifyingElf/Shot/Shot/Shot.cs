using Mirror;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Shot : Skill
{
    [SerializeField] private ArrowProjectile projectile;
    [SerializeField] private HeroComponent playerLinks;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Ghost ghostSkill;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private TerrifyingElfAura terrifyingElfAura;
    [SerializeField] private float minDamage;
    [SerializeField] private float maxDamage;

    private const string _startAnimTrigger = "ShotCastDelayTrigger";

    private AudioSource _audioSource;
    private int _consecutiveShots;

    private IDamageable _target;
    private Vector3 _targetPoint = Vector3.positiveInfinity;

    private bool _isHealthAboveThreshold;

    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerCast => Animator.StringToHash(_startAnimTrigger);
    protected override bool IsCanCast { get => CheckCanCast(); }

    private bool CheckCanCast()
    {
        if (_target == null) return Vector3.Distance(_targetPoint, transform.position) <= Radius;
        return Vector3.Distance(_targetPoint, transform.position) <= Radius || Vector3.Distance(_target.transform.position, transform.position) <= Radius;
    }

    private void OnDestroy()
    {
        OnSkillCanceled -= HandleSkillCanceled;
    }

    private void OnEnable()
    {
        OnSkillCanceled += HandleSkillCanceled;
    }

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void ShotAnimationMove()
    {
        if (_hero == null || _hero.Move == null) return;

        _isHealthAboveThreshold = false;

        if (_target != null && _target is Character targetCurrent)
        {
            var health = targetCurrent.Health;
            _isHealthAboveThreshold = health.CurrentValue >= health.MaxValue * 0.8f;
        }

        _hero.Move.StopMoveAndAnimationMove();
        _hero.Move.CanMove = false;

        if (_target != null) Hero.Move.LookAtTransform(_target.transform);
        else if (_targetPoint != Vector3.positiveInfinity) Hero.Move.LookAtPosition(_targetPoint);


        if (!terrifyingElfAura) Damage = UnityEngine.Random.Range(minDamage, maxDamage + 1);
        else
        {
            if (!_isHealthAboveThreshold) Damage = UnityEngine.Random.Range(minDamage, maxDamage + 1);

            else
            {
                var elvenSkill = playerLinks.CharacterState.GetState(States.ElvenSkill) as ElvenSkill;

                if (elvenSkill == null) Damage = UnityEngine.Random.Range(minDamage, maxDamage + 1);

                else
                {
                    float baseDamage = UnityEngine.Random.Range(minDamage, maxDamage + 1);
                    float extraDamage = UnityEngine.Random.Range(minDamage, maxDamage + 1) * 0.3f;
                    float total = baseDamage + extraDamage;

                    bool isCrit = UnityEngine.Random.value < 0.20f;
                    if (isCrit) total *= 3.2f;

                    Damage = total;
                }
            }
        }
    }

    public void ShotCastStart()
    {
        AnimStartCastCoroutine();
    }

    public void ShotCastEnd()
    {
        AnimCastEnded();
    }

    public void ShotPreparation()
    {
        _hero.Move.CanMove = false;
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        if (targetInfo.GetTargets().Count > 0) _target = targetInfo.GetTargets()[0] as IDamageable;
        _targetPoint = targetInfo.Points[0];
    }
    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        Hero.Animator.speed = Hero.Animator.speed / 1;

        ITargetable target = null;
        Vector3 targetPoint = Vector3.positiveInfinity;

        while (float.IsPositiveInfinity(targetPoint.x) && target == null)
        {
            if (GetMouseButton)
            {
                if (GetTarget() is ITargetable targetable) target = targetable;
                targetPoint = GetMousePoint();
            }
            yield return null;
        }
        TargetInfo targetInfo = new TargetInfo();
        targetInfo.AddTarget(target);
        targetInfo.Points.Add(targetPoint);
        callbackDataSaved(targetInfo);
    }

    protected override IEnumerator CastJob()
    {
        if (_target == null && _targetPoint == Vector3.positiveInfinity) yield return null;
        if (_target != null && !IsTargetInRange()) yield return null;


        ShotAnimationMove();
        ProcessGhostCooldownReduction();

        if (_target != null) CmdCreateProjectileAtTarget(_target.transform, Damage);
        else CmdCreateProjectileAtPosition(_targetPoint, Damage);

        yield return null;
    }
    private bool IsTargetInRange() { return _target != null && Vector3.Distance(transform.position, _target.transform.position) <= Radius; }
    private void ProcessGhostCooldownReduction()
    {
        if (!ghostSkill || !ghostSkill.CooldownGhostShotActive) return;

        _consecutiveShots++;
        if (_consecutiveShots >= 3)
        {
            ghostSkill.ReductionCooldownCharges(1);
            _consecutiveShots = 0;
        }
    }

    private void HandleSkillCanceled()
    {
        if (_hero?.Move != null)
        {
            Hero.Move.CanMove = true;
            Hero.Animator.speed = 1;
            _target = null;
            _targetPoint = Vector3.positiveInfinity;
            Hero.Move.StopLookAt();
        }
    }

    [Command]
    public void CmdCreateProjectileAtTarget(Transform target, float damage)
    {
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
        Vector3 direction = (target.transform.position - spawnPosition).normalized;

        if (direction == Vector3.zero) return;

        ArrowProjectile proj = Instantiate(projectile, spawnPosition, Quaternion.LookRotation(direction));
        proj.Init(playerLinks, 0, false, this, damage);
        SceneManager.MoveGameObjectToScene(proj.gameObject, _hero.NetworkSettings.MyRoom);
        NetworkServer.Spawn(proj.gameObject);
        proj.StartFly(target);
        RpcInit(proj.gameObject, damage);
        RpcPlayShotSound();
    }

    [Command]
    public void CmdCreateProjectileAtPosition(Vector3 position, float damage)
    {
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
        Vector3 direction = (position - spawnPosition).normalized;

        if (direction == Vector3.zero) return;

        ArrowProjectile proj = Instantiate(projectile, spawnPosition, Quaternion.LookRotation(direction));
        proj.Init(playerLinks, 0, false, this, damage);
        SceneManager.MoveGameObjectToScene(proj.gameObject, _hero.NetworkSettings.MyRoom);
        NetworkServer.Spawn(proj.gameObject);
        proj.StartFly(direction);
        RpcInit(proj.gameObject, damage);
        RpcPlayShotSound();
    }

    [ClientRpc]
    protected void RpcInit(GameObject gameObject, float damage)
    {
        if (gameObject == null) return;

        ArrowProjectile proj = gameObject.GetComponent<ArrowProjectile>();
        if (proj != null) proj.Init(playerLinks, 0, false, this, damage);
    }

    [ClientRpc]
    private void RpcPlayShotSound()
    {
        if (_audioSource != null && audioClip != null)
            _audioSource.PlayOneShot(audioClip);
    }

    protected override void ClearData()
    {
        _targetPoint = Vector3.positiveInfinity;
        _target = null;
        _consecutiveShots = 0;
    }
}