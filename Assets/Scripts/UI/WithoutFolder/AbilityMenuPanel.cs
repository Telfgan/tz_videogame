using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityMenuPanel : MonoBehaviour
{
	//[SerializeField] private PlayerAbilities _ability;
	//[SerializeField] private Image[] _icos;
	//[SerializeField] private TextMeshProUGUI[] _name;
	[SerializeField] private AbilityUiIco _abilityUiIco;
	[SerializeField] private Transform _parent;

	private List<GameObject> _icos = new List<GameObject>();

	/*private void Start()
	{
		Init(_ability);
	}*/
	public void Init(/*PlayerAbilities ability */ SkillManager ability)
	{
		if(_icos.Count > 0)
		{
			for(int i = _icos.Count - 1 ; i >= 0; i--) 
			{
				Destroy(_icos[i]);
				_icos.Remove(_icos[i]);
			}
		}
		for (int i = 0; i < ability.Abilities.Count; i++) 
		{
			var ico = Instantiate(_abilityUiIco, _parent);
			ico.Init(ability.Abilities[i].Icon, ability.Abilities[i].Name + " \n" + ability.Abilities[i].Description);
			_icos.Add(ico.gameObject);
		}
	}
}
