using Mirror;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public enum ResourceType
{
    Health,
    Mana,
    Energy,
    Rune,
    Psionic,
    CooldownEnergy,
}

public abstract class Resource : NetworkBehaviour
{
    [SerializeField] private ResourceType _resourceType;
    [SerializeField, SyncVar] protected float _regenerationDelay = 0;
    [SyncVar(hook = nameof(HookValueChanged))] protected float _currentValue;
    [SyncVar(hook = nameof(HookMaxValueChanged))] protected float _maxValue;
    [SyncVar] protected float _regenerationValue;
    [SyncVar] protected float _regenerationPeriod;

    protected Coroutine _regenCoroutine;

    private float _bonusMaxValue = 0f;
	protected bool canRegen = true;
	public float CurrentValue { get => _currentValue; set { _currentValue = value; } }
    public float MaxValue { get => _maxValue; set => _maxValue = value; }
    public float RegenerationValue { get => _regenerationValue; set { _regenerationValue = value; } }
    public float RegenerationDelay { get => _regenerationPeriod; set { _regenerationPeriod = value; } }

    public ResourceType Type => _resourceType;

    public event Action<float, float> MaxValueChanged;
    public event Action<float, float> ValueChanged;
    public event Action<float> PhantomValueShown;
    public event Action<Color> ChangedBarColor;

    private void Awake()
    {
        ClientStartRegenirateJob();
    }

    private void OnEnable()
    {
        ClientStartRegenirateJob();
    }

    private void OnDisable()
    {
        ClientStopRegenerateJob();
    }

    public virtual void Initialize(float maxValue, float regenValue, float regenDelay, CharacterData data)
    {
        _currentValue = maxValue / 2;
        _maxValue = maxValue;
        _regenerationValue = regenValue;
        _regenerationPeriod = regenDelay;

        /*if (regenValue > 0)
            ClientStartRegenirateJob();*/
    }

    public virtual void Add(float value)
    {
        if (_maxValue >= _currentValue + value)
            _currentValue += value;
        else
            _currentValue = _maxValue;
    }

    public virtual bool TryUse(float value)
    {
        //TEST!!!
		if (_regenCoroutine != null)
		{
            //Debug.Log("Restart regen");
			StopCoroutine(_regenCoroutine);
			_regenCoroutine = StartCoroutine(RegenerateJob());
		}
		if (_currentValue - value >= 0)
        {
            CurrentValue -= value;
            return true;
        }
        else
        {
            CurrentValue = 0;
            return false;
        }
    }

    public void PhantomValueShow(float value)
    {
        //Debug.Log("SHOW PHANTOM " + gameObject + " Count " + value, this);
        PhantomValueShown?.Invoke(value);
    }

    public void InstCurrentValue(float value)
    {
        _currentValue = value;

        if (isServer) RpcResetValueUpdate();
        else HookValueChanged(0, _currentValue);
    }

    public void ChangeBarColor(Color color)
    {
        ChangedBarColor?.Invoke(color);
    }

    public void AddMax(float delta, bool keepPercent = false)
    {
        bool pauseRegen = _regenCoroutine != null && delta < 0 && Mathf.Approximately(_currentValue, _maxValue);

        if (pauseRegen)
        {
            StopCoroutine(_regenCoroutine);
            _regenCoroutine = null;
        }

        float oldMax = _maxValue;
        float oldCurrent = _currentValue;

        _maxValue += delta;

        if (delta > 0f) _currentValue += keepPercent ? _maxValue * (oldCurrent / oldMax) - oldCurrent : delta;

        else
        {
            if (keepPercent) _currentValue = _maxValue * (oldCurrent / oldMax);
            if (_currentValue > _maxValue) _currentValue = _maxValue;
        }

        if (!Mathf.Approximately(oldMax, _maxValue)) HookMaxValueChanged(oldMax, _maxValue);
        if (!Mathf.Approximately(oldCurrent, _currentValue)) HookValueChanged(oldCurrent, _currentValue);

        if (pauseRegen && _currentValue < _maxValue) _regenCoroutine = StartCoroutine(RegenerateJob());
    }


    protected virtual void HookValueChanged(float oldValue, float newValue)
    {
        ValueChanged?.Invoke(oldValue, newValue);
    }

    protected virtual void HookMaxValueChanged(float oldValue, float newValue)
    {
        MaxValueChanged?.Invoke(oldValue, newValue);
    }

    public void ResetValue()
    {
        _currentValue = _maxValue;
        RpcResetValueUpdate();
    }

    public void ChangedMaxValue(float value)
    {
        _maxValue += value;
    }

    public void Regenerate() => _regenCoroutine = StartCoroutine(RegenerateJob());

    protected IEnumerator RegenerateJob()
    {
        while (true)
        {
            if (_regenerationValue < 0) yield return null;

            if (_currentValue < _maxValue)
            {
                yield return new WaitForSeconds(_regenerationDelay);

                while (_currentValue < _maxValue && canRegen)
                {
                   // Debug.Log("REGENING");
                    CmdRegen();
                    yield return new WaitForSeconds(_regenerationPeriod);
                }
            }
            yield return null;
        }
    }

    [Command]
    public void ReduceRegenerationPeriod(float value)
    {
        _regenerationPeriod *= value;
    }

    [Command]
    public void IncreaseRegenerationPeriod(float value)
    {
        _regenerationPeriod /= value;
    }

    [Command]
    public void CmdUse(float value)
    {
        //Debug.Log(value + " try " + _currentValue);
        TryUse(value);
    }

    [Command]
    public void CmdAdd(float value)
    {
        Add(value);
    }

    [ClientCallback]
    protected void ClientStartRegenirateJob()
    {
        if (_regenCoroutine == null)
            _regenCoroutine = StartCoroutine(RegenerateJob());
    }

    [Client]
    protected void ClientStopRegenerateJob()
    {
        if (_regenCoroutine != null)
        {
            StopCoroutine(_regenCoroutine);
        }
    }

    [Command]
    protected void CmdRegen()
    {
        Add(_regenerationValue);
    }

    [ClientRpc]
    private void RpcResetValueUpdate()
    {
        HookValueChanged(0, _currentValue);
    }
}