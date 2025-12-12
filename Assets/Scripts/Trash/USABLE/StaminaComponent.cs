using Mirror;
using TMPro;
using UnityEngine;

public abstract class StaminaComponent : NetworkBehaviour
{
	public Bar bar;
	public float CurrentValue { get { return _value; } }
	public float MaxValue { get { return _maxValue; } }

    public float RegenerationValue { get => _regenerationValue; set => _regenerationValue = value; }

    [SyncVar(hook = nameof(NetworkUpdateBar))]
	protected float _value;

	protected float _maxValue;
    private float _regenerationValue = 10;
    protected float _regenerationDelay = 3;
	
	private float _timerDelay = 0;

	public void Initialize(float maxValue, float regenValue, float regenDelay)
	{
		_value = maxValue;
		_maxValue = maxValue;
		_regenerationValue = regenValue;
		_regenerationDelay = regenDelay;
	}

	public abstract void Add(float value);
	public abstract bool TryUse(float value);

	protected virtual void Regen()
	{
		if (_value >= _maxValue) return;

        _timerDelay += Time.deltaTime;
		if (_timerDelay > _regenerationDelay)
		{
			_timerDelay = 0;
			Add(_regenerationValue);
		}
	}
	
	protected void ShowPopupValue(float value, Color startColor, Color endColor)
	{
		GetComponent<UIPlayerComponents>().ShowPopupValue(value,startColor,endColor);
	}

	protected void ShowPopupText(string text, Color startColor, Color endColor) //������������ ��� �������
	{
		GetComponent<UIPlayerComponents>().ShowPopupText(text,startColor,endColor);
	}

	protected void UpdateBar()
	{
		//bar.UpdateBar(_value,_maxValue);
	}
	public void NetworkUpdateBar(float oldValue, float newValue)
	{
		UpdateBar();
	}

	[Command]
	public void CmdUse(float value)
    {
		TryUse(value);
	}
}
