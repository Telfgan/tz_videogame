using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicalExcitementTalent : Talent
{
    [SerializeField] private float _increaseManaRegenerationPercentages = 1.1f;
    [SerializeField] private float _increaseManaRegenerationDuration = 3f;

    private WaitForSeconds _increaseManaRegenerationDeley;
    private Resource _mana;

    public override void Enter()
    {
        _increaseManaRegenerationDeley = new(_increaseManaRegenerationDuration);
        _mana = character.TryGetResource(ResourceType.Mana);

        character.DamageGeted += OnDamageTaked;
    }

    public override void Exit()
    {
        character.DamageGeted -= OnDamageTaked;
    }

    private void OnDamageTaked(Damage damage, GameObject target)
    {
        StartCoroutine(IncreaseManaRegeneration());
    }

    private IEnumerator IncreaseManaRegeneration()
    {
        _mana.IncreaseRegenerationPeriod(_increaseManaRegenerationPercentages);

        yield return _increaseManaRegenerationDeley;

        _mana.ReduceRegenerationPeriod(_increaseManaRegenerationPercentages);
    }
}
