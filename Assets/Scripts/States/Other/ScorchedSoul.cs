using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorchedSoul : AbstractCharacterState
{
    private float _duration;
    private int _currentStacks = 1;

    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Ability };

    public override States State => States.ScorchedSoul;
    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override StateType Type => StateType.Immaterial;
    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        Debug.Log("Entering ScorchedSoulDebuff State");

        _characterState = character;
        _duration = durationToExit;

        //effects.Add(StatusEffect.AbilitySpeed);
        //effects.Add(StatusEffect.AbilityCooldownSpeed);

        //Cmd111();
        var abilities = _characterState.GetComponentInChildren<SkillManager>();

        foreach (var ability in abilities.Abilities)
        {
            Debug.LogWarning($"Cast speed before: {ability.Buff.CastSpeed.Multiplier}");
            ability.Buff.CastSpeed.ReductionPercentage(.5f);
            Debug.LogWarning("Cast speed reduced!!!! - CharacterState.EnterState()");
            Debug.LogWarning($"Cast speed after: {ability.Buff.CastSpeed.Multiplier}");
            Debug.LogWarning($"Cast speed after: {ability.Buff.CastSpeed.GetBuffedValue(1f)}");
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exiting ScorchedSoulDebuff State");

        if (!_characterState.Check(StatusEffect.AbilitySpeed))
        {
            //return cast speed
            if (_characterState.TryGetComponent<SkillManager>(out SkillManager abilities))
            {
                foreach (var ability in abilities.Abilities)
                {
                    //ability.Buff.CastSpeed.IncreasePercentage(10f);
                }
            }
        }
        //if (_characterState.Check(StatusEffect.AbilityCooldownSpeed))
        //{
        //    //return abilitys' CD speed
        //}
        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        Debug.LogWarning($"Stacks ScorchedSoul: {_currentStacks}");

        _duration = time;

        if (_currentStacks >= 3)
            return false;

        _currentStacks++;
        return true;
    }

    public override void UpdateState()
    {
        Debug.Log("Updating ScorchedSoulDebuff State");

        _duration -= Time.deltaTime;

        if (_duration < 0)
        {
            ExitState();
        }
    }
}
