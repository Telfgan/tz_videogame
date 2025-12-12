using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Charges : MonoBehaviour
{
    private int _maxCharges;
    private float _chargeCooldown;
    TextMeshProUGUI _currentChargeText;
    private int _currentChargers;
    private Coroutine _rechargeJob;

    public int Chargers => _currentChargers;
    public bool IsHaveCharge { get => (_currentChargers > 0); private set { } }

    public void Init(int maxCharges, float chargeCooldown, TextMeshProUGUI counter)
    {
        _maxCharges = maxCharges;
        _currentChargers = _maxCharges;

        _chargeCooldown = chargeCooldown;

        _currentChargeText = counter;
        SetCurrentChargeText(_currentChargers);
    }

    public bool TryUseCharge()
    {
        if(_currentChargers > 0)
        {
            _currentChargers--;
            SetCurrentChargeText(_currentChargers);

            if (_rechargeJob == null)
                _rechargeJob = StartCoroutine(RechargeJob());
            return true;
        }
        else
        {
            return false;
        }
    }

    private void SetCurrentChargeText(int value)
    {
        if (value > 0)
            _currentChargeText.color = Color.green;
        else
            _currentChargeText.color = Color.red;

        _currentChargeText.text = value.ToString();
    }

    private IEnumerator RechargeJob()
    {
        while(_currentChargers < _maxCharges)
        {
            float time = 0;
            while (time < _chargeCooldown)
            {
                time += Time.deltaTime;
                yield return null;
            }
            _currentChargers++;
            SetCurrentChargeText(_currentChargers);
        }
        _rechargeJob = null;
    }
}
