using System.Collections.Generic;
using UnityEngine;

public class DivineEnhancementState : AbstractCharacterState, IDamageGivenModifier
{
    private float _duration;

    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override States State => States.DivineEnhancement;
    public override StateType Type => StateType.Physical;
    public override List<StatusEffect> Effects => new() { StatusEffect.Ability };

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _duration = durationToExit;
    }

    public override void UpdateState()
    {
        _duration -= Time.deltaTime;
        if (_duration <= 0) ExitState();
    }

    public override void ExitState()
    {
        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        _duration = time;
        return true;
    }

    public float ModifyOutgoingDamage(Damage damage)
    {
        return damage.Value * 2f;
    }
}
