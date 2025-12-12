using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivateObjectWithToggle : MonoBehaviour
{
    public Toggle toggle; 
    private bool isFirstToggle = true;

    private void Start()
    {
        gameObject.SetActive(false);
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void OnToggleValueChanged(bool isOn)
    {
        if (isFirstToggle)
        {
            isFirstToggle = false;
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
            isFirstToggle = true;
        }
    }
}
