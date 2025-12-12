using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIMenuMainCharactersPanel : MonoBehaviour
{
    [SerializeField] private UIMenuMainCharactersPanelItem _characterItem;
    [SerializeField] private RectTransform _itemsParent;

    public event UnityAction<HeroComponent> OnHeroChanged;
    
    private HeroComponent _currentHero;
    public HeroComponent CurrentHero => _currentHero;
    
    private List<UIMenuMainCharactersPanelItem> _characters = new();
    
    public void Show()
    {
        if (ServerManager.Instance == null || ServerManager.Instance.HeroList == null)
        {
            StartCoroutine(ShowJob());
            return;
        }
           
        var charactersGroup = ServerManager.Instance.HeroList;

        foreach (var item in charactersGroup)
        {
            var character = Instantiate(_characterItem, _itemsParent);
            character.Fill(item);
            character.Selected += OnPlayerSelected;
            _characters.Add(character);
        }

        if (_currentHero == null)
        {
            _characters[0].Select();   
        }
    }

    private IEnumerator ShowJob()
    {
        while (ServerManager.Instance == null || ServerManager.Instance.HeroList == null)
        {
            yield return null;
        }
        Show();
    }

    void OnPlayerSelected(HeroComponent hero)
    {
        _currentHero = hero;
        OnHeroChanged?.Invoke(_currentHero);
    }
}
