using System.Collections.Generic;
using UnityEngine;

public class InAirState : AbstractCharacterState
{
    public bool turnOff = false;

    private float _duration;
    private float _baseDuration;
    private float _damageToExit;

    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Move };
    public override States State => States.InAir;
    public override StateType Type => StateType.Physical;
    public override BaffDebaff BaffDebaff => BaffDebaff.Debaff;
    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _characterState.Character.Move.CanMove = false;
        _duration = durationToExit;
        _baseDuration = _duration;
        _baseDuration = durationToExit;
    }

    public override void UpdateState()
    {
        _duration -= Time.deltaTime;
        if (_duration < 0 || turnOff)
        {
            ExitState();
        }
    }


    public override void ExitState()
    {
        _characterState.Character.Move.CanMove = true;
        
        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        return false;
    }
}
