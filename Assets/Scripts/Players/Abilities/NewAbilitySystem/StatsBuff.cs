using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsBuff
{
    private StatBuff _damage = new StatBuff();
    private StatBuff _radius = new StatBuff();
    private StatBuff _area = new StatBuff();
    private StatBuff _attackSpeed = new StatBuff();
    private StatBuff _castSpeed = new StatBuff();
    private StatBuff _cooldown = new StatBuff();
    private StatBuff _manaCost = new StatBuff();

    public StatBuff Damage => _damage;
    public StatBuff Radius => _radius;
    public StatBuff Area => _area;
    public StatBuff AttackSpeed => _attackSpeed;
    public StatBuff CastSpeed => _castSpeed;
    public StatBuff Cooldown => _cooldown;
    public StatBuff ManaCost => _manaCost;
}

public class StatBuff
{
    private float _multiplier = 1;
    private float _additional = 0;

    public float Multiplier => _multiplier;
    public float Additional => _additional;

    public float GetBuffedValue(float value)
    {
        return (value + _additional) * _multiplier;
    }

    public void IncreasePercentage(float value)
    {
        _multiplier *= value;
    }

    public void ReductionPercentage(float value)
    {
        _multiplier /= value;
    }

    public void AddValue(float value)
    {
        _additional += value;
    }
}
