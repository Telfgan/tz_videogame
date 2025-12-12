using DG.Tweening;
using JetBrains.Annotations;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Bar : MonoBehaviour
{
	[CanBeNull]
	[SerializeField] private Resource _resource;
	[SerializeField] private Slider _shieldBar;
	[SerializeField] protected Slider _bar;
	[SerializeField] protected Slider _barMinus;
	//[SerializeField] protected Slider _barMinusPrewiew;
	[SerializeField] protected Slider _barPlus;
	[SerializeField] protected float _timeToDisapear = 0.2f;
	[SerializeField] protected float _disapearSpeed = 0.5f;
	[SerializeField] protected float _timeToShow = 0.2f;
	[SerializeField] protected float _ShowSpeed = 0.5f;
	[SerializeField] protected bool _showText = true;
	[SerializeField] protected TMP_Text _barText;
	[SerializeField] protected Image _barImage;

	protected float _currentValue;
	protected float _healthBarTarget;
	protected float _maxValue;
	protected float _preViewValue;
	private Health _health;

	private bool ShieldActive = false;

	public virtual void Init(Resource resource)
    {
		if(_resource != null)
        {
			_resource.ValueChanged -= OnValueChanged;
			_resource.PhantomValueShown -= PreviewChange;
			_resource.MaxValueChanged -= OnMaxValueChanged;
			_resource.ChangedBarColor -= OnChangeBarColor;
		}

		_resource = resource;

		_currentValue = resource.CurrentValue;
		_preViewValue = resource.CurrentValue;
		_maxValue = resource.MaxValue;

		UpdateBar();

		_resource.ValueChanged += OnValueChanged;
		_resource.PhantomValueShown += PreviewChange;
		_resource.MaxValueChanged += OnMaxValueChanged;
		_resource.ChangedBarColor += OnChangeBarColor;

		_health = resource as Health;
		if (_health != null)
		{
			_health.ShieldDeactivated += OnShieldDeactivated;
			_health.OnShieldValuesChanged += UpdateShieldBar;
		}

		if (_shieldBar != null)
		{
			UpdateShieldVisual();
		}
	}

    private void Start()
    {
        if (_resource != null) Init(_resource);
    }

    private void OnEnable()
    {
		if (_resource != null) Init(_resource);
	}

    private void OnDestroy()
    {
		if (_resource != null)
		{
			_resource.ValueChanged -= OnValueChanged;
			_resource.PhantomValueShown -= PreviewChange;
			_resource.MaxValueChanged -= OnMaxValueChanged;
			_resource.ChangedBarColor -= OnChangeBarColor;
		}
		if (_health != null)
		{
			_health.ShieldDeactivated -= OnShieldDeactivated;
			_health.OnShieldValuesChanged -= UpdateShieldBar;
		}
	}

	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.O)) OnChangeBarColor(Color.red);
	}

	public virtual void UpdateBarWithShield(float healthBarTarget)
	{
		//_bar.value = _healthBarTarget;
		_bar.DOValue(_healthBarTarget, _disapearSpeed);

		if (_showText) _barText.text = Mathf.RoundToInt(_currentValue).ToString();
		if (gameObject.activeInHierarchy) StartCoroutine(DisapearBar());
	}

	public virtual void UpdateBar()
	{
		if(_maxValue != 0)
			//_bar.value = _currentValue / _maxValue;
			_bar.DOValue(_currentValue / _maxValue, _disapearSpeed);

		if(_showText) _barText.text = Mathf.RoundToInt(_currentValue).ToString();
		if (gameObject.activeInHierarchy) StartCoroutine(DisapearBar());
	}

	private void OnValueChanged(float oldValue, float newValue)
    {
        _currentValue = newValue;

		if (_health != null && _health.IsDot) _bar.value = _currentValue / _maxValue;

		if (_shieldBar != null)
		{
			UpdateShieldVisual();
			if (ShieldActive) UpdateBarWithShield(_healthBarTarget);
			else UpdateBar();
		}

		else UpdateBar();
	}
    
    private void OnMaxValueChanged(float oldValue, float newValue)
    {
        _maxValue = newValue;

		if (_shieldBar != null)
		{
			UpdateShieldVisual();
			if (ShieldActive) UpdateBarWithShield(_healthBarTarget);
			else UpdateBar();
		}

		UpdateBar();
	}

	private IEnumerator DisapearBar()
	{
		yield return new WaitForSeconds(_timeToDisapear);
		if (ShieldActive)
        {
			_barMinus.DOValue(_healthBarTarget, _disapearSpeed);
		}

		else
		{
			//_barMinusPrewiew.DOValue(_currentValue / _maxValue, _disapearSpeed);
			_barMinus.DOValue(_currentValue / _maxValue, _disapearSpeed);
		}
	}

	public void PreviewDoTTick(float tickDamage)
	{
		float previewTarget = (_barMinus.value * _maxValue - tickDamage) / _maxValue;
		//float previewTarget = (_barMinusPrewiew.value * _maxValue - tickDamage) / _maxValue;
		previewTarget = Mathf.Clamp01(previewTarget);

		//_barMinusPrewiew.DOValue(previewTarget, 0.25f);
		_barMinus.DOValue(previewTarget, 0.25f);
	}

	public void PreviewChange(float damage)
	{
		float newValue = _currentValue - damage;
		//Debug.Log(newValue + " new " + _currentValue + " cur " + _maxValue + " max" );
		if (_barPlus != null)
		{
			if (newValue < _currentValue)
			{
				_preViewValue = newValue;

				_bar.value = _preViewValue / _maxValue;
				_barPlus.value = _currentValue / _maxValue;

				_barMinus.gameObject.SetActive(false);
			}
			else
			{
				_preViewValue = _currentValue;
				//_currentValue = newValue;

				_bar.value = _currentValue / _maxValue;
				_barPlus.value = newValue / _maxValue;

				_barMinus.gameObject.SetActive(true);
			}
		}
	}

	private void UpdateShieldBar(float absorbed, float maxAbsorption)
	{
		if (_shieldBar != null)
		{
			if (absorbed < maxAbsorption)
			{
				_healthBarTarget = (_currentValue - (maxAbsorption - absorbed)) / _maxValue;
				_bar.DOValue(_healthBarTarget, _disapearSpeed);
				_barMinus.DOValue(_healthBarTarget, _disapearSpeed);
				//_barMinusPrewiew.DOValue(_healthBarTarget, _disapearSpeed);

				ShieldActive = true;
			}

			else
			{
				ShieldActive = false;
				UpdateBar();
			}
		}
	}

	private void UpdateShieldVisual()
	{
		if (_shieldBar != null)
		{
			_shieldBar.value = _currentValue / _maxValue;
		}
	}

	private void OnShieldDeactivated()
	{
		ShieldActive = false;
		UpdateBar();
	}

	private void OnChangeBarColor(Color color)
	{
		if(_barImage != null)
			_barImage.color = color;
	}
}
