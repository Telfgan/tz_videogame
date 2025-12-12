using System;
using UnityEngine;

public class UITwoStates : MonoBehaviour
{
    [SerializeField]
    RectTransform enabledPanel;
        
    [SerializeField]
    RectTransform disabledPanel;

    [NonSerialized]
    public bool isActive = false;

    void Update()
    {
        enabledPanel.gameObject.SetActive(isActive);
        disabledPanel.gameObject.SetActive(!isActive);
    }
}
