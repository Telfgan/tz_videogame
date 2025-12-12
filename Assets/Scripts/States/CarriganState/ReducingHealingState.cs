using System.Collections.Generic;
using UnityEngine;

public class ReducingHealingState : AbstractCharacterState
{
    private AbstractCharacterState _state;

    private float _baseReductionHealingValues;
    private float _duration;
    private float _baseDuration;

    private float _startDelayBeforeChecking = 0.5f;
    private float _delayBeforeChecking;

    private Dictionary<AbstractCharacterState, float> _newHealingStatesValues = new();
    private Dictionary<AbstractCharacterState, float> _oldHealingStatesValues = new();

    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.ReducingEfficiency };

    public float BaseReductionHealingValue { get => _baseReductionHealingValues; set => _baseReductionHealingValues = value; }

    public override States State => States.ReducingHealing;
    public override StateType Type => StateType.Physical;
    public override BaffDebaff BaffDebaff => BaffDebaff.Debaff;
    public override List<StatusEffect> Effects => _effects;
    
    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        Debug.Log("ReducingHealingState / EnterState");
        _characterState = character;

        _duration = durationToExit;
        _baseDuration = durationToExit;

        _delayBeforeChecking = _startDelayBeforeChecking;
    }

    public override void UpdateState()
    {

    }

    public override void ExitState()
    {

    }

    public override bool Stack(float time)
    {
        return false;
    }

    private void UdpatingDictionaries()
    {
    }

    private void ReductionHealing()
    {
    }

    private List<AbstractCharacterState> TEST_GetStatesOnEffectAndType(StatusEffect effect, StateType type)
    {
        List<AbstractCharacterState> currentStates = new();

        if (_characterState.Check(effect) && _characterState.CheckStateType(type))
        {
            foreach (AbstractCharacterState state in _characterState.CurrentStates)
            {
                if (state.Effects.Contains(effect) && state.Type == type)
                {
                    currentStates.Add(state);
                }
            }

            if (currentStates.Count > 0)
            {
                return currentStates;
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }
}
