using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UIMenuMainGameTypesPanel : MonoBehaviour
{
    [SerializeField] private List<UIMenuMainGameTypesPanelMainTypeItem> gameTypeMainTypeItem;
    [SerializeField] private List<UIMenuMainGameTypesPanelCountTypeItem> gameTypeCountTypeItem;

	[SerializeField] private RectTransform _mainItemsParent;
    [SerializeField] private RectTransform _countItemsParent;
    
    private List<UIMenuMainGameTypesPanelMainTypeItem> _mainGameTypes = new();
    private List<UIMenuMainGameTypesPanelCountTypeItem> _countGameTypes = new();


	public void Show()
    {
		 foreach (var item in gameTypeMainTypeItem)
		 {
			 item.Fill();
			 item.Selected += OnMainModeSelected;
			 _mainGameTypes.Add(item);
		 }

		 foreach (var item in gameTypeCountTypeItem)
		 {
			 item.Fill();
			 item.Selected += OnCountModeSelected;
			 _countGameTypes.Add(item);
		 }
	}

	private void OnCountModeSelected(GameMode mode)
    {
        ServerManager.Instance.SetMode(mode);
    }
    
    public void OnMainModeSelected(MainGameMode mode)
    {
        
    }
}
