using System;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;


[Serializable]
public struct TalentStateInfo
{
    public string StateName;
    [TextArea] public string Description;
}

public class TalentSystem : NetworkBehaviour
{
    [SerializeField] private List<TalentsGroup> _talents;

    private int _points = 10;
    public List<TalentsGroup> TalentsGroups => _talents;
    public List<Talent> ActiveTalents => GetActiveTalents();

    public List<Talent> GetActiveTalents()
    {
        List<Talent> activeTalents = new();
        foreach(TalentsGroup group in TalentsGroups)
        {
            foreach(TalentRow row in group.TalentRows)
            {
                foreach(Talent talent in row.Talents)
                {
                    if(talent.Data.IsOpen)
                    {
                        activeTalents.Add(talent);
                    }
                }
            }
        }

        return activeTalents;
	}
    

   // [Command]
    public void Initialize()
    {
        foreach (var talentRow in _talents.SelectMany(talentsGroup => talentsGroup.TalentRows))
        {
            foreach (var talent in talentRow.Talents)
            {
                talent.Data.Name = talent.GetType().Name;
                if (talent.Data.IsOpen)
                {
                    //Debug.Log("Talent activated on init " + talent.GetType().Name);
                    talent.Enter();
                }
                else
                {
                    //Debug.Log("Talent DEactivated on init " + talent.GetType().Name);
                    talent.Exit();
                }
            }
        }
       // Initialize2();
    }

    [ClientRpc]
	public void Initialize2()
	{
		foreach (var talentRow in _talents.SelectMany(talentsGroup => talentsGroup.TalentRows))
		{
            foreach (var talent in talentRow.Talents)
            {
                talent.Data.Name = talent.GetType().Name;
                if (talent.Data.IsOpen)
                {
                    talent.Enter();
                }
                else
                {
                    talent.Exit();
                }
            }
		}
	}

	public void AddPoints(int value)
    {
    }

   /* public void SetActive(int row, int id, bool value)
    {
        _talents[row].TalentsData[id].SetActive(value);
    }*/

	public void SetActive(int group, int row ,int id, bool value)
	{
        _talents[group].TalentRows[row].Talents[id].SetActive(value);
	}



	public void SwitchTalent(int id, int row, string talentName, bool isActive)
	{
		var talentGroup = TalentsGroups.FirstOrDefault(o => o.ID == id);
        var talentRow = talentGroup.TalentRows[row];
		var talent = talentRow.Talents?.FirstOrDefault(o => o.Data.Name == talentName);

		if (isActive)
		{
			talent.Enter();
		}
		else
		{
			talent.Exit();
		}
	}

	/*[Command]
    public void CmdSwitchTalent(int id, string talentName, bool isActive)
    {
		SwitchTalent(id, talentName, isActive);
		ClientSwitchTalent(id, talentName, isActive);
	}

    [ClientRpc]
	public void ClientSwitchTalent(int id, string talentName, bool isActive)
	{
		SwitchTalent(id, talentName, isActive);
	}*/

	[Command]
	public void CmdSwitchTalent(int id, int row, string talentName, bool isActive)
	{
		SwitchTalent(id, row, talentName, isActive);
		ClientSwitchTalent(id, row, talentName, isActive);
	}

	[ClientRpc]
	public void ClientSwitchTalent(int id, int row, string talentName, bool isActive)
	{
		SwitchTalent(id, row, talentName, isActive);
	}

	[Command]
    public void CmdEnterAll()
    {
        EnterAll();
        RpcAddAll();
    }

    [Command]
    public void CmdExitAll()
    {
        ExitAll();
        RpcRemoveAll();
    }

    [Command]
    public void CmdAdd(int id, int row)
    {
        RpcAdd(id, row);
    }

    [Command]
    public void CmdRemove(int id, int row)
    {
        RpcRemove(id, row);
    }

    [ClientRpc]
    private void RpcAdd(int id, int row)
    {

    }

    [ClientRpc]
    private void RpcRemove(int id, int row)
    {

    }

    [ClientRpc]
    private void RpcAddAll()
    {
        EnterAll();
    }

    [ClientRpc]
    private void RpcRemoveAll()
    {
        ExitAll();
    }


    public void EnterAll()
    {
        foreach (TalentsGroup talentGroup in _talents)
        {
           /* foreach (var talent in talentGroup.TalentsData)
            {
                talent.Enter();
                talent.SetActive(true);
                _points--;
            }*/
        }
    }

    public void ExitAll()
    {
        foreach (TalentsGroup talentGroup in _talents)
        {
            /*foreach (var talent in talentGroup.TalentsData)
            {
                talent.Exit();
                talent.SetActive(false);
                _points++;
            }*/
        }
    }

    public void Add(Talent talent)
    {
        talent.Enter();
        talent.SetActive(true);
    }

    public void Remove(Talent talent)
    {
        talent.Exit();
        talent.SetActive(false);
    }

    public int GetActiveTalentCount()
    {
        return ActiveTalents.Count;
    }
}