using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttributeItem : MonoBehaviour
{
	[SerializeField] private Image _ico;
	[SerializeField] private TextMeshProUGUI _text;
	[SerializeField] private Button _plus;
	[SerializeField] private Button _minus;

	public Material mat;

	public Image Ico => _ico;
	public Button Plus => _plus;
	public Button Minus => _minus;

	[SerializeField] private bool _isPercent = false;
	private float _value;
	public void Init(Sprite ico, float value)
	{
	//	_ico.sprite = ico;
		_value = value;
		_text.text = value.ToString();

	//	_plus.onClick.AddListener(Add);
	//	_minus.onClick.AddListener(Remove);
	}

	public void Add()
	{
        if (_isPercent)
        {
			_value += 1;
        }
		else
		{
			_value *= 1.01f;
			_value = Mathf.Round(_value);
		}      
		
		_text.text = _value.ToString();
	}
	public void Remove()
	{
		if (_isPercent)
		{
			_value -= 1;
		}
		else
		{
			_value /= 1.01f;
			_value = Mathf.Round( _value );
		}
		_text.text = _value.ToString();
	}

	public void SetGreyScale(int grey)
	{
		mat = Instantiate(Ico.material);
		Ico.material = mat;
		mat.SetFloat("_GrayscaleAmount", grey);
	}
}
