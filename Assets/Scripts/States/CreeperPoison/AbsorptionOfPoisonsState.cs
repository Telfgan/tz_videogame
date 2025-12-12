using System.Collections.Generic;
using UnityEngine;

public class AbsorptionOfPoisonsState : AbstractCharacterState
{
    private Character _player;

    private float _maxHealth;
    private float _baseHealthIncrease = 0.1f;
    private float _increasedHealth;
    private float _allIncreasedHealth;

    private float _duration;
    private float _baseDuration;

    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Absorptions };
    public override States State => States.AbsorptionOfPoison;
    public override StateType Type => StateType.Physical;
    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _player = personWhoMadeBuff;

        _duration = durationToExit;
        _baseDuration = durationToExit;

        _maxHealth = _player.Health.MaxValue;

        IncreaseHealth();
    }

    public override void UpdateState()
    {
        _duration -= Time.deltaTime;

        if (_duration <= 0)
        {
            ExitState();
        }
    }

    public override bool Stack(float time)
    {
        CurrentStacksCount++;

        _duration = _baseDuration;

        IncreaseHealth();

        return true;
    }

    public override void ExitState()
    {
        _player.Health.ChangedMaxValue(-_allIncreasedHealth);

        ResetValues();

        _characterState.RemoveState(this);
    }

    private void IncreaseHealth()
    {
        float increasingValue = CurrentStacksCount * _baseHealthIncrease;

        _increasedHealth = _maxHealth * increasingValue;

        _player.Health.ChangedMaxValue(_increasedHealth);

        _allIncreasedHealth += _increasedHealth;
    }

    private void ResetValues()
    {
        _allIncreasedHealth = 0;

        CurrentStacksCount = 0;

        _duration = 0;

        _baseHealthIncrease = 0.1f;

        _increasedHealth = 0;
    }
}
