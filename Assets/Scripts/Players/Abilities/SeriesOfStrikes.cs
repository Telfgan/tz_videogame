using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeriesOfStrikes : MonoBehaviour
{
	[SerializeField] private HeroComponent _playerLinks;

    private float _timer = 6;
	private float _baseTimer = 6; //time and timer between losing streak

	private bool _isInTheRow;
	private Character _curTarget;
	private Energy _energy;
	private RuneComponent _rune;
	private float _sumPhisDamage = 0;
	private float _speedMultiplier = 5;

	private bool _seriesCompliteCompoTalent;
	private bool _iceRuneTalent;

	private static List<AbilityForm> _formList = new List<AbilityForm> {AbilityForm.Physical, AbilityForm.Physical, AbilityForm.Physical, AbilityForm.Physical, AbilityForm.Physical, AbilityForm.Physical };
	private static List<AbilityForm> _formList2 = new List<AbilityForm> {AbilityForm.Physical, AbilityForm.Physical, AbilityForm.Physical, AbilityForm.Physical, AbilityForm.Magic };
	private static List<AbilityForm> _formList3 = new List<AbilityForm> {AbilityForm.Physical, AbilityForm.Magic, AbilityForm.Physical, AbilityForm.Magic, AbilityForm.Physical, AbilityForm.Magic };
	//private static List<AbilityForm> _formList3 = new List<AbilityForm> { AbilityForm.Physical, AbilityForm.Magic, AbilityForm.Physical };

	private List<Series> _seriesOfStrikes = new List<Series>()
	{
		new Series(_formList),
		new Series(_formList2),
		new Series(_formList3),
	};

	private void Start()
	{
		for (int i = 0; i < _playerLinks.Resources.Count; i++)
		{
			if (_playerLinks.Resources[i].Type == ResourceType.Energy)
			{
				_energy = (Energy)_playerLinks.Resources[i];
			}
			if (_playerLinks.Resources[i].Type == ResourceType.Rune)
			{
				_rune = (RuneComponent)_playerLinks.Resources[i];
			}
		}

	}

	private void Update()
	{
		Timer();
	}

	public float GetMultipliedSpeed()
	{
		if (!_seriesCompliteCompoTalent) return 1f;

		int maxCount = 0;
		for(int i = 0; i< _seriesOfStrikes.Count; i++)
		{
			if (_seriesOfStrikes[i].hitCount > maxCount)
			{
				maxCount = _seriesOfStrikes[i].hitCount;
			}
		}
		return _speedMultiplier * Mathf.Pow(2, maxCount);
	}

	public bool MakeHit(Character target, AbilityForm form, float usedRuneValue, float usedEnergy, float damage)
	{
		if (_iceRuneTalent) BonusRuneForDamage(damage);
		CheckCurse(target, damage);

		if (!_seriesCompliteCompoTalent) return false;
		_energy.ChangeBarColor(new Color(255, 165, 0));

		return SeriesHit(target, form, usedRuneValue, usedEnergy, damage);
	}

	public void Timer()
    {
		if (_isInTheRow)
		{
			if (!_seriesCompliteCompoTalent) return;

			_timer -= Time.deltaTime;
			if (_timer <= 0)
			{
				_energy.ChangeBarColor(Color.cyan);
				_curTarget = null;
				//Debug.Log("lose streak");
				_timer = _baseTimer;
				_isInTheRow = false;

				for(int i = 0; i < _seriesOfStrikes.Count; i++) _seriesOfStrikes[i].Reset();
			}
		}
	}

	private void LastHit(float usedRune, float usedEnergy)
	{
		//Debug.Log("Last hit");
		if (_seriesCompliteCompoTalent)
		{
			_rune.CmdAdd(usedRune * 2 + 1);
			_energy.CmdAdd(usedEnergy * 0.4f);
			_energy.ForceRegenNow();
		}
		for (int i = 0; i < _seriesOfStrikes.Count; i++) _seriesOfStrikes[i].Reset();
		
	}

	private void BonusRuneForDamage(float damage)
	{
		_sumPhisDamage += damage;
		while( _sumPhisDamage >= 100 ) 
		{
			_rune.CmdAdd(1f);
			_sumPhisDamage -= 100;
		}
	}

	public void TalentBoostMultiplier(float multiplier)
	{
		_speedMultiplier = multiplier;
	}

    #region Talent

    public void SeriesCompliteCompoTalentActive(bool value)
	{
		_seriesCompliteCompoTalent = value;
	}

	public void IceRuneTalentActive(bool value)
	{
		_iceRuneTalent = value;
	}

    #endregion

    private void CheckCurse(Character target, float damage)
	{
		if(target == null) return;
		if(target.CharacterState.CheckForState(States.Curse))
		{
			var heal = new Heal { Value = damage * 0.2f };
			_playerLinks.Health.Heal(ref heal,name);
		}
	}

	private bool SeriesHit(Character target, AbilityForm form, float usedRuneValue, float usedEnergy, float damage)
	{
		if (!_seriesCompliteCompoTalent) return false;
		for (int i = 0; i < _seriesOfStrikes.Count; i++)
		{
			if (form == _seriesOfStrikes[i].formList[_seriesOfStrikes[i].hitCount] && (target == _curTarget || target == null))
			{
				_isInTheRow = true;
				_curTarget = target;
				_seriesOfStrikes[i].usedRune += usedRuneValue;
				_seriesOfStrikes[i].usedEnergy += usedEnergy;
				_seriesOfStrikes[i].hitCount++;
				_timer = _baseTimer;

				//Debug.Log("Hit from " + _seriesOfStrikes[i] + " #" + _seriesOfStrikes[i].hitCount);

				if (_seriesOfStrikes[i].hitCount >= _seriesOfStrikes[i].formList.Count)
				{
					LastHit(_seriesOfStrikes[i].usedRune, _seriesOfStrikes[i].usedEnergy);
					return true;
				}
			}
			else
			{
				_isInTheRow = true;
				_seriesOfStrikes[i].Reset(usedRuneValue);
				_timer = _baseTimer;
				_curTarget = target;
			}
		}
		return false;
	}
}

public class Series
{
	public List<AbilityForm> formList;
	public List<float> speedBoost;
	public int hitCount = 0;
	public float usedRune = 0;
	public float usedEnergy = 0;

	public Series(List<AbilityForm> formList)
	{
		this.formList = formList;
	}

	public void Reset(float usedRuneValue)
	{
		hitCount = 1;
		usedRune = usedRuneValue;
		usedEnergy = 0;
	}

	public void Reset()
	{
		hitCount = 0;
		usedRune = 0;
		usedEnergy = 0;
	}
}

/*
 * struck or class
 * 
 * list of ability form
 * hit count
 * used rune 
 * 
 * */