using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TalentData
{
	[SerializeField] private List<string> _descriptionsForInfoPanel;
	[SerializeField] private List<TalentStateInfo> _stateInfos = new();

	private string _name;
	public bool IsOpen;

	public string Description = string.Empty;
	public Sprite Icon;
	public int Group, Row = 0;
	public string Name
	{
		get { return _name; }
		set
		{
			//Debug.Log(value);
			_name = value;
		}
	}

	public List<string> DescriptionsForInfoPanel { get => _descriptionsForInfoPanel; }
	public List<TalentStateInfo> StateInfos => _stateInfos;

	public TalentData(string name, bool isOpen)
	{
		// Name = name;
		IsOpen = isOpen;
	}
}
