using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SpiritHealthState : AbstractCharacterState
{
    private const float DamageHealthRestorePercent = 0.05f;
    private const int _baseMaxStacks = 3;

    private float _baseDuration;
    private float _duration;
    private float _regenAmount;

    private Health _healthComponent;
    private Resource _manaResource;
    private Character _character;

    private List<StatusEffect> _effects = new() { StatusEffect.Healing };
    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override States State => States.SpiritHealth;
    public override StateType Type => StateType.Magic;
    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _character = character.Character;
        _duration = durationToExit;
        _baseDuration = durationToExit;
        CurrentStacksCount = 1;
        MaxStacksCount = _baseMaxStacks;

        _healthComponent = _character.GetComponent<Health>();

        if (_healthComponent != null)
        {
            _healthComponent.DamageTaken += OnDamageTaken;
        }

        RecalcRegenAmount();
    }

    public override void UpdateState()
    {
        _duration -= Time.deltaTime;

        if (_duration <= 0)
        {
            ExitState();
        }
    }

    public override bool Stack(float time)
    {
        _duration = Mathf.Max(_duration, time);

        if (CurrentStacksCount < MaxStacksCount)
        {
            CurrentStacksCount++;
        }

        RecalcRegenAmount();
        return true;
    }

    public override void ExitState()
    {
        if (_healthComponent != null)
        {
            _healthComponent.DamageTaken -= OnDamageTaken;
        }

        _characterState.RemoveState(this);
    }

    private void OnDamageTaken(Damage damage, Skill skill)
    {
        if (_character == null || skill == null) return;

        if (_manaResource == null && skill.Hero != null) _manaResource = skill.Hero.TryGetResource(ResourceType.Mana);

        float manaRestoreValue = damage.Value * DamageHealthRestorePercent * CurrentStacksCount;

        ApplyRegen(manaRestoreValue);
    }

    public void ApplyRegen(float manaRestoreValue)
    {
        if (_manaResource != null && manaRestoreValue > 0) _manaResource.CmdAdd(manaRestoreValue);
    }

    private void RecalcRegenAmount()
    {
        if (_manaResource != null)
        {
            _regenAmount = _manaResource.MaxValue * DamageHealthRestorePercent * CurrentStacksCount;
        }
    }
}
