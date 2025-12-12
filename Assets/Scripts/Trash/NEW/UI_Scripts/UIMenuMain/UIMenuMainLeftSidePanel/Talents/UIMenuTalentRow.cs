using System;
using System.Collections.Generic;
using UnityEngine;

public class UIMenuTalentRow : MonoBehaviour
{
    [SerializeField] private RectTransform _rect;
    private List<UIMenuMainTalentsPanelGroupItem> _talents = new();
    private bool _isOpen = false;
	//private event Action<TalentData, bool> OnRowActivate;

	public bool isOpen => _isOpen;
    public RectTransform Rect => _rect;
    public List<UIMenuMainTalentsPanelGroupItem> Talents => _talents;

    //public void FireRowActivate(TalentData data, bool isOpen) => OnRowActivate?.Invoke(data, isOpen);

	private void Awake()
	{
		
	}

	public void AddTalent(UIMenuMainTalentsPanelGroupItem item)
    {
        _talents.Add(item);
    }

    public void ActivateRow(TalentData data, bool isOpen)
    {
        if (isOpen)
        {
            foreach (var item in _talents)
            {
                item.SetActive();
            }
        }
    }

	public void ActivateRow()
	{
		foreach (var item in _talents)
		{
			item.SetActive();
		}
	}

	public void SetRowActive(bool active)
    {
        _isOpen = active;
        
        if(active)
        {
            ActivateRow();
        }
    }
}
