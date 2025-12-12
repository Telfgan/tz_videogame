using System.Collections.Generic;
using UnityEngine;

public class SelfHarmState : AbstractCharacterState
{
    private const float CastTimeReductionMultiplier = 0.5f;
    private const float StackIncreasePerHit = 50f;
    private float _currentStackChance = 0f;  
    
    private List<StatusEffect> _effects = new ();
    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override States State => States.SpiritEnergy;
    public override StateType Type => StateType.Magic;
    public override List<StatusEffect> Effects => _effects;

    private Health _healthComponent;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        
        _healthComponent = _characterState.GetComponent<Health>();

        foreach (var skill in _characterState.Character.Abilities.Abilities)
        {
            if (skill.AbilityForm == AbilityForm.Magic)
            {
                skill.PreparingStarted += OnMagicSpellStartPreparing;
                skill.PreparingSuccess += OnMagicSpellEndCast;
            }
        }

        if (_healthComponent != null)
        {
            _healthComponent.DamageTaken += OnDamageTaken;
        }
        
        Debug.Log("SelfHarm enter");
    }

    public override void UpdateState()
    {
    }

    public override void ExitState()
    {
        foreach (var skill in _characterState.Character.Abilities.Abilities)
        {
            if (skill.AbilityForm == AbilityForm.Magic)
            {
                skill.PreparingStarted -= OnMagicSpellStartPreparing;
                skill.PreparingSuccess -= OnMagicSpellEndCast;
            }
        }
        
        if (_healthComponent != null)
        {
            _healthComponent.DamageTaken -= OnDamageTaken;
        }
        
        _characterState.RemoveState(this);
        
        Debug.Log("SelfHarm exit");
    }

    public override bool Stack(float time)
    {
        return false;
    }

    private void OnDamageTaken(Damage damage, Skill skill)
    {
        if (damage.Type != DamageType.Physical) return;
        
        _currentStackChance += StackIncreasePerHit;
        _currentStackChance = Mathf.Clamp(_currentStackChance, 0, 100);
        Debug.Log($"[SelfHarm] Chance increased to {_currentStackChance}%");
    }

    private void OnMagicSpellStartPreparing(Skill skill)
    {
        var isNeedUseBuff = Random.Range(0f, 100f) <= _currentStackChance;

        if (skill.School == Schools.Light && isNeedUseBuff)
        {
            skill.Buff.CastSpeed.IncreasePercentage(CastTimeReductionMultiplier);
            Debug.Log("[SelfHarm] Start preparing");
            Debug.Log(skill.CastDeley);
        }
    }
    
    private void OnMagicSpellEndCast(Skill skill)
    {
        var isNeedUseBuff = Random.Range(0f, 100f) <= _currentStackChance;

        if (skill.School == Schools.Light && isNeedUseBuff)
        {
            _currentStackChance = 0f;
            skill.Buff.CastSpeed.ReductionPercentage(CastTimeReductionMultiplier);
            Debug.Log("[SelfHarm] Success preparing");
            Debug.Log(skill.CastDeley);
        }
    }
}