using System;
using Mirror;

public class Level : NetworkBehaviour
{
    [SyncVar] protected int _experienceForNextLVL = 10;
    protected int _additionalToExperienceForNextLVL = 10;
    protected float _multiplierToExperienceForNextLVL = 1;
    private float _multiplierToExperience = 1;
    [SyncVar] private int _experience = 0;
    [SyncVar] private int _value = 1;
    private int _maxValue = 9;

    public int Value { get => _value; protected set { _value = value; LVLUped?.Invoke(_value); } }
    public int Experience { get => _experience; }
    public int ExperienceForNextLVL { get => _experienceForNextLVL; }
    public float MultiplierToExperience { get => _multiplierToExperience; set => _multiplierToExperience = value; }

    public event Action<int> EXPAdded;
    public event Action<int> LVLUped;
    public event Action<int> EXPForNextLVLChanged;

    public void AddEXP(int value)
    {
        if (value <= 0)
            return;

        value = (int)(value * _multiplierToExperience);

        _experience += value;
        EXPAdded?.Invoke(value);
        RpcUpdateInfo(_value, _experience, _experienceForNextLVL);

        var expBeyondNecessery = _experience - _experienceForNextLVL;

        if (expBeyondNecessery >= 0)
        {
            LVLUp();

            _experience = 0;
            IncreasExperienceForNextLVL();
            AddEXP(expBeyondNecessery);
        }
    }

    private void LVLUp()
    {
        if(_value + 1 <= _maxValue)
        {
            _value++;
            LVLUped?.Invoke(_value);
            RpcUpdateInfo(_value, _experience, _experienceForNextLVL);
        }
    }

    private void IncreasExperienceForNextLVL()
    {
        _experienceForNextLVL = (int)(_experienceForNextLVL * _multiplierToExperienceForNextLVL) + _additionalToExperienceForNextLVL;
        EXPForNextLVLChanged?.Invoke(_experienceForNextLVL);
        RpcUpdateInfo(_value, _experience, _experienceForNextLVL);
    }

    [Command]
    public void CMDAddEXP(int value)
    {
        AddEXP(value);
    }

    [ClientRpc]
    private void RpcUpdateInfo(int value, int experience, int experienceForNextLVL)
    {
        LVLUped?.Invoke(value);
        EXPAdded?.Invoke(experience);
        EXPForNextLVLChanged?.Invoke(experienceForNextLVL);
    }
}
