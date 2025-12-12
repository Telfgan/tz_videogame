using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMenuMainCharactersPanelItem : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
{
    public event UnityAction<HeroComponent> Selected;
    
    [SerializeField] private Image _icon;

    public HeroComponent CurrentHero;
    private Vector3 _increasedScale = new Vector3(1.3f, 1.3f, 1);
    private Vector3 _standardScale = new Vector3(1f, 1f, 1);
    
    public void Fill(HeroComponent hero)
    {
        _icon.sprite = hero.Data.Icon;
        CurrentHero = hero;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = _increasedScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = _standardScale;
    }

    public void Select()
    {
        Selected?.Invoke(CurrentHero);
    }
}
