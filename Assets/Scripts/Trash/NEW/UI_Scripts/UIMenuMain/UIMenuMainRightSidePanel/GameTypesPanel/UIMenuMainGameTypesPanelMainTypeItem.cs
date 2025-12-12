using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class UIMenuMainGameTypesPanelMainTypeItem : MonoBehaviour
{
    public event UnityAction<MainGameMode> Selected;
    
    [SerializeField] private TMProLocalizer _itemTitle;

    [SerializeField] private MainGameMode _itemMode;
    
    public void Fill()
    {
        _itemTitle.Localize(_itemMode.ToString());
    }

    public void Select()
    {
        Selected?.Invoke(_itemMode);
    }
}
