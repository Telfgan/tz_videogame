using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class UIMenuMainTalentsPanelGroup : MonoBehaviour
{
    [SerializeField] private UIMenuMainTalentsPanelGroupItem _talentPrefab;
    [SerializeField] private TMProLocalizer _title;
    [SerializeField] private TMProLocalizer _talentsCount;
    [SerializeField] private RectTransform _itemsParent;
    [SerializeField] private UIMenuTalentRow _rowContainer;

	private bool _isGameUI = false;
    public event UnityAction OnShowPanelGroup;

    private List<UIMenuMainTalentsPanelGroupItem> _talents = new ();
    private List<UIMenuTalentRow> _rows = new ();

    private TalentsGroup _talentsGroup;
    private UIMenuMainAttributesPanel _attributesPanel;

    public event Action<TalentData> PointerEnteredOnTalentIcon;
    public event Action<TalentData> PointerExitedOnTalentIcon;

    public void SetPanel(TalentsGroup talentsGroup, UIMenuMainAttributesPanel attributesPanel, bool isGameUI, bool isInteractable = true)
    {
        _isGameUI = isGameUI;
        
        _attributesPanel = attributesPanel;
        _talentsGroup = talentsGroup;
        _title.Localize(talentsGroup.Name);

        UpdateActiveTalentsCount();

        //foreach (var row in talentsGroup.TalentRows)
        for (int i = 0; i < talentsGroup.TalentRows.Count; i++)
        {
            var row = Instantiate(_rowContainer, _itemsParent);
            _rows.Add(row);
            foreach (var item in talentsGroup.TalentRows[i].Talents)
            {
                var talent = Instantiate(_talentPrefab, row.Rect);

                talent.Owner = this;
                talent.Fill(item.Data, i);
                item.Data.Row = i;
                talent.Button.interactable = isInteractable;

                talent.Selected += OnTalentSelected;
                talent.PointerEntered += OnPointerEnteredOnTalentIcon;
                talent.PointerExited += OnPointerExitedOnTalentIcon;

				row.AddTalent(talent);
				_talents.Add(talent);
            }
        }
        if (_rows != null)
            if (_rows.Count > 0)
            _rows[0].SetRowActive(true);
        for(int i = 0; i < _rows.Count - 1; i++)
        {
            foreach(var talent in _rows[i].Talents)
            {
                if(talent.Talent.IsOpen)
                {
                    _rows[i + 1].ActivateRow();
                }
                talent.Selected += _rows[i + 1].ActivateRow;
            }
        }
    }
    
    private void OnDisable()
    {
        foreach (var talent in _talents)
        {
            talent.Selected -= OnTalentSelected;
            talent.PointerEntered -= OnPointerEnteredOnTalentIcon;
            talent.PointerExited -= OnPointerExitedOnTalentIcon;
        }
    }

    void UpdateActiveTalentsCount()
    {
        //var activeTalentsCount = _talentsGroup.TalentsData.Count(o => o.Data.IsOpen);
        var activeTalentsCount = GetActiveTalents();

		_talentsCount.ChangeKey(activeTalentsCount);
    }

    void OnTalentSelected(TalentData talent, bool isOpen)
    {
		Debug.Log("Talent selected in MAIN" + talent);
		SaveManager.Instance.SaveTalent(_talentsGroup.ID, talent.Row, talent.Name, isOpen);
        SaveManager.Instance.LoadTalent(_talentsGroup.ID, talent.Row, talent.Name, _isGameUI);

        UpdateActiveTalentsCount();
        _attributesPanel.UpdateAttributesPoints();
    }
	private int GetActiveTalents()
	{
		List<Talent> activeTalents = new();
		
			foreach (TalentRow row in _talentsGroup.TalentRows)
			{
				foreach (Talent talent in row.Talents)
				{
					if (talent.Data.IsOpen)
					{
						activeTalents.Add(talent);
					}
				}
			}
		

		return activeTalents.Count;

	}
	public void Show()
    {
        if (_itemsParent.gameObject.activeInHierarchy == false)
        {
            OnShowPanelGroup?.Invoke();
            _itemsParent.gameObject.SetActive(true);
        }
        else
        {
            OnShowPanelGroup?.Invoke();
        }
    }
    
    public void Hide()
    {
        _itemsParent.gameObject.SetActive(false);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    private void OnPointerEnteredOnTalentIcon(TalentData talent)
    {
        PointerEnteredOnTalentIcon?.Invoke(talent);
    }

    private void OnPointerExitedOnTalentIcon(TalentData talent)
    {
        PointerExitedOnTalentIcon?.Invoke(talent);
    }
}