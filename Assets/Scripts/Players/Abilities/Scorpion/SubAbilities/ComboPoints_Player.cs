using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ComboPoints_Player : Resource
{
    //public float Value { get { return _value; } }

    //[SerializeField] protected int _value;
    //[SerializeField] protected int _maxValue;
    //[SyncVar(hook = nameof(HookMaxValueChanged))]
    //new protected float _maxValue = 3;
    public int ComboAbilities {  get; private set; }

    private void Start()
    {
        _currentValue = 0;
        _maxValue = 3;
    }
    public void RemoveAll()
    {
        _currentValue = 0;

        //visualize
    }
    public override void Add(float value)
    {
        //_currentValue += (int)value;
        _currentValue = Mathf.Clamp(value + _currentValue, 0, _maxValue);
        //if (_currentValue > _maxValue)
        //{
        //    _currentValue = _maxValue;
        //    UpdateBar();
        //    return;
        //}

        //visualize
        //_comboBar.TurnOn((int)value);
    }

    //public override bool TryUse(float value)
    //{

    //    if (_value >= value)
    //    {
    //        _value -= (int) value;
    //        ComboAbilities += (int) value;
    //        if (_value < 0) { _value = 0; }

    //        //visualize
    //        //_comboBar.TurnOff((int)value);
    //        UpdateBar();
    //        return true;
    //    }
    //    else return false;
    //}

    //protected override void UpdateBar()
    //{
    //    _comboBar.UpdateBar((int) _value);
    //}

    //[Command]
    //private void UpdateValue(int newValue)
    //{
    //    _value = newValue;
    //}
}
