using UnityEngine;
using System.Collections;

public class LoadGamePressCharacter : MonoBehaviour
{
    [SerializeField] private UIMenuMainCharactersPanelItem uIMenuMainCharactersPanelItem;
    private UIMenuMainWindow _uiMenuMainWindow;

    void Start()
    {
        _uiMenuMainWindow = FindObjectOfType<UIMenuMainWindow>();

        if (_uiMenuMainWindow == null)
            Debug.LogError("UIMenuMainWindow не найден на сцене!");
    }

    public void SelectAndStart()
    {
        if (_uiMenuMainWindow == null) return;
        uIMenuMainCharactersPanelItem.Select();
        _uiMenuMainWindow.UI_StartClient();
    }
}
