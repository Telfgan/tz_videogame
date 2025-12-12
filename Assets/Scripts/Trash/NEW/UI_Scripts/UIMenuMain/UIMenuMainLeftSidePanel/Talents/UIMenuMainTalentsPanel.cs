using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class UIMenuMainTalentsPanel : MonoBehaviour
{
    [SerializeField] private UIMenuMainAttributesPanel _attributesPanel;
    [SerializeField] private UIMenuMainTalentsPanelGroup _talentsPanelGroup;
    [SerializeField] private RectTransform _itemsParent;
    [SerializeField] private TalentInfoPanel _talentInfoPanel;
    
    private List<UIMenuMainTalentsPanelGroup> ItemsPool = new();
    
    private TalentSystem _talentSystem;

    public void Show(TalentSystem talentSystem, bool isGameUI, bool isInteractable = true)
    {
        ResetPanel();
        
        _talentSystem = talentSystem;

        foreach (var data in _talentSystem.TalentsGroups)
        {
            var panel = Instantiate(_talentsPanelGroup, _itemsParent);
            
            panel.SetPanel(data, _attributesPanel, isGameUI, isInteractable);

            panel.OnShowPanelGroup += HidePanels;
            panel.PointerEnteredOnTalentIcon += ShowTalentInfo;
            panel.PointerExitedOnTalentIcon += HideTalentInfo;


            ItemsPool.Add(panel);
        }
    }

    private void OnDisable()
    {
        foreach (var item in ItemsPool)
        {
            item.OnShowPanelGroup -= HidePanels;
            item.PointerEnteredOnTalentIcon -= ShowTalentInfo;
            item.PointerExitedOnTalentIcon -= HideTalentInfo;
        }
    }

    private void ResetPanel()
    {
        if (ItemsPool.Count <= 0) return;
        
        foreach (var attribute in ItemsPool)
        {
            attribute.Destroy();
        }
        ItemsPool.Clear();
    }

    public void HidePanels()
    {
        foreach (var item in ItemsPool)
        {
            item.Hide();
        }
    }

    private void ShowTalentInfo(TalentData data)
    {
        _talentInfoPanel.Show(data);
    }

    private void HideTalentInfo(TalentData data)
    {
        _talentInfoPanel.Hide();
    }
}
