using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterFlowSkill : Skill
{
    [SerializeField] private float damageDecrease = 0.33f;
    [SerializeField] private bool stopAtZero = true;
    [SerializeField] private int minDamage = 1;
    [SerializeField] private GameObject particles;

    [Header("Конусные параметры")]
    [SerializeField] private float coneAngle = 60f; // Угол конуса в градусах
    [SerializeField] private float maxConeDistance = 10f; // Максимальная дальность конуса
    [SerializeField] private bool checkLineOfSight = true; // Проверять ли прямую видимость

    [Header("Отладка")]
    [SerializeField] private bool showDebugGizmos = true; // Показывать визуализацию конуса

    private enum HitOrder
    {
        ClosestFirst,
        FarthestFirst,
        Random
    }

    [SerializeField] private HitOrder hitOrder = HitOrder.ClosestFirst;

    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerCast => 0;

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        // Загрузка данных о цели (если нужно)
    }

    public void Start()
    {
        
    }

    protected override IEnumerator CastJob()
    {

        // Активируем частицы с автоотключением
        if (particles.TryGetComponent<ParticleSystem>(out var ps))
        {
            particles.SetActive(true);
            ps.Play();
        }
        else
        {
            particles.SetActive(true);
        }
        // Получаем цели в конусе
        List<IDamageable> targets = GetTargetsInCone();

        // Сортируем цели в соответствии с настройками
        targets = SortTargetsByOrder(targets, hitOrder);

        int baseDamage = Mathf.RoundToInt(Buff.Damage.GetBuffedValue(Damage));
        float currentMultiplier = 1f;
        int hitCount = 0;
        
        
        foreach (var target in targets)
        {
            currentMultiplier = Mathf.Max(0f, 1f - (damageDecrease * hitCount));

            if (stopAtZero && currentMultiplier <= 0f) break;

            int currentDamage = Mathf.RoundToInt(baseDamage * currentMultiplier);

            if (currentDamage < minDamage) break;

            Damage damage = new Damage
            {
                Value = currentDamage,
                Type = DamageType,
                PhysicAttackType = AttackRangeType,
            };

            CmdApplyDamage(damage, target.gameObject);
            hitCount++;

            Debug.Log($"Попадание #{hitCount}: урон = {currentDamage}, множитель = {currentMultiplier}, цель: {target.gameObject.name}");
        }



        yield return new WaitForSeconds(CastStreamDuration);

        // Плавно останавливаем частицы
        if (ps != null)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            yield return new WaitForSeconds(ps.main.startLifetime.constantMax);
        }

        particles.SetActive(false);

    }

    /// <summary>
    /// Получает все цели в пределах конуса перед персонажем
    /// </summary>
    private List<IDamageable> GetTargetsInCone()
    {
        List<IDamageable> targetsInCone = new List<IDamageable>();

        // Используем OverlapSphere для предварительного отбора целей
        var colliders = Physics.OverlapSphere(transform.position, maxConeDistance, _targetsLayers);

        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent<IDamageable>(out IDamageable target))
            {
                // Проверяем, находится ли цель в пределах конуса
                if (IsTargetInCone(collider.transform))
                {
                    // Дополнительно проверяем прямую видимость (если включено)
                    if (!checkLineOfSight || HasLineOfSightToTarget(collider.transform))
                    {
                        targetsInCone.Add(target);
                    }
                }
            }
        }

        Debug.Log($"Найдено целей в конусе: {targetsInCone.Count}");
        return targetsInCone;
    }

    /// <summary>
    /// Проверяет, находится ли цель в пределах конуса
    /// </summary>
    private bool IsTargetInCone(Transform targetTransform)
    {
        Vector3 directionToTarget = targetTransform.position - transform.position;

        // Проверяем расстояние
        float distanceToTarget = directionToTarget.magnitude;
        if (distanceToTarget > maxConeDistance)
            return false;

        // Проверяем угол
        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
        return angleToTarget <= coneAngle * 0.5f;
    }

    /// <summary>
    /// Проверяет прямую видимость до цели
    /// </summary>
    private bool HasLineOfSightToTarget(Transform targetTransform)
    {
        Vector3 startPosition = transform.position + Vector3.up * 1f; // Немного выше для реалистичности
        Vector3 targetPosition = targetTransform.position + Vector3.up * 1f;
        Vector3 direction = targetPosition - startPosition;
        float distance = direction.magnitude;

        // Игнорируем слои целей и самого кастера
        int layerMask = ~LayerMask.GetMask("IgnoreRaycast", "Player");

        if (Physics.Raycast(startPosition, direction.normalized, out RaycastHit hit, distance, layerMask))
        {
            // Если попали во что-то, что не является целью
            if (hit.collider.transform != targetTransform)
            {
                Debug.DrawLine(startPosition, hit.point, Color.red, 2f);
                return false;
            }
        }

        Debug.DrawLine(startPosition, targetPosition, Color.green, 2f);
        return true;
    }

    private List<IDamageable> SortTargetsByOrder(List<IDamageable> targets, HitOrder order)
    {
        switch (order)
        {
            case HitOrder.ClosestFirst:
                targets.Sort((a, b) =>
                    Vector3.Distance(transform.position, (a as MonoBehaviour).transform.position)
                    .CompareTo(Vector3.Distance(transform.position, (b as MonoBehaviour).transform.position)));
                break;

            case HitOrder.FarthestFirst:
                targets.Sort((a, b) =>
                    Vector3.Distance(transform.position, (b as MonoBehaviour).transform.position)
                    .CompareTo(Vector3.Distance(transform.position, (a as MonoBehaviour).transform.position)));
                break;

            case HitOrder.Random:
                System.Random rng = new System.Random();
                int n = targets.Count;
                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);
                    var temp = targets[k];
                    targets[k] = targets[n];
                    targets[n] = temp;
                }
                break;
        }

        return targets;
    }

    protected IEnumerator CastJobGeometric()
    {
        List<IDamageable> targets = GetTargetsInCone();
        targets = SortTargetsByOrder(targets, HitOrder.ClosestFirst);

        int baseDamage = Mathf.RoundToInt(Buff.Damage.GetBuffedValue(Damage));

        for (int i = 0; i < targets.Count; i++)
        {
            float damageMultiplier = Mathf.Pow(1f - damageDecrease, i);

            int currentDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);

            if (currentDamage < minDamage) break;

            Damage damage = new Damage
            {
                Value = currentDamage,
                Type = DamageType,
                PhysicAttackType = AttackRangeType,
            };

            CmdApplyDamage(damage, targets[i].gameObject);
            Debug.Log($"Цель #{i + 1}: урон = {currentDamage}, множитель = {damageMultiplier}");
        }

        yield return null;
    }

    /// <summary>
    /// Визуализация конуса в редакторе (для отладки)
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || !Application.isPlaying) return;

        Gizmos.color = new Color(0, 1, 1, 0.3f); // Голубой с прозрачностью

        // Рисуем сектор конуса
        DrawConeSector();

        // Рисуем линии к целям в конусе
        DrawTargetLines();
    }

    private void DrawConeSector()
    {
        Vector3 forward = transform.forward;
        Vector3 startDirection = Quaternion.AngleAxis(-coneAngle * 0.5f, Vector3.up) * forward;

        const int segments = 20;
        float angleStep = coneAngle / segments;

        // Рисуем границы конуса
        Gizmos.DrawRay(transform.position, startDirection * maxConeDistance);
        Gizmos.DrawRay(transform.position, Quaternion.AngleAxis(coneAngle, Vector3.up) * startDirection * maxConeDistance);

        // Рисуем дугу
        Vector3 previousPoint = transform.position + startDirection * maxConeDistance;
        for (int i = 1; i <= segments; i++)
        {
            float angle = -coneAngle * 0.5f + angleStep * i;
            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * forward;
            Vector3 point = transform.position + direction * maxConeDistance;

            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
    }

    private void DrawTargetLines()
    {
        var targets = GetTargetsInCone();
        foreach (var target in targets)
        {
            MonoBehaviour mb = target as MonoBehaviour;
            if (mb != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, mb.transform.position);
            }
        }
    }

    protected override void ClearData()
    {
        // Очистка данных (если нужно)
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> targetDataSavedCallback)
    {
        yield return null;
    }
}