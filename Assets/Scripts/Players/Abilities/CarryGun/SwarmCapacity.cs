using System;
using System.Collections;
using UnityEngine;
using Mirror;
using System.Linq;

public class SwarmCapacity : Skill, IPassiveSkill, ICounterSkill
{
    #region Skill
    protected override int AnimTriggerCastDelay => throw new NotImplementedException();
    protected override int AnimTriggerCast => throw new NotImplementedException();
    public override void LoadTargetData(TargetInfo targetInfo) => throw new NotImplementedException();
    protected override IEnumerator CastJob() { yield return null; }
    protected override void ClearData() => throw new NotImplementedException();
    protected override IEnumerator PrepareJob(Action<TargetInfo> targetDataSavedCallback) => throw new NotImplementedException();
    #endregion

    private SpawnComponent _spawnComponent;
    private Coroutine _overloadCheckRoutine;

    private void Start()
    {
        _spawnComponent = Hero.GetComponent<SpawnComponent>();

        if (_spawnComponent != null)
        {
            _spawnComponent.UnitAdded += UpdateCounter;
            _spawnComponent.UnitRemoved += UpdateCounter;
            UpdateCounter();
        }
    }

    private void OnDestroy()
    {
        if (_spawnComponent != null)
        {
            _spawnComponent.UnitAdded -= UpdateCounter;
            _spawnComponent.UnitRemoved -= UpdateCounter;
        }

        if (_overloadCheckRoutine != null)
        {
            StopCoroutine(_overloadCheckRoutine);
            _overloadCheckRoutine = null;
        }
    }

    private void UpdateCounter(Character _) => UpdateCounter();

    private void UpdateCounter()
    {
        if (_spawnComponent == null) return;

        CurrentCounter = _spawnComponent.Units.Count(unit => unit != null && !unit.TryGetComponent<MucusAutoGrowth>(out _));

        if (_overloadCheckRoutine == null)
            _overloadCheckRoutine = StartCoroutine(CheckOverloadRoutine());
    }

    private IEnumerator CheckOverloadRoutine()
    {
        WaitForSeconds delay = new WaitForSeconds(1f);

        while (true)
        {
            int realCount = _spawnComponent.Units.Count(unit => unit != null && !unit.TryGetComponent<MucusAutoGrowth>(out _));

            if (realCount > MaxCounter)
            {
                float overloadCount = realCount - MaxCounter;
                float percentDamage = overloadCount * 0.05f;

                foreach (var minion in _spawnComponent.Units)
                {
                    if (minion == null || minion.IsDead) continue;
                    if (minion.TryGetComponent<MucusAutoGrowth>(out _)) continue;

                    float damageValue = minion.Health.MaxValue * percentDamage;

                    Damage damage = new Damage
                    {
                        Value = damageValue,
                        Type = DamageType.None,
                        PhysicAttackType = AttackRangeType.MeleeAttack,
                    };

                    CmdApplyDamage(damage, minion.gameObject);
                }
            }

            yield return delay;
        }
    }


    [Command]
    private void CmdApplyDamage(Damage damage, GameObject target)
    {
        if (target == null) return;

        var hp = target.GetComponent<Health>();
        if (hp == null) return;

        hp.TryTakeDamage(ref damage, this);
        Hero.DamageTracker.AddDamage(damage, target, isServerRequest: true);
    }
}
