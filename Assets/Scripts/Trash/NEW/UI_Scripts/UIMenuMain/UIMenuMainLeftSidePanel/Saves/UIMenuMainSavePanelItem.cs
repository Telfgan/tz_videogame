using Mirror;
using UnityEngine;

public class UIMenuMainSavePanelItem : MonoBehaviour
{
    [ReadOnly, ShowInInspector]
    public UIMenuMainSavesPanel Owner;

    [SerializeField] private TMProLocalizer number;
    
    private int _index;

    public void Fill(int index)
    {
        _index = index;
        number.Localize(_index);
    }
    
    public void UI_Select()
    {
        Owner.Select(_index);
    }
}
