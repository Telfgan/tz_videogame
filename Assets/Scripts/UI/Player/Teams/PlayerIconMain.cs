using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerIconMain : PlayerIcon, IPointerExitHandler, IPointerEnterHandler
{
    [SerializeField] private LvlInfo _lvlInfo;
    [SerializeField] private Image _FrameBlink;

    public void OnPointerEnter(PointerEventData eventData)
    {
        _FrameBlink.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _FrameBlink.gameObject.SetActive(false);
    }

    protected override void UpdateInfo(Character character)
    {
        base.UpdateInfo(character);
        _lvlInfo.Init(character.LVL);
    }
}
