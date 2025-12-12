using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;

public class SpellMoveTo : Skill
{
    [SerializeField] private float _moveDurationPerUnit = 0.2f;
    [SerializeField] private float _damageDelay = 0.5f;
    [SerializeField] private float _attackDistance = 3f;
    [SerializeField] private float _damage = 5f;
    [SerializeField] private Animator _animator;

    private Queue<Vector3> _movementQueue = new();
    private Coroutine _attackCoroutine;
    private Character _currentEnemyTarget;
    private float _lastAttackTime;
    private Tween _activeTween;

    public Action<GameObject> DoMove;

    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerCast => 0;
    protected override bool IsCanCast => !IsCasting;
    private void OnDisable()
    {
        if (_activeTween != null && _activeTween.IsActive())
        {
            _activeTween.Kill();
            _activeTween = null;
        }

        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
        }

        Hero.Move.CanMove = true;
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        if (targetInfo.Points != null && targetInfo.Points.Count > 0)
        {
            foreach (var point in targetInfo.Points)
            {
                _movementQueue.Enqueue(point);
            }
        }
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        while (!GetMouseButton)
        {
            yield return null;
        }

        Vector3 clickedPoint = GetMousePoint();

        TargetInfo targetInfo = new TargetInfo();
        targetInfo.Points.Add(clickedPoint);
        callbackDataSaved(targetInfo);
    }
    protected override IEnumerator CastJob()
    {
        IsCasting = true;

        while (_movementQueue.Count > 0)
        {
            Vector3 point = _movementQueue.Dequeue();

            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }

            yield return MoveToPointWithNavMeshPath(point, false);

            _attackCoroutine = StartCoroutine(AttackNearbyEnemiesJob());
        }

        IsCasting = false;
    }

    private IEnumerator MoveToPointWithNavMeshPath(Vector3 targetPoint, bool stopAtObstacle)
    {
        Hero.Move.CanMove = false;

        NavMeshPath path = new NavMeshPath();
        bool hasPath = NavMesh.CalculatePath(transform.position, targetPoint, NavMesh.AllAreas, path);

        if (!hasPath || path.status != NavMeshPathStatus.PathComplete)
        {
            Hero.Move.CanMove = true;
            yield break;
        }

        Vector3 lastDoMovePoint = transform.position;

        for (int i = 1; i < path.corners.Length; i++)
        {
            Vector3 segmentTarget = path.corners[i];
            float distance = Vector3.Distance(transform.position, segmentTarget);
            float duration = distance * _moveDurationPerUnit;

            Quaternion lookRotation = Quaternion.LookRotation((segmentTarget - transform.position).normalized);
            transform.rotation = lookRotation;

            bool interruptedByObstacle = false;

            if (_activeTween != null && _activeTween.IsActive())
            {
                _activeTween.Kill();
                _activeTween = null;
            }

            _activeTween = transform.DOMove(segmentTarget, duration)
                .SetEase(Ease.Linear)
                .OnUpdate(() =>
                {
                    if (this == null || !gameObject.activeInHierarchy) return;

                    float movedDist = Vector3.Distance(lastDoMovePoint, transform.position);
                    if (movedDist >= 1f)
                    {
                        DoMove?.Invoke(gameObject);
                        lastDoMovePoint = transform.position;
                    }

                    if (stopAtObstacle && Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 1f, LayerMask.GetMask("Obstacle"))) interruptedByObstacle = true;
                    if (interruptedByObstacle && _activeTween != null && _activeTween.IsActive()) _activeTween.Kill();
                });

            yield return _activeTween.WaitForCompletion();

            if (interruptedByObstacle) break;
        }

        Hero.Move.CanMove = true;
    }

    private IEnumerator AttackNearbyEnemiesJob()
    {
        while (true)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, Radius, LayerMask.GetMask("Enemy"));

            Character nearest = null;
            float minDist = float.MaxValue;

            foreach (var hit in hits)
            {
                Character enemy = hit.GetComponent<Character>();
                if (enemy != null && !enemy.IsDead)
                {
                    float dist = Vector3.Distance(transform.position, enemy.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = enemy;
                    }
                }
            }

            if (nearest == null)
            {
                yield break;
            }

            while (nearest != null && !nearest.IsDead && Vector3.Distance(transform.position, nearest.transform.position) <= Radius)
            {
                float distance = Vector3.Distance(transform.position, nearest.transform.position);

                Vector3 dir = (nearest.transform.position - transform.position).normalized;
                if (dir != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(dir);

                if (distance > _attackDistance)
                {
                    Vector3 safeTarget = GetApproachPointNearEnemy(nearest);
                    yield return MoveToPointWithNavMeshPath(safeTarget, true);
                    continue;
                }

                if (Time.time - _lastAttackTime > _damageDelay)
                {
                    _currentEnemyTarget = nearest;
                    _animator.SetTrigger("AutoAttackScared");
                    _lastAttackTime = Time.time;
                    yield return new WaitForSeconds(_damageDelay);
                }

                yield return null;
            }

            yield return null;
        }
    }

    private Vector3 GetApproachPointNearEnemy(Character enemy)
    {
        Vector3 toEnemy = (enemy.transform.position - transform.position).normalized;
        float stopDistance = _attackDistance - 0.1f;
        return enemy.transform.position - toEnemy * stopDistance;
    }

    private void DealDamage()
    {
        if (_currentEnemyTarget == null) return;

        Damage damage = new Damage
        {
            Value = Buff.Damage.GetBuffedValue(_damage),
            Type = DamageType,
            PhysicAttackType = AttackRangeType
        };

        CmdApplyDamage(damage, _currentEnemyTarget.gameObject);
    }

    public void OnAutoAttackAnimationHit()
    {
        if (_currentEnemyTarget == null) return;
        DealDamage();
    }

    public void OnAutoAttackAnimationEnd()
    {
        _currentEnemyTarget = null;
    }

    protected override void ClearData()
    {
        _movementQueue.Clear();
        _currentEnemyTarget = null;
        IsCasting = false;
    }
}