using System.Collections.Generic;
using UnityEngine;

public class TiredSoul : AbstractCharacterState
{
    private float _duration;
    private float _baseDuration;

    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override States State => States.TiredSoul;
    public override StateType Type => StateType.Magic;
    public override List<StatusEffect> Effects => new List<StatusEffect>();

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _duration = durationToExit;
        _baseDuration = durationToExit;
        CurrentStacksCount++;
        MaxStacksCount = 2;
    }

    public override void UpdateState()
    {
        _duration -= Time.deltaTime;
        
        if (_duration <= _baseDuration * (CurrentStacksCount - 1) && CurrentStacksCount > 0)
        {
            CurrentStacksCount--;
            _duration = _baseDuration * CurrentStacksCount;

            if (CurrentStacksCount == 0)
            {
                ExitState();
            }
        }
    }

    public override void ExitState()
    {
       if(!_characterState.CheckForState(States.TiredSoul)) 
           return;
       
       _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        if (CurrentStacksCount < MaxStacksCount)
        {
            CurrentStacksCount++;
            _duration += time;
            _duration = Mathf.Min(_duration, _baseDuration * CurrentStacksCount);
        }
        return true;
    }
}