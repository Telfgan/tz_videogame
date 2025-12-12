using System;
using UnityEngine;

public class UIGameWindowPopup : MonoBehaviour
{
    [SerializeField] private UIMenuMainAttributesPanel _attributesPanel;
    [SerializeField] private UIMenuMainTalentsPanel _talentsPanel;
    [SerializeField] private PlayerIcon _playerIcon;
    [SerializeField] private MinionPanel _minionPanel;
    [SerializeField] private SkillPanel _skillPanel;
    [SerializeField] private SelectManager _selectManager;
    [SerializeField] private GameObject _settings;
    [SerializeField] private GameObject[] _forHide;

    private HeroComponent _currentHero;
    private Character _currentCharacter;

    private void Awake()
    {
        InputHandler.ShowMenu += ShowSettings;
    }

    public void SwichAll(bool value)
    {
        if (value == false)
            foreach (var item in _forHide)
                item.SetActive(false);
        else
            foreach (var item in _forHide)
                item.SetActive(true);
    }

    private void ShowSettings()
    {
        if (_settings.activeSelf)
        {
            _settings.SetActive(false);
        }
        else
        {
            _settings.SetActive(true);
        }
    }

    private void OnEnable()
    {
        _selectManager.CharacterSelected += OnCharacterSelected;
        _selectManager.CharacterDeselected += OnCharacterDeselected;
    }

    private void OnDisable()
    {
        _selectManager.CharacterSelected -= OnCharacterSelected;
        _selectManager.CharacterDeselected -= OnCharacterDeselected;
    }
    
    private void OnCharacterSelected(Character character)
    {
        _currentCharacter = character;

        if (character is not HeroComponent hero)
        {
            UpdateCharacterPanels();
            return;
        }
        
        _currentHero = hero;
        SaveManager.Instance.SetHero(_currentHero);
        UpdateCharacterPanels();
    }

    private void OnCharacterDeselected(Character character)
    {
        _playerIcon.OnCharacterDeselected(character);
        _minionPanel.OnCharacterDeselected(character);
        _skillPanel.OnCharacterDeselected(character);
        _attributesPanel.ShowHide(false);
        _attributesPanel.gameObject.SetActive(false);
        _talentsPanel.HidePanels();
        _talentsPanel.gameObject.SetActive(false);
    }
    
    private void UpdateCharacterPanels()
    {
        if(_currentHero == null)
            return;
        
        _playerIcon.OnCharacterSelected(_currentHero);
        _minionPanel.OnCharacterSelected(_currentHero);
        _skillPanel.OnCharacterSelected(_currentCharacter);

        _attributesPanel.gameObject.SetActive(true);
        _attributesPanel.Show(_currentHero.Data.Attributes);
        
        _talentsPanel.gameObject.SetActive(true);
        _talentsPanel.Show(_currentHero.TalentManager, true);

    }
}
