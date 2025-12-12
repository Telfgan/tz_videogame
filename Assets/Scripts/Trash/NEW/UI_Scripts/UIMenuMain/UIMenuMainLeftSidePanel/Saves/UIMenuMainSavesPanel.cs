using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class UIMenuMainSavesPanel : MonoBehaviour
{
    [SerializeField] private UIMenuMainSavePanelItem savePanelItemItem;
    
    [SerializeField] private RectTransform _itemsParent;
    
    private List<UIMenuMainSavePanelItem> ItemsPool = new();

    private int _currentActiveIndex = 0;
    
    public event UnityAction<int> OnSelect;
    
    public void Show()
    {
        for (int i = 0; i < 3; i++)
        {
           var item = Instantiate(savePanelItemItem, _itemsParent);
           item.Owner = this;
           item.Fill(i + 1);
           ItemsPool.Add(item);
        }
    }

    public void Select(int index)
    {
        _currentActiveIndex = _currentActiveIndex ==  index ? 0 : index;
        OnSelect?.Invoke(_currentActiveIndex);
    }
}
