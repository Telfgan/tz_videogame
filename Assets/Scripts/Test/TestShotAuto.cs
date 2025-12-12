using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestShootAuto : Skill
{
    [SerializeField] private Character shoter;

    private Coroutine _damageCoroutine;
    private readonly List<Character> _targetsInRange = new();

    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerCast => 0;
    protected override bool IsCanCast => false;

    private void OnEnable()
    {
       _damageCoroutine = StartCoroutine(DamageTickRoutine());
    }

    private void OnDisable()
    {
        if (_damageCoroutine != null)
            StopCoroutine(_damageCoroutine);
    }

    private IEnumerator DamageTickRoutine()
    {
        var wait = new WaitForSeconds(CastDeley);
        while (true)
        {
            UpdateTargets();
            foreach (var target in _targetsInRange)
            {
                if (target != null)
                {
                    Damage damage = new Damage
                    {
                        Value = Damage,
                        Type = DamageType.Physical
                    };

                    CmdApplyDamage(damage, target.gameObject);
                    //target.CharacterState.CmdAddState(States.Frozen, 5, 0, shoter.gameObject, name);
                }
            }
            yield return wait;
        }
    }

    private void UpdateTargets()
    {
        _targetsInRange.Clear();
        Collider[] colliders = Physics.OverlapSphere(transform.position, Radius, _targetsLayers);
        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent<Character>(out var character) && character != null && character != shoter)
            {
                _targetsInRange.Add(character);
            }
        }
    }

    public override void LoadTargetData(TargetInfo targetInfo) { }
    protected override IEnumerator CastJob() { yield break; }
    protected override void ClearData() { }
    protected override IEnumerator PrepareJob(System.Action<TargetInfo> targetDataSavedCallback) { yield break; }
}
