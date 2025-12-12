using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameModeSelectButton : MonoBehaviour
{
    [SerializeField] private GameMode _gameMode;

    public event Action<GameMode> Clicked;

    private void Awake()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        Clicked?.Invoke(_gameMode);
    }
}
