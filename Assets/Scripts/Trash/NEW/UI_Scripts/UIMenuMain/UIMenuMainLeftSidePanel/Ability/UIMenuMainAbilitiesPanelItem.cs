using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMenuMainAbilitiesPanelItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _icon;
    [SerializeField] private UITwoStates frameState;
    
    
    private Skill _ability;
    public void Fill(Skill ability)
    {
        _ability = ability;
        _icon.sprite = _ability.Icon;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        frameState.isActive = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        frameState.isActive = false;
    }
}
