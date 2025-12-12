using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class HeatedGlandsState : AbstractCharacterState
{
    private int _maxStacks = 7;

    private float _duration;
    private float _baseDuration;

    private float _baseManaRegenIncrease = 0.3f;
    private float _allManaRegenIncrease;
    private float _baseManaRegen;

    private Character _player;
    private Resource _playerMana;

    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Strengthening };
    public override States State => States.HeatedGlands;
    public override StateType Type => StateType.Physical;
    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        Debug.Log("HeatedGlands / EnterState");

        MaxStacksCount = _maxStacks;

        _characterState = character;
        _player = personWhoMadeBuff;
        _playerMana = _player.TryGetResource(ResourceType.Mana);

        _duration = durationToExit;
        _baseDuration = _duration;

        _baseManaRegen = _player.TryGetResource(ResourceType.Mana).RegenerationValue;

        if (CurrentStacksCount < MaxStacksCount)
        {
            CurrentStacksCount++;
            IncreasingManaRegeneration();
        }
    }

    public override void UpdateState()
    {
        _duration -= Time.deltaTime;
        if (_duration < 0)
        {
            ExitState();
        }
    }

    public override void ExitState()
    {
        _player.TryGetResource(ResourceType.Mana).RegenerationValue = _baseManaRegen;
        
        _allManaRegenIncrease = 0;

        CurrentStacksCount = 0;

        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        if (CurrentStacksCount < MaxStacksCount)
        {
            CurrentStacksCount++;

            _duration = _baseDuration;

            IncreasingManaRegeneration();

            return true;
        }
        else
        {
            _duration = _baseDuration;

            return true;
        }
    }

    [Server]
    private void IncreasingManaRegeneration()
    {
        _allManaRegenIncrease += _baseManaRegenIncrease;
        Debug.Log("HeatedGlands / IncreasingManaRegen / _allManaRegenIncrease = " + _allManaRegenIncrease);
        float increasingManaRegen = _baseManaRegen * _allManaRegenIncrease;
        Debug.Log("HeatedGlands / IncreasingManaRegen / increasingManaRegen = " + increasingManaRegen);
        _player.TryGetResource(ResourceType.Mana).RegenerationValue = increasingManaRegen;
        Debug.Log("HeatedGlands / IncreasingManaRegen / player current ManaRegen = " + _player.TryGetResource(ResourceType.Mana).RegenerationValue);

    }
}
