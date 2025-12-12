using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuneComponent : Resource
{
    private List<AbilityTimer> _abilities = new List<AbilityTimer>();
	[SerializeField] private float _sumDamageGiven = 0;
	private bool _disableMultiplier = false;

	/*private void Update()
    {
        Timer();
    }*/

	private void Awake()
	{
        _regenerationDelay = 3;
	}

	private bool RemoveRune(float runeValue, Skill usedAbility)
    {
        if (_abilities.Count > 0)
        {
            for (int i = 0; i < _abilities.Count; i++)
            {
                if (_disableMultiplier && _currentValue >= runeValue * _abilities[i].multiplier)
                {
                    _currentValue -= runeValue * _abilities[i].multiplier;
                    _disableMultiplier = false;
                    return true;
                }
                if (_abilities[i].ability == usedAbility && _currentValue >= runeValue * _abilities[i].multiplier * 2)
                {
                    _abilities[i].multiplier *= 2;

                    runeValue *= _abilities[i].multiplier;

                    _currentValue -= runeValue;
                    // _multiplyCost = true;
                    _abilities[i].time = 6;
                    return true;
                }
            }
            return false;
        }
        else
        {
            if (_currentValue >= runeValue)
            {
                AbilityTimer abilityTimer = new AbilityTimer();
                abilityTimer.time = 6;
                abilityTimer.multiplier = 1;
                abilityTimer.ability = usedAbility;
                _abilities.Add(abilityTimer);
                _disableMultiplier = false;
                _currentValue -= runeValue;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public void SwitchMultiplier(bool value)
    {
        _disableMultiplier = value;
    }

    private void Timer()
    {
        if (_abilities != null)
        {
            foreach (var ability in _abilities)
            {
                ability.time -= Time.deltaTime;
                if (ability.time <= 0)
                {
                    _abilities.Remove(ability);
                }
            }
        }
    }

    public void ResetValueRune()
    {
        _currentValue = _maxValue;
        _abilities.Clear();
        _disableMultiplier = false;
    }

    public void SumDamageMake(float damage)
    {
       // Debug.Log("SUM DAMAGE MAKE Rune" + damage);

        _sumDamageGiven += damage;
        while (_sumDamageGiven >= 50)
        {
            CmdAdd(1);
            _sumDamageGiven -= 50;
        }
    }
}

public class AbilityTimer
{
    public Skill ability;
    public float time;
    public float multiplier;
}
