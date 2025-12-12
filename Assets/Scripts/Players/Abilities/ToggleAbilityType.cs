using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleAbilityType : MonoBehaviour
{
    private Toggle _toggle;

    private void Start()
    {
        _toggle = GetComponent<Toggle>();
    }
    void Update()
    {
        if (_toggle.isOn)
        {
            AbilityTypeManager.ActiveAbilityType = 1;
        }
        else
        {
            AbilityTypeManager.ActiveAbilityType = 0;
        }
    }
}
