using System.Collections.Generic;
using UnityEngine;

public class HealingSlime : AbstractCharacterState
{
    public override States State => States.HealingSlime;
    public override StateType Type => StateType.Magic;
    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override List<StatusEffect> Effects => _effects;

    private readonly List<StatusEffect> _effects = new() { StatusEffect.Healing };

    private const float PercentPerStack = 0.01f;

    private float _timer;
    private float _remaining;
    private bool _infinite;

    public override float RemainingDuration => _infinite ? 999f : _remaining;

    public HealingSlime()
    {
        MaxStacksCount = 9;
    }

    public void SwitchToFinite()
    {
        _timer = 0f;
        _infinite = false;
        _remaining = Mathf.Clamp(CurrentStacksCount, 1, 999f);
    }

    public void SwitchToInfinite()
    {
        _infinite = true;
        _timer = 0f;
        duration = 999f;
    }

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character caster, string skillName)
    {
        _characterState = character;
        _personWhoMadeBuff = caster;
        _health = character.Character.Health;

        CurrentStacksCount = 0;
        SwitchToInfinite();
        Stack(0);
    }

    public override void UpdateState()
    {
        if (_infinite) return;

        _timer += Time.deltaTime;
        if (_timer >= 1f)
        {
            _timer = 0f;

            if (CurrentStacksCount > 0)
            {
                CurrentStacksCount--;
                float removeValue = Mathf.Floor(_health.MaxValue * PercentPerStack);
                _health.AddMax(-removeValue);
                _characterState.StateIcons.RemoveIconCount();
            }

            _remaining -= 1f;
            if (_remaining <= 0f || CurrentStacksCount <= 0) ExitState();
        }
    }

    public override bool Stack(float _)
    {
        if (CurrentStacksCount >= MaxStacksCount) return false;

        CurrentStacksCount++;
        float addValue = Mathf.Floor(_health.MaxValue * PercentPerStack);
        _health.AddMax(addValue);

        if (!_infinite) SwitchToInfinite();
        return true;
    }

    public override void ExitState()
    {
        if (CurrentStacksCount > 0)
        {
            float removeValue = Mathf.Floor(_health.MaxValue * PercentPerStack * CurrentStacksCount);
            _health.AddMax(-removeValue);
        }

        _characterState.RemoveState(this);
    }
}
