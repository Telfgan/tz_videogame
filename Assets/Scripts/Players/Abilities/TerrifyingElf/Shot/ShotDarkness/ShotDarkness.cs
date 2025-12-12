using Mirror;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShotDarkness : Skill
{
    [SerializeField] private ArrowProjectile projectile;
    [SerializeField] private HeroComponent playerLinks;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Ghost ghostSkill;
    [SerializeField] private MultiMagicSpell multiMagicSpell;
    [SerializeField] private TerrifyingElfAura terrifyingElfAura;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private float minDamage;
    [SerializeField] private float maxDamage;

    private const string _startAnimTrigger = "ShotDarkCastDelayTrigger";
    private const string _endAnimTrigger = "ShotCastDelayEndAnimTrigger"; // убрать в дальнейшем две анимации, остаток от автоатаки

    private Vector3 _targetPoint = Vector3.positiveInfinity;
    private AudioSource _audioSource;
    private int _consecutiveShots;
    private float _magicDamage;
    private Character _lastTarget;
    private bool _isHealthAboveThreshold;

    protected override int AnimTriggerCastDelay => Animator.StringToHash(_startAnimTrigger);
    protected override int AnimTriggerCast => 0;
    protected override bool IsCanCast =>
        Vector3.Distance(_targetPoint, transform.position) <= Radius &&
        NoObstacles(_targetPoint, transform.position, _obstacle);

    private void OnDestroy() => OnSkillCanceled -= HandleSkillCanceled;
    private void OnEnable() => OnSkillCanceled += HandleSkillCanceled;
    private void Start() => _audioSource = GetComponent<AudioSource>();

    public void ShotDarknessAnimationMove()
    {
        if (_hero == null || _hero.Move == null) return;

        _isHealthAboveThreshold = false;
        if (_lastTarget != null)
        {
            var health = _lastTarget.Health;
            _isHealthAboveThreshold = health.CurrentValue >= health.MaxValue * 0.8f;
        }

        _hero.Move.StopMoveAndAnimationMove();
        _hero.Move.CanMove = false;

        Vector3 direction = _targetPoint - _hero.transform.position;
        bool badDirection = float.IsInfinity(_targetPoint.x) || direction.sqrMagnitude < 0.0001f;

        if (badDirection) _hero.Move.StopLookAt();

        else Hero.Move.LookAtPosition(_targetPoint);

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

        float manaDamage = playerLinks.Resources.Where(resource => resource.Type == ResourceType.Mana).Sum(resource => resource.CurrentValue);
        _magicDamage = Mathf.Min(6f, Mathf.Floor(manaDamage));

        if (_magicDamage > 0) SpendBonusMana(_magicDamage);
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        Hero.Animator.speed = Hero.Animator.speed / CastDeley;
        var multiMagic = Hero.CharacterState.GetState(States.MultiMagic) as MultiMagic;

        while (float.IsPositiveInfinity(_targetPoint.x))
        {
            if (GetMouseButton)
            {
                Vector3 clickedPoint = GetMousePoint();

                if (NoObstacles(clickedPoint, transform.position, _obstacle) && TryGetDamageableAtPoint(clickedPoint, out var damageable))
                {
                    if (_lastTarget == null) _lastTarget = (damageable as Component)?.GetComponent<Character>();
                    if (multiMagic != null) multiMagic.LastTarget = _lastTarget;
                    _targetPoint = clickedPoint;
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
        if (!IsCanCast)
        {
            Hero.Move.CanMove = true;
            Hero.Move.StopLookAt();
            ClearData();
            yield break;
        }

        CmdCreateProjectileAtPosition(_targetPoint, Damage);

        var multiMagic = Hero.CharacterState.GetState(States.MultiMagic) as MultiMagic;

        if (multiMagic != null)
        {
            foreach (var character in multiMagic.PopPendingTargets())
            {
                TryPayCost();
                CmdUseMana(_magicDamage);
                CmdCreateProjectileAtPosition(character.transform.position, Damage);
            }

            float reduce = multiMagicSpell.RemainingCooldownTime * 0.1f;
            multiMagicSpell.DecreaseSetCooldown(reduce);
        }

        ProcessGhostCooldownReduction();
        WorkAnimator(_startAnimTrigger, _endAnimTrigger);
        HandleSkillCanceled();
        ClearData();
    }

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
            _lastTarget = null;
            Hero.Animator.speed = 1;
            Hero.Move.StopLookAt();
        }

        AfterCastJob();
    }

    private bool TryGetDamageableAtPoint(Vector3 point, out IDamageable damageable)
    {
        damageable = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _targetsLayers))
        {
            if (hit.collider.TryGetComponent(out damageable))
            {
                return true;
            }
        }

        return false;
    }

    private void WorkAnimator(string oldAnim, string newAnim)
    {
        _hero.Animator.ResetTrigger(Animator.StringToHash(oldAnim));
        _hero.NetworkAnimator.ResetTrigger(Animator.StringToHash(oldAnim));
        _hero.Animator.CrossFade(newAnim, 0.1f);
        CmdCrossFade(newAnim);
    }

    private void UseMana(float amount)
    {
        float mana = amount;
        foreach (var resource in playerLinks.Resources.Where(resource => resource.Type == ResourceType.Mana))
        {
            if (mana <= 0) break;
            float spend = Math.Min(resource.CurrentValue, mana);
            resource.CurrentValue -= spend;
            mana -= spend;
        }
    }

    [Command]
    protected void CmdCreateProjectileAtPosition(Vector3 position, float damage)
    {
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
        Vector3 direction = (position - spawnPosition).normalized;

        if (direction == Vector3.zero) return;

        ArrowProjectile proj = Instantiate(projectile, spawnPosition, Quaternion.LookRotation(direction));
        proj.Init(playerLinks, _magicDamage, false, this, damage);
        SceneManager.MoveGameObjectToScene(proj.gameObject, _hero.NetworkSettings.MyRoom);
        NetworkServer.Spawn(proj.gameObject);
        proj.StartFly(direction);
        RpcInit(proj.gameObject, _magicDamage, damage);
        RpcPlayShotSound();
    }

    [Command] private void CmdCrossFade(string newAnim) => _hero.Animator.CrossFade(newAnim, 0.1f);
    [Command] private void CmdUseMana(float amount) => UseMana(amount);
    [Command] private void CmdSpendBonusMana(float amount) => SpendBonusMana(amount);

    private void SpendBonusMana(float amount)
    {
        float mana = amount;
        foreach (var resource in playerLinks.Resources.Where(resource => resource.Type == ResourceType.Mana))
        {
            if (mana <= 0) break;
            float spend = Math.Min(resource.CurrentValue, mana);
            resource.CurrentValue -= spend;
            mana -= spend;
        }

        _magicDamage = amount - mana;
    }

    [ClientRpc]
    protected void RpcInit(GameObject gameObject, float magicDamage, float damage)
    {
        if (gameObject == null) return;

        ArrowProjectile proj = gameObject.GetComponent<ArrowProjectile>();
        if (proj != null) proj.Init(playerLinks, magicDamage, false, this, damage);
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
        _consecutiveShots = 0;
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        _targetPoint = targetInfo.Points[0];
    }
}
