using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeSelectPanel : MonoBehaviour
{
    [SerializeField] private List<GameModeSelectButton> _gameMods;

    private GameMode _gameMode;

    public GameMode GameMode { get => _gameMode; }

    private void Awake()
    {
        foreach (var item in _gameMods)
        {
            item.Clicked += OnClick;
        }
    }

    private void OnClick(GameMode gameMode)
    {
        _gameMode = gameMode;
    }
}
