using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AttributePanel : MonoBehaviour
{
    [SerializeField] private GameObject _content;
    [SerializeField] private AttributeItem[] _attributes;
    [SerializeField] private TextMeshProUGUI _count;
    //[SerializeField] private HeroComponent _hero;

    private int[] _modif = new int[7];
    private List<int> _changes = new List<int>();

    private int _points = 0;
    private int _bonus = 0;
    private int _bonus2 = 0;
    private int _bonus3 = 0;

    public void SwitchVisible(bool visible)
    {
        _content.SetActive(visible);
    }

	public void SwitchVisible()
	{
		_content.SetActive(!_content.activeSelf);
	}

	public void Init(HeroComponent character)
    {
        /*_attributes[0].Init(null, character.Data.Health);
        _attributes[1].Init(null, character.Data.Runes);
        _attributes[2].Init(null, character.Data.HealthInfo.DefaultPhysicsDamage);
        _attributes[3].Init(null, character.Data.HealthInfo.DefaultMagicDamage);
        _attributes[4].Init(null, character.Data.HealthInfo.EvadeMeleeDamage);
        _attributes[5].Init(null, character.Data.HealthInfo.EvadeRangeDamage);
        _attributes[6].Init(null, character.Data.HealthInfo.EvadeMagicDamage);
*/
        for(int i = 0; i < _attributes.Length; i++) 
        {
            _modif[i] = 0;
            int id = i;
            _attributes[i].Plus.onClick.AddListener(() => Add(id));
            _attributes[i].Minus.onClick.AddListener(() => Remove(id));
        }
    }

    private void OnValueChange(float value, int id)
    {
        if (value > 0 && _points > 0)//if we adding value and if we have some free points, we can add attribute value
        {
           // _modif[id] =+ value;
            _points--;
            _attributes[id].Add();
        }
        if(value < 0 && _modif[id] > 0) //if we removing value and if value is bigger than default then we removing value and adding points
		{
		//	_modif[id] =+ value;
            _points++;
			_attributes[id].Remove();
		}
        
        if(_points <=0)
        {
            for(int i = 0; i < _attributes.Length ; i++) 
            {
                _attributes[i].mat = Instantiate(_attributes[i].Ico.material);
				_attributes[i].Ico.material = _attributes[i].mat;
				_attributes[i].mat.SetFloat("_GrayscaleAmount", 1);
			}
        }
        else
        {
			for (int i = 0; i < _attributes.Length; i++)
			{
				_attributes[i].mat = Instantiate(_attributes[i].Ico.material);
				_attributes[i].Ico.material = _attributes[i].mat;
				_attributes[i].mat.SetFloat("_GrayscaleAmount", 0);
			}
		}
    }

    private void Add(int id)
    {
        if(_points > 0)
        {
			_modif[id]++;
			_points--;
			_attributes[id].Add();
            _changes.Add(id);
            _count.text = _points.ToString();
		}
        if(_points <=0)
        {
			for (int i = 0; i < _attributes.Length; i++)
			{
                _attributes[i].SetGreyScale(1);
			}
		}
    }    

    private void Remove(int id)
    {
		if (_modif[id] > 0)
		{
			_modif[id]--;
			_points++;
			_attributes[id].Remove();
            _changes.Remove(id);
			_count.text = _points.ToString();
		}
		if (_points > 0)
		{
			for (int i = 0; i < _attributes.Length; i++)
			{
				_attributes[i].SetGreyScale(0);
			}
		}
	}
	private void Remove2(int id)
	{
		if (_modif[id] > 0)
		{
			_modif[id]--;
			//_points++;
			_attributes[id].Remove();
			_changes.Remove(id);
			_count.text = _points.ToString();
		}
		if (_points > 0)
		{
			for (int i = 0; i < _attributes.Length; i++)
			{
				_attributes[i].SetGreyScale(0);
			}
		}
	}
	public void AddPoints(int value)
    {
        _points += value;
		_count.text = _points.ToString();
	}

    public bool RemovePoints(int value) 
    {
        if (_points >= value)
        {
            _points -= value;
			_count.text = _points.ToString();
			return true;
        }
        else if(_changes.Count > 0)
        {
            for (int i = 0; i < value - _points; i++)
            {
                Remove2(_changes[0]);
               // _changes.RemoveAt(0);
                _count.text = _points.ToString();
            }
			return true;
        }
        return false;
    }

    public void SetBonus(int bonus, int bonus2, int bonus3)
    {
        if(_bonus > bonus) //first row bonus
        {
            RemovePoints(_bonus - bonus);
            _bonus = bonus;
        }
        else
        {
            AddPoints(bonus - _bonus);
            _bonus = bonus;
        }

		if (_bonus2 > bonus2) // second row bonus
		{
			RemovePoints(_bonus2 - bonus2);
			_bonus2 = bonus2;
		}
		else
		{
			AddPoints(bonus2 - _bonus2);
			_bonus2 = bonus2;
		}

		if (_bonus3 > bonus3) // third row bonus
		{
			RemovePoints(_bonus3 - bonus3);
			_bonus3 = bonus3;
		}
		else
		{
			AddPoints(bonus3 - _bonus3);
			_bonus3 = bonus3;
		}
	}
}
