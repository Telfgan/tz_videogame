using System.Collections.Generic;
using UnityEngine;

public class ShadowTree : AbstractCharacterState
{
    public override States State => States.ShadowTree;
    public override StateType Type => StateType.Physical;
    public override BaffDebaff BaffDebaff => BaffDebaff.Null;
    public override List<StatusEffect> Effects => _effects;

    private readonly List<StatusEffect> _effects = new() { StatusEffect.Ability };
    private const float _heroBonusHealthPerStack = 3f;
    private const float _minionBonusHealthPerStack = 1f;
    private float BonusPerStack => _characterState.Character is HeroComponent ? _heroBonusHealthPerStack : _minionBonusHealthPerStack;

    private float _timer;
    private float _remaining;
    private bool _infinite;

    public override float RemainingDuration => _infinite ? 9999 : _remaining;

    public ShadowTree()
    {
        MaxStacksCount = 60;
    }

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
        _personWhoMadeBuff = caster;
        _infinite = true;
        duration = 9999;
        CurrentStacksCount = 0;
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
                _characterState.Character.Health.AddMax(-BonusPerStack);
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
        _characterState.Character.Health.AddMax(BonusPerStack);


        if (!_infinite) SwitchToInfinite();
        return true;
    }

    public override void ExitState()
    {
        if (CurrentStacksCount > 0)  _characterState.Character.Health.AddMax(-CurrentStacksCount * BonusPerStack);
        _characterState.RemoveState(this);
    }
}
