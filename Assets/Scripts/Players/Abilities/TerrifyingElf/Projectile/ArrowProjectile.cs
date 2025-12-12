using Mirror;
using System.Collections;
using System.Linq;
using UnityEngine;

public class ArrowProjectile : Projectiles
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _lifeTime = 5f;
    [SerializeField] private bool _arrowDark;
    [SerializeField] private float duration;
    [SerializeField] private DamageType damageTypePhysics;

    private Transform _followTarget;
    private bool _isFollowingTarget = false;

    private float _magDamage;
    private float _damage;

    public bool ArrowDark { get => _arrowDark; set => _arrowDark = value; }

    public void StartFly(Vector3 direction)
    {
        if (_rb != null) _rb.linearVelocity = direction * _speed;

        Destroy(gameObject, _lifeTime);
    }
    public void StartFly(Transform target)
    {
        _followTarget = target;
        _isFollowingTarget = true;
        StartCoroutine(FollowTargetCoroutine());
        Destroy(gameObject, _lifeTime);
    }

    public void Init(HeroComponent dad, float energy, bool lastHit, Skill skill, float damage)
    {
        base.Init(dad, energy, lastHit, skill);
        _damage = damage;
        _magDamage = energy;
    }

    [Server]
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == _dad?.gameObject) return;
        if (!other.TryGetComponent<IDamageable>(out _)) return;

        if (_arrowDark && other.GetComponentInParent<ReconnaissanceFireAura>() != null)
        {
            Destroy(gameObject);
            return;
        }

        if (((1 << other.gameObject.layer) & _skill.TargetsLayers.value) == 0) return;

        if (other.TryGetComponent<ObjectHealth>(out ObjectHealth objectHealth) &&
            objectHealth.ResistMagicDamage >= 100 && _arrowDark)
            return;

        ApplyEnemy(other);
        Destroy(gameObject);
    }


    //private void TargetApply(Collider other)
    //{
    //    //if (other.TryGetComponent<IDamageable>(out IDamageable target))
    //    //{
    //    //    //if (other.TryGetComponent<UserNetworkSettings>(out UserNetworkSettings userNetworkSettings))
    //    //    //{
    //    //    //    if (userNetworkSettings.TeamIndex != _dad.NetworkSettings.TeamIndex)
    //    //    //    {
    //    //    //        ApplyEnemy(other, target);
    //    //    //    }
    //    //    //}

    //    //    if (other.gameObject != _dad.gameObject && ((1 << other.gameObject.layer) & _skill.TargetsLayers.value) != 0) ApplyEnemy(other);
    //    //}
    //}

    #region ApplyEnemy
    private void ApplyEnemy(Collider collider)
    {
        bool inAstral = _dad != null && _dad.CharacterState.CheckForState(States.Astral);

        if (_arrowDark)
        {
            if (!inAstral)
            {
                ApplyDamage(_damage, damageTypePhysics, collider.gameObject);
                if (TryApplyDamage(damageTypePhysics, _skill.AttackRangeType, collider.gameObject)) return;
            }

            float totalMagDamage = _magDamage;
            if (inAstral) totalMagDamage *= 1.5f;

            ApplyDamage(totalMagDamage, _skill.DamageType, collider.gameObject);

            if (collider.TryGetComponent<Character>(out Character character)) character.CharacterState.AddState(States.InnerDarkness, duration, 0, _skill.Hero.gameObject, _skill.name);
        }

        else ApplyDamage(_damage, damageTypePhysics, collider.gameObject);
    }
    #endregion

    private void ApplyDamage(float value, DamageType type, GameObject target)
    {
        var damage = new Damage { Value = value, Type = type };
        _skill.ApplyDamage(damage, target);
    }

    private bool TryApplyDamage(DamageType damageType, AttackRangeType attackRangeType, GameObject target)
    {
        if (target.TryGetComponent<Health>(out Health health)) return health.TryEvade(damageType, attackRangeType);

        return false;
    }

    private IEnumerator FollowTargetCoroutine()
    {
        while (_isFollowingTarget && _followTarget != null)
        {
            Vector3 dir = (_followTarget.position - transform.position).normalized;
            if (_rb != null)
                _rb.linearVelocity = dir * _speed;

            yield return null;
        }
    }
}