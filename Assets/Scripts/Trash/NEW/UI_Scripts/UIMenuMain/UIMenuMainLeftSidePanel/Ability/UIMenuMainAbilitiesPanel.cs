using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class UIMenuMainAbilitiesPanel : MonoBehaviour
{
    [SerializeField] private UIMenuMainAbilitiesPanelItem _abilityItem;
    [SerializeField] private RectTransform _itemsParent;
    
    private SkillManager _abilitiesComponent;
    private List<UIMenuMainAbilitiesPanelItem> _abilities = new ();
    
    public void Show(SkillManager skillManager)
    {
        if (_abilitiesComponent != null)
        {
            _abilitiesComponent.SkillAdded -= UpdatePanel;
            _abilitiesComponent.SkillRemoved -= UpdatePanel;
        }

        _abilitiesComponent = skillManager;

        _abilitiesComponent.SkillAdded += UpdatePanel;
        _abilitiesComponent.SkillRemoved += UpdatePanel;

        ResetPanel();

        foreach (var item in _abilitiesComponent.DefaultSkills)
        {
            var abilityIcon = Instantiate(_abilityItem, _itemsParent);
            abilityIcon.Fill(item);
            _abilities.Add(abilityIcon);
        }
    }

    private void ResetPanel()
    {
        if (_abilities.Count > 0)
        {
            foreach (var ability in _abilities)
            {
                ability.Destroy();
            }
            _abilities.Clear();
        }
    }

    private void UpdatePanel(Skill skill)
    {
        Show(_abilitiesComponent);
    }
}
