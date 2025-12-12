using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillPanel : MonoBehaviour
{
    [SerializeField] private Camera _uiCamera;
    [SerializeField] private float _cameraCanvasDistance = 100;
    [SerializeField] private bool _hideUnusedButtons = true;
    [SerializeField] private List<RebindUI> _rebindsUI;
    [SerializeField] private SkillIcon[] _skillIcons;
    [SerializeField] private DraggableIcon _draggableIconPref;
    [SerializeField] private FillAmountOverTime _castLine;
    [SerializeField] private QueuePanel _queuePanel;
    [SerializeField] private AbilityNameBox _abilityNameBox;

    private List<DraggableIcon> _skills = new List<DraggableIcon>();
    private Character _currentCharacter;
    private SkillManager _playerAbilities;
    private bool _isActive;
    private bool _isSelect;

    private void Start()
    {
        UpdateKeys();

        foreach (var item in _rebindsUI)
        {
            item.updateBindingUIEvent.AddListener(OnRebindSpellKeys);
        }

        for (int i = 0; i < _skillIcons.Length; i++)
        {
            _skillIcons[i].Init(i);
            _skillIcons[i].CurrentSkillChenged += SkillChenged;
        }
    }

    public void Fill(SkillManager abilities)
    {
        ClearPanel();

        if (_playerAbilities != null)
        {
            _playerAbilities.SkillSelected -= OnAbilitySelected;
            _playerAbilities.SkillDeselected -= OnAbilityDeselected;
            _playerAbilities.SkillAdded -= OnSkillAdded;
            _playerAbilities.SkillRemoved -= OnSkillRemoved;
        }

        _playerAbilities = abilities;

        for (int i = 0; i < _playerAbilities.SelectedSkills.Length; i++)
        {
            if (_playerAbilities.SelectedSkills[i] == null)
            {
                _skillIcons[i].CurrentIcon = null;
                continue;
            }
                

            var icon = Instantiate(_draggableIconPref, _skillIcons[i].transform);
            icon.Init(_playerAbilities.SelectedSkills[i], _skillIcons[i].transform, _uiCamera, _cameraCanvasDistance);
            _skillIcons[i].CurrentIcon = icon;
            icon.transform.SetAsFirstSibling();
            _skills.Add(icon);

            icon.BeginDrag += OnBeginDrag;
            icon.EndDrag += OnEndDrag;
            icon.PointerEnter += OnPointerEnterIcon;
            icon.PointerExit += OnPointerExitIcon;
        }

        _playerAbilities.SkillSelected += OnAbilitySelected;
        _playerAbilities.SkillDeselected += OnAbilityDeselected;
        _playerAbilities.SkillAdded += OnSkillAdded;
        _playerAbilities.SkillRemoved += OnSkillRemoved;

        OnBeginDrag();
        OnEndDrag();
    }

    public void SetHideUnusedButtons(bool value)
    {
        if (value)
        {
            _hideUnusedButtons = value;

            OnEndDrag();
        }
        else
        {
            _hideUnusedButtons = value;

            foreach (var item in _skillIcons)
            {
                item.Show();
            }
        }
    }

    private void OnPointerEnterIcon(DraggableIcon skill)
    {
        _abilityNameBox.Show(skill.Skill);
        _abilityNameBox.gameObject.SetActive(true);
    }

    private void OnPointerExitIcon(DraggableIcon skill)
    {
        _abilityNameBox.gameObject.SetActive(false);
    }

    private void OnBeginDrag()
    {
        if (_hideUnusedButtons)
        {
            foreach (var item in _skillIcons)
            {
                item.Show();
            }
        }
    }
    
    private void OnEndDrag()
    {
        if (_hideUnusedButtons)
        {
            foreach (var item in _skillIcons)
            {
                if (item.CurrentIcon == null)
                {
                    item.Hide();
                }
            }
        }
    }

    private void SkillChenged(int index, Skill skill)
    {
        _playerAbilities.SelectedSkills[index] = skill;
    }

    public void OnCharacterSelected(Character character)
    {
        if (character != null && character != _currentCharacter)
        {
            gameObject.SetActive(true);
            _currentCharacter = character;
            Fill(_currentCharacter.Abilities);
            _queuePanel.Init(character.Abilities.SkillQueue);
        }
    }

    public void OnCharacterDeselected(Character character)
    {
        if (character != null && character == _currentCharacter)
        {
            ClearPanel();

            gameObject.SetActive(false);
            _currentCharacter = null;
        }
    }

    private void ClearPanel()
    {
        foreach (var item in _skills)
        {
            Destroy(item.gameObject);
        }
        _skills.Clear();
    }

    private void OnAbilitySelected(int index)
    {
        _skillIcons[index].Selected();
    }

    private void OnAbilityDeselected(int index)
    {
        _skillIcons[index].Deselected();
    }


    private void OnSkillAdded(Skill skill)
    {
		Fill(_playerAbilities);
		//UpdatePanel();
	}

    private void OnSkillRemoved(Skill skill)
    {
        Fill(_playerAbilities);
		//UpdatePanel();
	}

    public void UpdateKeys()
    {
        _skillIcons[0].Key.text = InputHandler.Instance.InputActions.GameplayMap.Spell1.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
        _skillIcons[1].Key.text = InputHandler.Instance.InputActions.GameplayMap.Spell2.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
        _skillIcons[2].Key.text = InputHandler.Instance.InputActions.GameplayMap.Spell3.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
        _skillIcons[3].Key.text = InputHandler.Instance.InputActions.GameplayMap.Spell4.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
        _skillIcons[4].Key.text = InputHandler.Instance.InputActions.GameplayMap.Spell5.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
        _skillIcons[5].Key.text = InputHandler.Instance.InputActions.GameplayMap.Spell6.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
        _skillIcons[6].Key.text = InputHandler.Instance.InputActions.GameplayMap.Spell7.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
        _skillIcons[7].Key.text = InputHandler.Instance.InputActions.GameplayMap.Spell8.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
        _skillIcons[8].Key.text = InputHandler.Instance.InputActions.GameplayMap.Spell9.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
        _skillIcons[9].Key.text = InputHandler.Instance.InputActions.GameplayMap.Spell10.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
        _skillIcons[10].Key.text = InputHandler.Instance.InputActions.GameplayMap.Spell11.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
        _skillIcons[11].Key.text = InputHandler.Instance.InputActions.GameplayMap.Spell12.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
        _skillIcons[12].Key.text = InputHandler.Instance.InputActions.GameplayMap.Spell13.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
        _skillIcons[13].Key.text = InputHandler.Instance.InputActions.GameplayMap.Spell14.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
        _skillIcons[14].Key.text = InputHandler.Instance.InputActions.GameplayMap.Spell15.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
        _skillIcons[15].Key.text = InputHandler.Instance.InputActions.GameplayMap.Spell16.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
    }

    public void OnRebindSpellKeys(RebindUI rebindUI, string key, string deviceLayoutName, string controlPath)
    {
        UpdateKeys();
    }
}
