using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.EventSystems;

public class AbilityUiIco : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] private GameObject _description;
	[SerializeField] private Image _ico;
	[SerializeField] private TextMeshProUGUI _text;

	public void OnPointerEnter(PointerEventData eventData)
	{
		_description.transform.DOScale(1, 0.2f);
		Debug.Log("TEST On mouse enter");
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_description.transform.DOScale(0, 0.2f);
		Debug.Log("Mouse exit");
	}

	public void Init(Sprite ico, string text)
	{
		_ico.sprite = ico;
		_text.text = text;
	}

}