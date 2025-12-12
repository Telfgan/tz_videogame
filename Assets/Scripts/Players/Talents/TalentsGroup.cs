using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class TalentsGroup
{
	[SerializeField] private int _id;
	[SerializeField] private string _name;
	[SerializeField] private List<Talent> _talentGroup;
	[SerializeField] private List<TalentRow> _talentRow;

	public int ID => _id;
	public string Name => _name;
	//public List<Talent> TalentsData => _talentGroup;
	public List<TalentRow> TalentRows => _talentRow;

	public int BonusAttributePoints(string talentName, bool isDecrease)
	{
		var bonus = 1;
		var rowLength = 3;

		var talentIndex = _talentGroup.FindIndex(talent => talent.Data.Name == talentName);
		if (talentIndex == -1)
		{
			return 0;
		}

		var row = talentIndex / rowLength;

		var activeCount = 0;
		for (var i = row * rowLength; i < (row + 1) * rowLength && i < _talentGroup.Count; i++)
		{
			if (_talentGroup[i].Data.IsOpen)
			{
				activeCount++;
			}
		}

		activeCount = isDecrease ? activeCount - 1 : activeCount;
		bonus += row switch
		{
			0 => activeCount == 0 ? 0 : activeCount == 1 ? 1 : activeCount == 2 ? 2 : 0,
			1 => activeCount == 0 ? 0 : activeCount == 1 ? 1 : activeCount == 2 ? 1 : 0,
			2 => activeCount == 0 ? 0 : activeCount == 1 ? 0 : activeCount == 2 ? 1 : 0,
			_ => 0
		};
		return bonus;
	}

	public void SetActive(TalentData data, bool isActive)
	{
		var talent = _talentGroup.FirstOrDefault(a => a.Data == data);
		if (talent == null) return;

		talent.SetActive(isActive);
	}

	/*  [Command]
	  public void CmdActiveTalent(TalentData data, bool isActive)
	  {
		//  Debug.Log("CMD TALENT");
		  ActiveTalent(data, isActive);
		  //ClientActivateTalent(data, isActive);
	  }

	  [ClientRpc]
	  public void ClientActivateTalent(TalentData data, bool isActive)
	  {
		  //Debug.Log("CLIENT TALENT");
		  ActiveTalent(data, isActive);
	  }

	  public void ActiveTalent(TalentData data, bool isActive)
	  {
		  Debug.Log("Talent " + isActive+  " on " + data.Name + " TEEEEEEEEST");
		  var talent = TalentsData.FirstOrDefault(a => a.Data == data);
		  if(talent == null) return;

		  if (isActive)
		  {
		  //	Debug.Log("Talent activated " + talent.GetType().Name);
			  talent.Enter();   
		  }
		  else
		  {
			  talent.Exit();
		  }
	  }*/
}
