using System;
using UnityEngine;

public class OnPressButton : MonoBehaviour
{
    public static event Action OnSpacePressed;

    [SerializeField] private GameObject workOn;
    [SerializeField] private GameObject workOff;
    [SerializeField] private UIMenuMainWindow uIMenuMainWindow;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (workOn != null) workOn.SetActive(true);
            if (workOff != null) workOff.SetActive(false);
            if (uIMenuMainWindow != null) uIMenuMainWindow.UI_StartClient();

            OnSpacePressed?.Invoke();
        }
    }
}
