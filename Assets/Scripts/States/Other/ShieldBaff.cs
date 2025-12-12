using System;
using System.Collections.Generic;
using UnityEngine;

public class ShieldBaff : AbstractCharacterState, IDamageable
{
    private float _damageAbsorbed;
    private float _maxAbsorption;
    private float _curentAbsorption;
    private float _duration;

    public event Action<Damage, Skill> DamageTaken;
    public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
    public override States State => States.ShieldBaff;
    public override StateType Type => StateType.Magic;
    public override List<StatusEffect> Effects => new List<StatusEffect>();

    public Transform transform => throw new NotImplementedException();
    public GameObject gameObject => throw new NotImplementedException();

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
        _duration = durationToExit;
        _maxAbsorption = damageToExit;
        _curentAbsorption = _maxAbsorption;
        _damageAbsorbed = 0;
        _characterState.Character.Health.TotalMaxAbsorption += _maxAbsorption;
        _characterState.Character.Health.AddShieldValues(_maxAbsorption);

        UpdateShieldValues();
    }

    public override void UpdateState()
    {
        _duration -= Time.deltaTime;
        if (_duration <= 0)
        {
            ExitState();
        }
    }

    public override void ExitState()
    {
        Debug.Log("LightShield state exited.");
        _characterState.RemoveState(this);
        ResetCharacterShieldValues();
    }

    public override bool Stack(float time)
    {
        //_duration = time;
        //_damageAbsorbed = 0;
        CurrentStacksCount += 1;
        return false;
    }

    public bool TryTakeDamage(ref Damage damage, Skill skill)
    {
        Debug.Log($"Урон по стостоянию: {_damageAbsorbed}");
        float damageToAbsorb = Mathf.Min(_characterState.Character.Health.TotalMaxAbsorption - _damageAbsorbed, damage.Value);
        _damageAbsorbed += damageToAbsorb;
        damage.Value -= damageToAbsorb;
        _curentAbsorption = _maxAbsorption - _damageAbsorbed;

        var tempDamage = new Damage
        {
            Form = damage.Form,
            PhysicAttackType = damage.PhysicAttackType,
            School = damage.School,
            Type = damage.Type,
            Value = damageToAbsorb,
        };

        _characterState.GetComponent<Character>().DamageTracker.AddDamage(damage, null);
        DamageTaken?.Invoke(tempDamage, skill);

        _characterState.Character.Health.ClientRpcInvokeShieldDamageTaken(damageToAbsorb, damage.Type, skill);

        UpdateShieldValues();

        if (_damageAbsorbed >= _maxAbsorption)
        {
            ExitState();
            return true;
        }

        return damage.Value == 0;
    }

    public void UpdateShieldValues()
    {
        if (_characterState.Character.Health != null)
        {
            _characterState.Character.Health.UpdateShieldValues(_damageAbsorbed, _characterState.Character.Health.TotalMaxAbsorption);
        }
    }

    private void ResetCharacterShieldValues()
    {
        _characterState.Character.Health.TotalMaxAbsorption -= _curentAbsorption + _damageAbsorbed;
        _characterState.Character.Health.UpdateShieldValues(0, _characterState.Character.Health.TotalMaxAbsorption);
        if (_characterState.Character.Health.TotalMaxAbsorption <= 0) _characterState.Character.Health.ResetShieldValues();
        _characterState.Character.Health.AddShieldValues(-_curentAbsorption);
    }

    public void ShowPhantomValue(Damage phantomValue)
    {
        throw new NotImplementedException();
    }
}
