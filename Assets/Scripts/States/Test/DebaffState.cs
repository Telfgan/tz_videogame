using System.Collections.Generic;
using UnityEngine;


public class DebaffState : AbstractCharacterState
{
    private float _durationRemaining;
    private string _skillName;

    public override States State => States.DebaffState;

    public override StateType Type => StateType.Magic;

    public override BaffDebaff BaffDebaff => BaffDebaff.Debaff;

    public override List<StatusEffect> Effects => new List<StatusEffect>() { StatusEffect.Strengthening };

    public DebaffState()
    {
        MaxStacksCount = 20;
        CurrentStacksCount = 1;
    }

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _personWhoMadeBuff = personWhoMadeBuff;
        _durationRemaining = durationToExit;
        _skillName = skillName;
    }

    public override void UpdateState()
    {
        if (_durationRemaining <= 0f)
        {
            ExitState();
            return;
        }

        _durationRemaining -= Time.deltaTime;
    }

    public override void ExitState()
    {
        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        if (CurrentStacksCount < MaxStacksCount)
        {
            CurrentStacksCount++;
            _durationRemaining = time;

            return true;
        }

        return false;
    }
}