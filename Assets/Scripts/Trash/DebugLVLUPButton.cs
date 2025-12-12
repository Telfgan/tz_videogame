using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DebugLVLUPButton : MonoBehaviour
{
    [SerializeField] private SelectManager _selectManager;

    private Button _button;

    private void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(LVLUP);
    }

    public void LVLUP()
    {
        _selectManager.SelectedControllableUnits[0].LVL.CMDAddEXP(_selectManager.SelectedControllableUnits[0].LVL.ExperienceForNextLVL);
    }
}