using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamsPanel : MonoBehaviour
{
    [SerializeField] private PlayerIcon _playerIconPref;
    [SerializeField] private Image _team1;
    [SerializeField] private Image _team2;

    private List<PlayerIcon> _playerIcons = new();

    public void AddInFirstTeam(Character character)
    {
        var icon = Instantiate(_playerIconPref, _team1.transform);
        icon.Init(character);
        _playerIcons.Add(icon);
    }

    public void AddInSecondTeam(Character character)
    {
        var icon = Instantiate(_playerIconPref, _team2.transform);
        icon.Init(character);
        _playerIcons.Add(icon);
    }

    public void StartReviveTimer(Character character, float time)
    {
        foreach (var item in _playerIcons)
        {
            if (item.Character == character)
            {
                item.StartReviveTimer(time);
                break;
            }   
        }
    }
}
