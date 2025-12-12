using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class FireFlash : AbstractCharacterState
{
    private readonly List<StatusEffect> _effects = new() { StatusEffect.Ability };

    public override States State => States.FireFlash;
    public override StateType Type => StateType.Immaterial;
    public override BaffDebaff BaffDebaff => BaffDebaff.Null;
    public override List<StatusEffect> Effects => _effects;

    private int _ñhance => CurrentStacksCount * 10;

    private float _timer;
    private float _remaining;
    private bool _infinite;

    public override float RemainingDuration => _infinite ? 9999 : _remaining;
    public int Chance { get => _ñhance; }

    public FireFlash() => MaxStacksCount = 3;

    public void SwitchToFinite()
    {
        _timer = 0f;
        _infinite = false;
        _remaining = Mathf.Clamp(CurrentStacksCount, 1, 9999);
    }

    public void SwitchToInfinite()
    {
        _infinite = true;
        _timer = 0f;
        duration = 9999;
    }

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character caster, string skillName)
    {
        _characterState = character;
        duration = durationToExit;
        _timer = 0f;
        CurrentStacksCount = 1;
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
                _characterState.StateIcons.RemoveIconCount();
            }

            _remaining--;
            if (CurrentStacksCount <= 0) ExitState();
        }
    }

    public override void ExitState() => _characterState.RemoveState(this);

    public override bool Stack(float time)
    {
        if (CurrentStacksCount >= MaxStacksCount) return false;
        CurrentStacksCount++;
        if (!_infinite) SwitchToInfinite();
        _characterState?.StateIcons?.ActivateIco(State, RemainingDuration, 1, true, MaxStacksCount);
        return true;
    }
}
