using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdealEvade : AbstractCharacterState
{
    private float _baseDuration;
    private float _duration;
    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Ability };
    public override States State => States.IdealEvade;

    public override StateType Type => StateType.Immaterial;
    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        Debug.Log("Entering IdealEvadeBuff State");
        _characterState = character;

        //type = StateType.Immaterial;
        //effects.Add(StatusEffect.Others);

        _duration = durationToExit;
        _baseDuration = durationToExit;
    }

    public override void ExitState()
    {
        Debug.Log("Exiting IdealEvadeBuff State");

        if (!_characterState.Check(StatusEffect.Others))
        {
            //return evade chance
        }

        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        _duration = _baseDuration;
        return true;
    }

    public override void UpdateState()
    {
        Debug.Log("Updating IdealEvadeBuff State");
        _duration -= Time.deltaTime;

        if (_duration < 0 /*|| turnOff*/)
        {
            ExitState();
        }
    }
}
