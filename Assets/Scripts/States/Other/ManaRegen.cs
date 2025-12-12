using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaRegen : AbstractCharacterState
{
    private float _duration;
    private GameObject _manaRegen;

    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override States State => States.ManaRegen;
    public override StateType Type => StateType.Magic;
    public override List<StatusEffect> Effects => _effects;

    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Ability };

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _duration = durationToExit;
        _characterState = character;
        _personWhoMadeBuff = personWhoMadeBuff;
        _manaRegen = _characterState.StateEffects.ManaRegen;

        if (_manaRegen) _manaRegen.SetActive(true);
    }

    public override void ExitState()
    {
        if (_manaRegen) _manaRegen.SetActive(false);

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
