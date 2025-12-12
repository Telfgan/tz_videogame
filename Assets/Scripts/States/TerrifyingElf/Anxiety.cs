using System.Collections.Generic;
using UnityEngine;

public class Anxiety : AbstractCharacterState
{
    private float spellSpeedReduction = 0.1f;
    private float manaCostIncrease = 0.1f;
    private const int maxStacks = 3;
    private float duration;

    private List<StatusEffect> _effects = new List<StatusEffect> { StatusEffect.Ability, StatusEffect.Strengthening };
    public override States State => States.Anxiety;
    public override StateType Type => StateType.Magic;
    public override BaffDebaff BaffDebaff => BaffDebaff.Debaff;
    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _abilities = character.Character.Abilities;
        _health = character.Character.Health;
        _personWhoMadeBuff = personWhoMadeBuff;
        MaxStacksCount = maxStacks;

        duration = durationToExit;

        ApplyEffects();
        Debug.Log($"Anxiety state applied: {CurrentStacksCount}/{MaxStacksCount} stacks, duration {duration}s");
    }

    public override void UpdateState()
    {
        duration -= Time.deltaTime;
        if (duration <= 0)
        {
            ExitState();
        }
    }

    public override void ExitState()
    {
        RemoveEffects();
        Debug.Log($"Anxiety state removed: {CurrentStacksCount}/{MaxStacksCount} stacks");
    }

    public override bool Stack(float newDuration)
    {
        if (CurrentStacksCount < MaxStacksCount)
        {
            CurrentStacksCount++;
        }
        duration = Mathf.Max(duration, newDuration);
        ApplyEffects();
        return true;
    }

    private void ApplyEffects()
    {
        if (_abilities != null)
        {
            foreach (var skill in _abilities.Abilities)
            {
                skill.CastDeley *= 1f + (spellSpeedReduction * CurrentStacksCount);

                foreach (var cost in skill.SkillEnergyCosts)
                {
                    cost.ModifyResourceCost(1f + (manaCostIncrease * CurrentStacksCount));
                }
            }
        }
    }

    private void RemoveEffects()
    {
        if (_abilities != null)
        {
            foreach (var skill in _abilities.Abilities)
            {
                skill.CastDeley /= 1f + (spellSpeedReduction * CurrentStacksCount);

                foreach (var cost in skill.SkillEnergyCosts)
                {
                    cost.ModifyResourceCost(1f / (1f + (manaCostIncrease * CurrentStacksCount)));
                }
            }
        }
    }
}
