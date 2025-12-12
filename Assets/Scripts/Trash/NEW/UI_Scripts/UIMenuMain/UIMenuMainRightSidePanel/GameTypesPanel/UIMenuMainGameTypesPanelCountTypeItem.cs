using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class UIMenuMainGameTypesPanelCountTypeItem : MonoBehaviour
{
    public event UnityAction<GameMode> Selected;
    
    [SerializeField] private TMProLocalizer _itemTitle;
    [SerializeField] private GameMode _itemMode;
    
    public void Fill()
    {
        _itemTitle.Localize(_itemMode.ToString());
    }

    public void Select()
    {
        Selected?.Invoke(_itemMode);
    }
}
