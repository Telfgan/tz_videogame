using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SingleToggleGroup : MonoBehaviour
{
    public Toggle[] toggles;

    private void Start()
    {
        // Подписываемся на событие изменения состояния чекбокса для каждого тоггла
        foreach (var toggle in toggles)
        {
            toggle.onValueChanged.AddListener(isOn => OnToggleValueChanged(toggle, isOn));
        }
    }

    private void OnToggleValueChanged(Toggle toggle, bool isOn)
    {
        if (isOn)
        {
            foreach (var otherToggle in toggles)
            {
                if (otherToggle != toggle)
                {
                    otherToggle.isOn = false;
                }
            }
        }
    }
}

