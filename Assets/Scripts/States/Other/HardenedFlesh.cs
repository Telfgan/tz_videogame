using System;
using System.Collections.Generic;
using UnityEngine;

public class HardenedFlesh : AbstractCharacterState
{
    private List<StatusEffect> _effects = new() { StatusEffect.Destruction };

    public override States State => States.HardenedFlesh;
    public override StateType Type => StateType.Physical;
    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override List<StatusEffect> Effects => _effects;

    private float _buffPercent = 0.2f;
    private int _currentStacks = 0;
    private const int _maxStacks = 5;

    private float _originalDefPhysDamage;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _health = character.Character.Health;
        _abilities = character.Character.Abilities;
        _personWhoMadeBuff = personWhoMadeBuff;

        if (_currentStacks == 0) _originalDefPhysDamage = _health.DefPhysDamage;

        duration = durationToExit;

        _health.DefPhysDamage = _originalDefPhysDamage + _originalDefPhysDamage * _buffPercent;
    }

    public override void ExitState()
    {
        _health.DefPhysDamage = _originalDefPhysDamage;
        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        duration = time;
        return false;
    }

    public override void UpdateState()
    {
        duration -= Time.deltaTime;

        if (duration <= 0)
        {
            ExitState();
            return;
        }
    }
}
