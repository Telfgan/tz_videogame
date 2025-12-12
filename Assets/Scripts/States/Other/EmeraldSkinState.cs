using System.Collections.Generic;
using UnityEngine;

public class EmeraldSkinState : AbstractCharacterState
{
    private float _buffDuration = 2f;
    private float _defenseIncrease = 0.9f;
    private float _physDefenseIncrease = 0f;
    private float _magDefenseIncrease = 0f;

    private float _flashBuffDuration = 1f;
    private float _lightMagicBuffDuration = 1f;
    private float _shieldBuffDuration = 2f;

    private bool _isTalentActive = false;
    
    private List<StatusEffect> _effects = new();
    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override States State => States.EmeraldSkin;
    public override StateType Type => StateType.Magic;
    public override List<StatusEffect> Effects => _effects;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _buffDuration = durationToExit;
        _isTalentActive = damageToExit > 0;
        
        ApplyBuff();
        
        foreach (var skill in _characterState.Character.Abilities.Abilities)
        {
            if (skill.School == Schools.Light && _isTalentActive)
            {
                skill.CastEnded += AddTimeByLightMagic;
            }

            switch (skill.Name)
            {
                case "FlashOfLight":
                    skill.CastEnded += AddTimeByFlash;
                    break;
                case "PriestShield":
                    skill.CastEnded += AddTimeByShield;
                    break;
                default:
                    continue;
            }
        }
    }

    public override void UpdateState()
    {
        _buffDuration -= Time.deltaTime;

        if (_buffDuration <= 0)
        {
            ExitState();
        }
    }

    public override void ExitState()
    {
        foreach (var skill in _characterState.Character.Abilities.Abilities)
        {
            if (skill.School == Schools.Light && _isTalentActive)
            {
                skill.CastEnded -= AddTimeByLightMagic;
            }
            switch (skill.name)
            {
                case "FlashOfLight":
                    skill.CastEnded -= AddTimeByFlash;
                    break;
                case "PriestShield":
                    skill.CastEnded -= AddTimeByShield;
                    break;
                default:
                    continue;
            }
        }

        Debug.Log("Emerald Skin state Exit");
        RemoveBuff();
        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        _buffDuration += time;
        return true;
    }

    private void AddTimeByFlash()
    {
        Debug.Log("Add time by flash - " + _flashBuffDuration);
        _buffDuration += _flashBuffDuration;

        _characterState.StateIcons?.ActivateIco(State, _buffDuration, 1, false);
    }

    private void AddTimeByShield()
    {
        Debug.Log("Add time by shield - " + _shieldBuffDuration);
        _buffDuration += _shieldBuffDuration;

        _characterState.StateIcons?.ActivateIco(State, _buffDuration, 1, false);
    }
    
    private void AddTimeByLightMagic()
    {
        Debug.Log("Add time by light - " + _lightMagicBuffDuration);
        _buffDuration += _lightMagicBuffDuration;

        _characterState.StateIcons?.ActivateIco(State, _buffDuration, 1, false);
    }

    private void ApplyBuff()
    {
        _physDefenseIncrease = _defenseIncrease - _characterState.Character.Health.DefPhysDamage;
        _magDefenseIncrease = _defenseIncrease - _characterState.Character.Health.DefMagDamage;
        
        _characterState.Character.Health.SetPhysicDef(_characterState.Character.Health.DefPhysDamage + _physDefenseIncrease);
        _characterState.Character.Health.SetMagicDef(_characterState.Character.Health.DefMagDamage + _magDefenseIncrease);
    }

    private void RemoveBuff()
    {
        _characterState.Character.Health.SetPhysicDef(_characterState.Character.Health.DefPhysDamage - _physDefenseIncrease);
        _characterState.Character.Health.SetMagicDef(_characterState.Character.Health.DefMagDamage - _magDefenseIncrease);
    }
}