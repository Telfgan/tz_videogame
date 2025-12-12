
using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class RegeneratingPoisonState : AbstractCharacterState
{
    /* For SpitPoison Ability */

    private Character _playerWithTalent;

    private int _maxStacks = 5;

    private float _baseHealingValue = 1.0f;
    private float _endHealingValue;

    private float _timeBetweenHeal;
    private float _startTimeBetweenHeal = 1.0f;

    private float _duration;
    private float _baseDuration;

    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Healing };

    public override States State => States.RegeneratingPoison;
    public override StateType Type => StateType.Physical;
    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        MaxStacksCount = _maxStacks;

        _characterState = character;
        _playerWithTalent = personWhoMadeBuff;

        _duration = durationToExit;
        _baseDuration = durationToExit;

        if (CurrentStacksCount < MaxStacksCount)
        {
            CurrentStacksCount++;
        }
    }

    public override void UpdateState()
    {
        _timeBetweenHeal -= Time.deltaTime;
        if (_timeBetweenHeal <= 0)
        {
            MakeHeal();
            _timeBetweenHeal = _startTimeBetweenHeal;
        }

        _duration -= Time.deltaTime;
        if (_duration < 0)
        {
            ExitState();
        }
    }

    public override void ExitState()
    {
        ResetValues();

        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        if (CurrentStacksCount < MaxStacksCount)
        {
            CurrentStacksCount++;
            _duration = _baseDuration;
            return true;
        }
        else
        {
            _duration = _baseDuration;
            return true;
        }
    }

    [Server]
    private void MakeHeal()
    {
        _endHealingValue = CurrentStacksCount * _baseHealingValue;

        Heal heal = new Heal
        {
            Value = _endHealingValue,
            DamageableSkill = null,
        };

        _characterState.Character.Health.Heal(ref heal, null);
        //_characterState.Character.DamageTracker.AddHeal(heal);
    }

    private void ResetValues()
    {
        CurrentStacksCount = 0;
        _endHealingValue = 0;
        _baseDuration = 0;
        _duration = 0;
    }
}
