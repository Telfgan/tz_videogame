using System.Collections.Generic;
using UnityEngine;

public class IrradiationState : AbstractCharacterState
{
    private float _baseDuration;
    private float _durationIncrease = 1;
    private const float _magicDefenseReduction = 3;
    private float _totalAppliedReduction = 0f;

    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Ability};
    public override BaffDebaff BaffDebaff => BaffDebaff.Debaff;
    public override States State => States.Irradiation;
    public override StateType Type => StateType.Magic;
    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {

        Debug.Log("Entering Irradiation State");
        _characterState = character;
        _personWhoMadeBuff = personWhoMadeBuff;
        _baseDuration = durationToExit;
        duration = _baseDuration;

        MaxStacksCount = 3;

        _characterState.OnStateAdded += OnNewStateAdded;

        ExtendExistingNegativeMagic();
        ApplyMagicDefenseReduction();
    }

    public override void UpdateState()
    {
        duration -= Time.deltaTime;
        if (duration <= 0) ExitState();
    }

    public override void ExitState()
    {
        RestoreMagicDefense();
        _characterState.RemoveState(this);
        _characterState.OnStateAdded -= OnNewStateAdded;
    }

    public override bool Stack(float time)
    {
        if (CurrentStacksCount < MaxStacksCount)
        {
            CurrentStacksCount++;
            duration = _baseDuration;
            ApplyMagicDefenseReduction();

            Debug.Log($"Stacking Irradiation. Current stacks: {CurrentStacksCount}, New duration: {duration}s");
            return true;
        }
        else
        {
            duration = _baseDuration;
            Debug.Log($"Max stacks reached. Refreshing Irradiation duration: {duration}s");
            return false;
        }
    }

    private void ApplyMagicDefenseReduction()
    {
        _characterState.Character.Health.DefMagDamage -= _magicDefenseReduction;
        _totalAppliedReduction += _magicDefenseReduction;
    }

    private void RestoreMagicDefense()
    {
        _characterState.Character.Health.DefMagDamage += _totalAppliedReduction;
        _totalAppliedReduction = 0f;
    }

    private void OnNewStateAdded(AbstractCharacterState newState)
    {
        if (newState != this && newState.Type == StateType.Magic && newState.BaffDebaff == BaffDebaff.Debaff) ExtendState(newState);
    }

    private void ExtendExistingNegativeMagic()
    {
        foreach (var state in _characterState.CurrentStates) if (state != this && state.Type == StateType.Magic && state.BaffDebaff == BaffDebaff.Debaff) ExtendState(state);
    }

    private void ExtendState(AbstractCharacterState state)
    {
        state.duration += _durationIncrease;
        state.RemainingDuration += _durationIncrease;
        _characterState.StateIcons?.ActivateIco(state.State, state.RemainingDuration, 0, false, state.MaxStacksCount);
    }
}
