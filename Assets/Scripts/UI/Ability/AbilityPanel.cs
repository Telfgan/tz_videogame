using System.Collections.Generic;
using UnityEngine;

public class AbilityPanel : MonoBehaviour
{
    [SerializeField] private AbilityIcon _abilityIconPref;
    [SerializeField] private FillAmountOverTime _castLine;
    [SerializeField] private QueuePanel _queuePanel;
    [SerializeField]
    private SelectManager _selectManager;
    private Character _currentCharacter;
    private SkillManager _playerAbilities;
    private List<Skill> _abilities = new List<Skill>();
    private List<AbilityIcon> _abilityIcons = new List<AbilityIcon>();
    private bool _isActive;
    private bool _isSelect;

    private void Start()
    {
        _selectManager.CharacterSelected += OnCharacterSelected;
        _selectManager.CharacterDeselected += OnCharacterDeselected;
    }

    private void OnDestroy()
    {
        _selectManager.CharacterSelected -= OnCharacterSelected;
        _selectManager.CharacterDeselected -= OnCharacterDeselected;
    }

    public bool IsSelect
    {
        get => _isSelect;
        set => _isSelect = value;
    }

    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value && IsSelect;
            gameObject.SetActive(_isActive);
        }
    }

    public void Fill(SkillManager abilities)
    {
        if (_playerAbilities != null)
        {
            _playerAbilities.SkillSelected -= OnAbilitySelected;
            _playerAbilities.SkillDeselected -= OnAbilityDeselected;
            _playerAbilities.SkillAdded -= OnSkillAdded;
            _playerAbilities.SkillRemoved -= OnSkillRemoved;
        }

        _playerAbilities = abilities;
        _abilities.AddRange(_playerAbilities.Abilities);

        foreach (var item in _abilities)
        {
            AbilityIcon abilityIcon = Instantiate(_abilityIconPref, transform);
            abilityIcon.Init(item, _castLine);
            _abilityIcons.Add(abilityIcon);
        }

        _playerAbilities.SkillSelected += OnAbilitySelected;
        _playerAbilities.SkillDeselected += OnAbilityDeselected;
        _playerAbilities.SkillAdded += OnSkillAdded;
        _playerAbilities.SkillRemoved += OnSkillRemoved;
    }

    public void UpdatePanel()
    {
        ClearPanel();
        Fill(_currentCharacter.Abilities);
        _queuePanel.Init(_currentCharacter.Abilities.SkillQueue);
    }

    private void OnCharacterSelected(Character character)
    {
        Debug.Log(character);
        if (character != null && character != _currentCharacter)
        {
            gameObject.SetActive(true);
            _currentCharacter = character;
            Fill(_currentCharacter.Abilities);
            _queuePanel.Init(character.Abilities.SkillQueue);
        }
    } 

    private void OnCharacterDeselected(Character character)
    {
        Debug.Log(character);
        if (character != null && character == _currentCharacter)
        {
            ClearPanel();

            gameObject.SetActive(false);
            _currentCharacter = null;
        }
    }

    private void OnSkillAdded(Skill skill)
    {
        UpdatePanel();
    }

    private void OnSkillRemoved(Skill skill)
    {
        UpdatePanel();
    }

    private void ClearPanel()
    {
        foreach (var item in _abilityIcons)
        {
            Destroy(item.gameObject);
        }
        _abilityIcons.Clear();
        _abilities.Clear();
    }

    private void OnAbilitySelected(int index)
    {
        _abilityIcons[index].Selected();
    }

    private void OnAbilityDeselected(int index)
    {
        _abilityIcons[index].Deselected();
    }

    private void OnAutoAttackAbilitySelected(int index)
    {
        _abilityIcons[index].AutoAttackSelected();
    }

    private void OnAutoAttackAbilityDeselected(int index)
    {
        _abilityIcons[index].AutoAttackDeselected();
    }
}
