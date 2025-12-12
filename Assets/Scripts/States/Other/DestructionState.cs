using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DestructionState : AbstractCharacterState
{
    private readonly List<StatusEffect> _effects = new() { StatusEffect.Destruction };
    public override States State => States.Destruction;
    public override StateType Type => StateType.Magic;
    public override BaffDebaff BaffDebaff => BaffDebaff.Debaff;
    public override List<StatusEffect> Effects => _effects;

    private float _tickInterval = 4f;
    private float _damagePerTick = 6f;
    private float _effectivenessIncreasePerTick = 0.1f;

    private float _timer;
    private float _accumulatedEffectiveness = 1f;
    private float _totalDamageInInterval = 0f;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _personWhoMadeBuff = personWhoMadeBuff;
        duration = durationToExit;

        _health = character.Character.Health;
        _accumulatedEffectiveness = 1f;
        _totalDamageInInterval = 0f;

        _timer = _tickInterval;

		float damageValue = _damagePerTick * _accumulatedEffectiveness;

		CmdDamage(damageValue);
	}

    public override void UpdateState()
    {
        if (_health == null) return;

        duration -= Time.deltaTime;
        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            float damageValue = _damagePerTick * _accumulatedEffectiveness;

            CmdDamage(damageValue);

            _accumulatedEffectiveness += _totalDamageInInterval * _effectivenessIncreasePerTick;
            _totalDamageInInterval = damageValue;

            _timer = _tickInterval;
        }

        if (duration <= 0)
        {
            ExitState();
            return;
        }
    }

    public override void ExitState()
    {
        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        if (_personWhoMadeBuff.TryGetComponent<StunMagicPassiveSkill>(out StunMagicPassiveSkill stunMagicPassiveSkill) && stunMagicPassiveSkill.IsFillingDestruction) duration = time + 3f;
        else duration = time;
        return false;
    }

    [Server] private void CmdDamage(float damageValue) => ClientRpcDamage(damageValue);

    [ClientRpc]
    private void ClientRpcDamage(float damageValue)
    {
        Damage damage = new()
        {
            Value = damageValue,
            Type = DamageType.Magical,
        };

        _health.TryTakeDamage(ref damage, null);
    }
}
