using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActiveToggleAtText : MonoBehaviour
{
    private List<Toggle> toggles = new List<Toggle>(); 
    public TextMeshProUGUI textMeshPro;

    private void Start()
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            GameObject childObject = gameObject.transform.GetChild(i).gameObject;

            Toggle toggle = childObject.GetComponent<Toggle>();

            if (toggle != null)
            {
                toggles.Add(toggle);
            }
        }
        // Инициализация текста
        UpdateToggleCount();

        // Подписываемся на события изменения состояния тогглов
        for (int i = 0; i < toggles.Count; i++)
        {
            toggles[i].onValueChanged.AddListener((value) => UpdateToggleCount());
        }
    }

    private void UpdateToggleCount()
    {
        // Подсчитываем количество включенных тогглов
        int enabledToggles = 0;
        for (int i = 0; i < toggles.Count; i++)
        {
            if (toggles[i].isOn)
            {
                enabledToggles++;
            }
        }

        // Отображаем количество включенных тогглов в текстовом поле
        textMeshPro.text = $"{enabledToggles}";
    }
}
