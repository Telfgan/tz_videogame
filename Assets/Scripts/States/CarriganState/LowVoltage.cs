using System.Collections.Generic;
using UnityEngine;

public class LowVoltage : AbstractCharacterState
{
    private const float ReductionPerStack = 0.15f;
    private const int MaxStack = 6;

    private float _duration;
    private float _remainingDuration;

    public override States State => States.LowVoltage;
    public override StateType Type => StateType.Physical;
    public override BaffDebaff BaffDebaff => BaffDebaff.Debaff;
    public override List<StatusEffect> Effects => new List<StatusEffect>();

    public LowVoltage()
    {
        CurrentStacksCount = 1;
        MaxStacksCount = MaxStack;
    }

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _personWhoMadeBuff = personWhoMadeBuff;

        _duration = durationToExit;
        _remainingDuration = _duration;
        CurrentStacksCount = 1;

        Debug.Log($"[LowVoltage] Applied! Max stacks: {MaxStacksCount}, duration: {_duration}s");

        _characterState.OnStateAdded += OnNewStateAdded;

        ApplyDebuffToActiveMagicBuffs();
    }

    public override void UpdateState()
    {
        _remainingDuration -= Time.deltaTime;

        if (_remainingDuration <= 0)
        {
            ExitState();
            _characterState.RemoveState(this);
            return;
        }
    }

    public override void ExitState()
    {
        Debug.Log("[LowVoltage] ExitState called");

        CurrentStacksCount = 0;

        _characterState.OnStateAdded -= OnNewStateAdded;

        _characterState.StateIcons.RemoveItemByState(State);

        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        if (CurrentStacksCount < MaxStacksCount)
        {
            CurrentStacksCount++;
        }

        _remainingDuration = time;

        Debug.Log($"[LowVoltage] Stacked to {CurrentStacksCount}. Remaining duration: {_remainingDuration}");

        ApplyDebuffToActiveMagicBuffs();

        return true;
    }

    private void ApplyDebuffToActiveMagicBuffs()
    {
        List<AbstractCharacterState> currentStates = _characterState.CurrentStates;

        foreach (var state in currentStates)
        {
            if (state == this) continue;
            if (state.Type != StateType.Magic) continue;
            if (state.BaffDebaff != BaffDebaff.Baff) continue;

            ReduceStateDuration(state, ReductionPerStack);
        }
    }

    private void OnNewStateAdded(AbstractCharacterState newState)
    {
        if (newState.Type != StateType.Magic || newState.BaffDebaff != BaffDebaff.Baff)
            return;

        float totalReduction = ReductionPerStack * CurrentStacksCount;
        ReduceStateDuration(newState, totalReduction);
    }

    private void ReduceStateDuration(AbstractCharacterState state, float reductionPercent)
    {
        var stateExitDurationField = state.GetType().GetField("_durationToExit", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (stateExitDurationField != null)
        {
            float currentDuration = (float)stateExitDurationField.GetValue(state);
            float reducedDuration = currentDuration * (1f - reductionPercent);

            reducedDuration = Mathf.Max(0.1f, reducedDuration);

            stateExitDurationField.SetValue(state, reducedDuration);
        }
    }
}
