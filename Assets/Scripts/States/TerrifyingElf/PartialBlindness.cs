using System;
using System.Collections.Generic;
using UnityEngine;

public class PartialBlindness : AbstractCharacterState
{
    private float _baseDuration;
    private float _duration;
    private int _maxStack = 3;
    private float _currentMissChance = 10f;
    private float _currentEffectiveness = 1f;
    private const float _missChanceReductionPerSecond = 0.04f;
    private const float _stackEffectivenessIncrease = 0.2f;
    private string _talentPartialBlindnessActive;

    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Ability };

    public override States State => States.PartialBlindness;
    public override StateType Type => StateType.Physical;
    public override BaffDebaff BaffDebaff => BaffDebaff.Debaff;
    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        Debug.Log("Entering PartialBlindness State");

        _characterState = character;
        _personWhoMadeBuff = personWhoMadeBuff;
        _baseDuration = durationToExit;
        _duration = _baseDuration;
        _currentEffectiveness = 1f;
        _talentPartialBlindnessActive = skillName;
        MaxStacksCount = _maxStack;

        ApplyMissChance();
    }

    public override void UpdateState()
    {
        _duration -= Time.deltaTime;

        if (_duration <= 0)
        {
            ExitState();
            return;
        }

        if (_talentPartialBlindnessActive == "partialBlindnessTalent")
        {
            _currentEffectiveness -= _missChanceReductionPerSecond * Time.deltaTime;
            _currentEffectiveness = Mathf.Max(0f, _currentEffectiveness);
            _currentMissChance = 10f * _currentEffectiveness;
        }

        ReduceMissChanceOverTime();
    }

    public override void ExitState()
    {
        Debug.Log("Exiting PartialBlindness State");
        _characterState.RemoveState(this);
        ResetMissChance();
        CurrentStacksCount = 0;
    }

    public override bool Stack(float time)
    {
        if (CurrentStacksCount < MaxStacksCount)
        {
            CurrentStacksCount++;
            _duration = _baseDuration;

            if (_talentPartialBlindnessActive == "partialBlindnessTalent")
            {
                Debug.Log("Talent is active, increasing effectiveness");
                _currentEffectiveness += _stackEffectivenessIncrease;
                _currentEffectiveness = Mathf.Clamp(_currentEffectiveness, 0f, 1f);
            }

            _currentMissChance = 10f * _currentEffectiveness;
            return true;
        }

        else if (CurrentStacksCount == MaxStacksCount)
        {
            Debug.Log("Max stacks reached for PartialBlindness.");
            _duration = _baseDuration;

            _currentMissChance = 10f * _currentEffectiveness;
            return false;
        }

        return false;
    }

    private void ApplyMissChance()
    {
        if (_characterState.Character.Health != null)
        {
            _characterState.Character.Health.EvadeMeleeDamage += _currentMissChance;
            _characterState.Character.Health.EvadeRangeDamage += _currentMissChance;
        }
    }

    private void ReduceMissChanceOverTime()
    {
        float effectivenessReduction = _missChanceReductionPerSecond * Time.deltaTime;
        _currentEffectiveness = Mathf.Max(0, _currentEffectiveness - effectivenessReduction);

        float oldMissChance = _currentMissChance;
        _currentMissChance = 10f * _currentEffectiveness;

        if (_characterState.Character.Health != null)
        {
            float reduction = oldMissChance - _currentMissChance;
            _characterState.Character.Health.EvadeMeleeDamage -= reduction;
            _characterState.Character.Health.EvadeRangeDamage -= reduction;
        }
    }

    private void ResetMissChance()
    {
        if (_characterState.Character.Health != null)
        {
            _characterState.Character.Health.EvadeMeleeDamage -= _currentMissChance;
            _characterState.Character.Health.EvadeRangeDamage -= _currentMissChance;
        }
        _currentMissChance = 10f;
        _currentEffectiveness = 1f;
    }
}
