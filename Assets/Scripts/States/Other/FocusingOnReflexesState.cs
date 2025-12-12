using System.Collections.Generic;
using UnityEngine;

public class FocusingOnReflexesState : AbstractCharacterState
{
    private float _duration;
    private float _originalEvadeMelee;
    private float _originalEvadeRange;

    private Character _character;

    private List<StatusEffect> _effects = new() { StatusEffect.Evade };

    public override States State => States.FocusingOnReflexesState;
    public override StateType Type => StateType.Physical;
    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _duration = durationToExit;
        _character = character.Character;

        var health = _character.Health;

        _originalEvadeMelee = health.EvadeMeleeDamage;
        _originalEvadeRange = health.EvadeRangeDamage;

        health.EvadeMeleeDamage = 100f;
        health.EvadeRangeDamage = 60f;
        health.DamageTaken += OnDamageTaken;
    }

    public override void UpdateState()
    {
        _duration -= Time.deltaTime;

        if (_duration <= 0f)
        {
            ExitState();
        }
    }

    public override bool Stack(float time)
    {
        _duration = Mathf.Max(_duration, time);
        return true;
    }

    public override void ExitState()
    {
        if (_character != null)
        {
            var health = _character.Health;

            health.EvadeMeleeDamage = _originalEvadeMelee;
            health.EvadeRangeDamage = _originalEvadeRange;
            health.DamageTaken -= OnDamageTaken;
        }

        _characterState.RemoveState(this);
    }

    private void OnDamageTaken(Damage damage, Skill skill)
    {
        ExitState();
    }
}
