using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreeperInvisibleState : AbstractCharacterState
{
    private List<Skill> _skills = new();
    private CreeperInvisible _creeperInvisible;
    private Character _player;

    private float _reductionMoveSpeed = 0.3f;
    private float _originalMoveSpeed;
    private float _increaseStaminaRegen = 0.3f;
    private float _originalStaminaRegen;

    private static bool _isIncreasedManaCost = false;
    private bool _isCanApplyInvisible;
    private bool _playerInInvisible;

    private List<StatusEffect> _effects = new List<StatusEffect>() { StatusEffect.Move, StatusEffect.AbilitySpeed };
    public override States State => States.CreeperInvisible;
    public override StateType Type => StateType.Physical;
    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _player = _characterState.Character;

        _originalMoveSpeed = _player.Move.DefaultSpeed;
        _originalStaminaRegen = _player.TryGetResource(ResourceType.Mana).RegenerationDelay;

        if (_player != null)
        {
            _skills = _player.CharacterState.Character.Abilities.Abilities;
            foreach (Skill ability in _skills)
            {
                if (ability is CreeperInvisible creeperInvisible)
                {
                    if (_creeperInvisible == null)
                    {
                        _creeperInvisible = creeperInvisible;
                    }
                }
            }
        }
    }

    public override void UpdateState()
    {
        _isCanApplyInvisible = _creeperInvisible.IsInvisible;

        if (_isCanApplyInvisible)
        { 
            if (_playerInInvisible == false)
            {
                ApplyInvisible();
            }
        }
        else
        {
            ExitState();
        }
    }

    public override void ExitState()
    {
        _playerInInvisible = false;
        ResetValues();
        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        return false;
    }

    private void ApplyInvisible()
    {
        _playerInInvisible = true;

        float reductionMoveSpeed = _originalMoveSpeed * _reductionMoveSpeed;

        float endReductionMoveSpeed = _originalMoveSpeed - reductionMoveSpeed;

        _player.Move.SetMoveSpeed(endReductionMoveSpeed);

        _player.TryGetResource(ResourceType.Mana).RegenerationDelay *= (1 + _increaseStaminaRegen);

        if (_isIncreasedManaCost == false)
        {
            foreach (Skill ability in _skills)
            {
                ability.Buff.ManaCost.IncreasePercentage(1.3f);
            }
            _isIncreasedManaCost = true;
        }
    }

    private void ResetValues()
    {
        _player.Move.SetDefaultSpeed();

        if (_player.TryGetResource(ResourceType.Mana).RegenerationDelay != _originalStaminaRegen)
        {
            _player.TryGetResource(ResourceType.Mana).RegenerationDelay /= (1 + _increaseStaminaRegen);
        }

        if (_isIncreasedManaCost)
        {
            foreach (Skill ability in _skills)
            {
                ability.Buff.ManaCost.ReductionPercentage(1.3f);
            }
            _isIncreasedManaCost = false;
        }

        _playerInInvisible = false;
    }
}
