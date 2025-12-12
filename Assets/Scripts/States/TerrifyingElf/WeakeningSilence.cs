using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeakeningSilence : AbstractCharacterState
{
    private float _damagePerTick;
    private float _currentDamage;
    private float _tickInterval = 1f;
    private float _duration;

    private bool damageTick;

    public override States State => States.WeakeningSilence;
    public override StateType Type => StateType.Magic;
    public override BaffDebaff BaffDebaff => BaffDebaff.Debaff;
    public override List<StatusEffect> Effects => new List<StatusEffect> { StatusEffect.Poison };

    public WeakeningSilence() => MaxStacksCount = 6;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _health = character.Character.Health;
        _damagePerTick = damageToExit;
        damageTick = true;
        _currentDamage = _damagePerTick;
        _duration = durationToExit;

        if (_health == null)
        {
            Debug.LogWarning($"Health component is missing on {character.name}. WeakeningSilence will not deal damage.");
            return;
        }

        _characterState.StartCoroutine(PeriodicDamageRoutine());
    }

    public override void ExitState()
    {
        _characterState.RemoveState(this);
        damageTick = false;
        _characterState.StopCoroutine(PeriodicDamageRoutine());
    }

    public override void UpdateState()
    {
        _duration -= Time.deltaTime;
        if (_duration <= 0) ExitState();
    }

    public override bool Stack(float addDuration)
    {
        if (CurrentStacksCount >= MaxStacksCount) return false;

        CurrentStacksCount++;
        _currentDamage += _damagePerTick;
        duration = Mathf.Max(duration, addDuration);

        return true;
    }

    private IEnumerator PeriodicDamageRoutine()
    {
        while (damageTick)
        {
            yield return new WaitForSeconds(_tickInterval);
            ApplyDamage();
        }
    }

    [Server]
    private void ApplyDamage()
    {
        if (_health != null)
        {
            Damage damage = new Damage
            {
                Value = _currentDamage,
                Type = DamageType.Magical
            };
            if (_health != null)
            {
                _health.TryTakeDamage(ref damage, null);
            }
        }
        else
        {
            Debug.LogError("Health is null in CmdApplyDamage.");
        }
    }
}
