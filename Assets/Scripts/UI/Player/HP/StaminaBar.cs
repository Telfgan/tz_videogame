using UnityEngine;

public class StaminaBar : Bar
{
    public override void UpdateBar()
    {
        _bar.value = _currentValue/_maxValue;
        _barText.text = Mathf.RoundToInt(_currentValue).ToString();
    }
}
