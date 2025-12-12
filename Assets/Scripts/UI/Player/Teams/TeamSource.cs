using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamSource : MonoBehaviour
{
    [SerializeField] private HeroInfoUI[] _heroInfoUI = new HeroInfoUI[2];

    private void OnEnable()
    {
        //UpdateInfo();
    }

    public void AddInFirstTeam(Character character)
    {
        //icon.Init(character);
        //_playerIcons.Add(icon);
        _heroInfoUI[0].SetHero(character);
    }

    public void AddInSecondTeam(Character character)
    {
        //icon.Init(character);
        //_playerIcons.Add(icon);
        _heroInfoUI[1].SetHero(character);
    }

    private void UpdateInfo()
    {
        foreach (var item in _heroInfoUI)
        {
            item.UpdateInfo();
        }
    }
}
