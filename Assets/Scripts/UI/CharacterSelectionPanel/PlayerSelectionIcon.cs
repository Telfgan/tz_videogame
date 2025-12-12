using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerSelectionIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Transform _transform;
    [SerializeField] private Image _icon;
    private HeroComponent _playerPref;

    private Vector3 _increasedScale = new Vector3(1.3f, 1.3f, 1);
    private Vector3 _standardScale = new Vector3(1f, 1f, 1);

    public event UnityAction<HeroComponent> Selected;

    public void Init(HeroComponent player)
    {
        _playerPref = player;
        _icon.sprite = player.Data.Icon;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _transform.localScale = _increasedScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _transform.localScale = _standardScale;
    }

    public void OnClick()
    {
        Selected?.Invoke(_playerPref);
    }
}
