using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Rune : MonoBehaviour
{
    [SerializeField] private Image _runeImg;

    private float _value = 1;
    public float Value => _value;

    public void SetValue(float value)
    {
        _value = value;
        _runeImg.fillAmount = value;
    }
}
