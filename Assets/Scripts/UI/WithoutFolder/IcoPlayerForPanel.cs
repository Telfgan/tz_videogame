using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class IcoPlayerForPanel : MonoBehaviour
	//, IPointerEnterHandler, IPointerExitHandler
{
	public Image ico;
	public Button button;

	[SerializeField] private Image _outLine;
	[SerializeField] private Image _border;
	[SerializeField] private Sprite[] _borders;
	private bool _isActive = false;

	public void Init(Sprite icos, UnityAction action)
	{
		ico.sprite = icos;
		button.onClick.AddListener(action);
	}
	/*public void OnPointerEnter(PointerEventData eventData)
	{
		_outLine.DOFade(1, 0.2f);
		/*talentName.DOFade(1, 0.2f);
		description.transform.DOScale(1, 0.2f);
		Debug.Log("TEST On mouse enter");
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_outLine.DOFade(0, 0.2f);
		/*talentName.DOFade(0, 0.2f);
		description.transform.DOScale(0, 0.2f);
		Debug.Log("Mouse exit");
	}

	public void SwitchBorder()
	{
		if (_isActive)
		{
			_border.sprite = _borders[0];
			_isActive = false;
		}
		else
		{
			_border.sprite = _borders[1];
			_isActive = true;
		}
	}*/
}
