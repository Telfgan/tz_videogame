using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntressMark : AbstractCharacterState
{
    private float _duration;

    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override States State => States.HuntressMark;
    public override StateType Type => StateType.Physical;
    public override List<StatusEffect> Effects => _effects;

    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Ability };

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _duration = durationToExit;
        _characterState = character;
        _personWhoMadeBuff = personWhoMadeBuff;
    }

    public override void ExitState()
    {
        _characterState.StateIcons.RemoveItemByState(State);
        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        return false;
    }

    public override void UpdateState()
    {
        _duration -= Time.deltaTime;

        if (_duration <= 0) ExitState();
    }
}
