using System.Collections.Generic;
using UnityEngine;

public class DefenceReductionState : AbstractCharacterState
{
    private float _healthBuffActiveTime = 2f;
    private float _healthBoostPercentage = 0.25f;
    private float _defaultPhysDef = 0;

    private List<StatusEffect> _effects = new ();
    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override States State => States.DefenseReduction;
    public override StateType Type => StateType.Magic;
    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _healthBuffActiveTime = durationToExit;
        _healthBoostPercentage = damageToExit;
        ApplyBuff();
    }

    public override void UpdateState()
    {
        _healthBuffActiveTime -= Time.deltaTime;

        if (_healthBuffActiveTime <= 0)
        {
            ExitState();
        }
    }

    public override void ExitState()
    {
        RemoveBuff();
        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        return false;
    }

    private void ApplyBuff()
    {
        _defaultPhysDef = _characterState.Character.Health.DefPhysDamage;
        _characterState.Character.Health.SetPhysicDef(_defaultPhysDef * _healthBoostPercentage);
    } 

    private void RemoveBuff()
    {
        _characterState.Character.Health.SetPhysicDef(_defaultPhysDef);
    }
}