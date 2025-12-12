using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BindResetButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private List<RebindUI> _rebindsUI;

    private void Start()
    {
        _button.onClick.AddListener(ResetBind);
    }

    public void ResetBind()
    {
        foreach (var item in _rebindsUI)
        {
            item.ResetToDefault();
        }
    }
}
