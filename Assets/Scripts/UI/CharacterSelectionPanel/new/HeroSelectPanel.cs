using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSelectPanel : MonoBehaviour
{
	[SerializeField] private List<HeroComponent> _heroList;
    [SerializeField] private PlayerSelectionIcon _iconPref;

    private HeroComponent _selectedHero;
    private List<PlayerSelectionIcon> _iconList = new List<PlayerSelectionIcon>();

    public HeroComponent SelectedHero { get => _selectedHero; }
    public int SelectedHeroIndex { get => _heroList.IndexOf(_selectedHero); }
    public List<HeroComponent> HeroList { get => _heroList; }

    private void Start()
    {
        foreach (var item in _heroList)
        {
            var icon = Instantiate(_iconPref, transform);
            icon.Init(item);
            icon.Selected += OnCharacterSelected;
            _iconList.Add(icon);
        }
        _selectedHero = _heroList[0];
    }

    private void OnCharacterSelected(HeroComponent hero)
    {
        _selectedHero = hero;
	}
}
