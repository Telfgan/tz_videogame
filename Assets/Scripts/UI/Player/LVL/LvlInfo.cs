using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LvlInfo : MonoBehaviour
{
	[SerializeField] private Slider _lvlBar;
	[SerializeField] protected TMP_Text _LvlText;

	private Level _playerLevel;
	private int _lvlValue;
	private int _expValue;
	private int _maxExpValue;

	public void Init(Level level)
    {
		if (_playerLevel != null)
		{
			_playerLevel.EXPAdded -= OnEXPAdded;
			_playerLevel.LVLUped -= OnLVLUped;
			_playerLevel.EXPForNextLVLChanged -= OnEXPForNextLVLChanged;
		}

		_playerLevel = level;

		_lvlValue = _playerLevel.Value;
		_expValue = _playerLevel.Experience;
		_maxExpValue = _playerLevel.ExperienceForNextLVL;

		UpdateInfo();

		_playerLevel.EXPAdded += OnEXPAdded;
		_playerLevel.LVLUped += OnLVLUped;
		_playerLevel.EXPForNextLVLChanged += OnEXPForNextLVLChanged;
	}

    private void OnEXPAdded(int obj)
    {
		_expValue = obj;
		UpdateInfo();
	}
	
    private void OnLVLUped(int obj)
    {
		_lvlValue = obj;
		_LvlText.transform.DOKill();
		_LvlText.transform.localScale = new Vector3(2,2,2);
		_LvlText.transform.DOScale(1, 1);

		UpdateInfo();
	}
	
    private void OnEXPForNextLVLChanged(int obj)
    {
		_maxExpValue = obj;
		UpdateInfo();
	}

    private void UpdateInfo()
    {
		_lvlBar.value = (float)_expValue / _maxExpValue;
		_LvlText.text = _lvlValue.ToString();
	}
}
