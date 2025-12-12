using Mirror;
using System.Linq;
using UnityEngine;

public class TalentSaveManager
{
    private ISaveData _saveData;
    private SaveManager _saveManager;

    public TalentSaveManager(ISaveData saveData, SaveManager saveManager)
    {
        _saveData = saveData;
        _saveManager = saveManager;
    }

	/*public void SaveTalent(HeroComponent character, int idGroup, string idTalent, bool isActive, int saveGroup)
	{
		var isTalentActive = isActive ? 1 : 0;
		var talentGroup = character.TalentManager.TalentsGroups.FirstOrDefault(o => o.ID == idGroup);
		var talent = talentGroup?.TalentsData.FirstOrDefault(o => o.Data.Name == idTalent);

		if (talentGroup == null || talent == null) return;

		var points = talentGroup.BonusAttributePoints(talent.Data.Name, !isActive);
		talent.Data.IsOpen = isActive;

		if (isActive)
		{
			_saveManager.SaveAttributePoints(points);
		}
		else
		{
			HandleDeactivation(points);
		}
		Debug.Log("SHOULD " + isActive + " TALENT " + $"{character.Data.Name}_Group{saveGroup}_{talentGroup.Name}_{talent.Data.Name}");

		_saveData.SaveInt($"{character.Data.Name}_Group{saveGroup}_{talentGroup.Name}_{talent.Data.Name}", isTalentActive);
	}*/

	public void SaveTalent(HeroComponent character, int idGroup, int row, string idTalent, bool isActive, int saveGroup)
    {
        var isTalentActive = isActive ? 1 : 0;
        //var talentGroup = character.TalentManager.TalentsGroups.FirstOrDefault(o => o.ID == idGroup);
       // var talent = talentGroup?.TalentsData.FirstOrDefault(o => o.Data.Name == idTalent);

		var talentGroup = character.TalentManager.TalentsGroups.FirstOrDefault(o => o.ID == idGroup);
		var talentRow = talentGroup.TalentRows[row];
		var talent = talentRow.Talents?.FirstOrDefault(o => o.Data.Name == idTalent);

		if (talentGroup == null || talent == null) return;

        var points = talentGroup.BonusAttributePoints(talent.Data.Name, !isActive);
        talent.Data.IsOpen = isActive;

        if (isActive)
        {
            _saveManager.SaveAttributePoints(points);
        }
        else
        {
            HandleDeactivation(points);
        }
        Debug.Log("SHOULD " + isActive + " TALENT " + $"{character.Data.Name}_Group{saveGroup}_{talentGroup.Name}_{talent.Data.Name}");

        _saveData.SaveInt($"{character.Data.Name}_Group{saveGroup}_{talentGroup.Name}_{talent.Data.Name}", isTalentActive);
    }

    private void HandleDeactivation(int points)
    {
        int remainingPoints = points;

        remainingPoints = _saveManager.ReduceFreePoints(remainingPoints);

        if (remainingPoints > 0)
        {
            _saveManager.ReduceAttributePoints(remainingPoints);
        }

        if (remainingPoints > 0)
        {
            Debug.LogWarning("Недостаточно очков для деактивации таланта!");
        }
    }
	/*public void LoadTalent(HeroComponent character, int idGroup, string idTalent, bool needActive, int saveGroup)
	{
		var talentGroup = character.TalentManager.TalentsGroups.FirstOrDefault(o => o.ID == idGroup);
		var talent = talentGroup?.TalentsData.FirstOrDefault(o => o.Data.Name == idTalent);

		if (talentGroup == null || talent == null) return;

		int isActive = _saveData.LoadInt($"{character.Data.Name}_Group{saveGroup}_{talentGroup.Name}_{talent.Data.Name}", 0);

		talent.Data.IsOpen = isActive == 1;
		talentGroup.SetActive(talent.Data, isActive == 1);

		if (needActive)
		{
			character.TalentManager.CmdSwitchTalent(idGroup, idTalent, isActive == 1);
			// talentGroup.CmdActiveTalent(talent.Data, isActive == 1);
			//talentGroup.ClientActivateTalent(talent.Data, isActive == 1);
			//CmdActiveTalent(talentGroup, talent.Data, isActive == 1);

		}
	}*/

	public void LoadTalent(HeroComponent character, int idGroup, int row, string idTalent, bool needActive, int saveGroup)
    {
        /*var talentGroup = character.TalentManager.TalentsGroups.FirstOrDefault(o => o.ID == idGroup);
        var talent = talentGroup?.TalentsData.FirstOrDefault(o => o.Data.Name == idTalent);*/

		var talentGroup = character.TalentManager.TalentsGroups.FirstOrDefault(o => o.ID == idGroup);
		var talentRow = talentGroup.TalentRows[row];
		var talent = talentRow.Talents?.FirstOrDefault(o => o.Data.Name == idTalent);

		if (talentGroup == null || talent == null) return;

        int isActive = _saveData.LoadInt($"{character.Data.Name}_Group{saveGroup}_{talentGroup.Name}_{talent.Data.Name}", 0);

        talent.Data.IsOpen = isActive == 1;
        talentGroup.SetActive(talent.Data, isActive == 1);

        if (needActive)
        {
            character.TalentManager.CmdSwitchTalent(idGroup, row, idTalent, isActive == 1);
			// talentGroup.CmdActiveTalent(talent.Data, isActive == 1);
			//talentGroup.ClientActivateTalent(talent.Data, isActive == 1);
			//CmdActiveTalent(talentGroup, talent.Data, isActive == 1);

		}
    }

	/*  [Command]
	  private void CmdActiveTalent(TalentsGroup group, TalentData data, bool isActive)
	  {
		  Debug.Log("CMD TALENT");
		  group.ActiveTalent(data, isActive);
		  group.ClientActivateTalent(data, isActive);
		  //ClientActiveTalent(group, data, isActive);
	  }

	  [ClientRpc]
	  private void ClientActiveTalent(TalentsGroup group, TalentData data, bool isActive)
	  {
		  Debug.Log("CLIENT TALENT");
		  group.ActiveTalent(data, isActive);
	  }*/

	/*public void SaveAllTalents(HeroComponent character, int saveGroup)
	{
		foreach (var talentGroup in character.TalentManager.TalentsGroups)
		{
			foreach (var talent in talentGroup.TalentsData)
			{
				_saveData.SaveInt($"{character.Data.Name}_Group{saveGroup}_{talentGroup.Name}_{talent.Data.Name}", talent.Data.IsOpen ? 1 : 0);
			}
		}
	}

	public void LoadAllTalents(HeroComponent character, int saveGroup)
	{
		foreach (var talentGroup in character.TalentManager.TalentsGroups)
		{
			foreach (var talent in talentGroup.TalentsData)
			{
				int isActive = _saveData.LoadInt($"{character.Data.Name}_Group{saveGroup}_{talentGroup.Name}_{talent.Data.Name}", 0);
				//int isActive = _saveData.LoadInt($"{character.Data.Name}_Group{saveGroup}_{talentGroup.Name}_{talent.Data.Name}", talent.Data.IsOpen ? 1 : 0);
				talent.Data.IsOpen = isActive == 1;
			}
		}
	}*/
	public void SaveAllTalents(HeroComponent character, int saveGroup)
    {
        foreach (var talentGroup in character.TalentManager.TalentsGroups)
        {
			foreach (var row in talentGroup.TalentRows)
			{
				foreach (var talent in row.Talents)
				{
					_saveData.SaveInt($"{character.Data.Name}_Group{saveGroup}_{talentGroup.Name}_{talent.Data.Name}", talent.Data.IsOpen ? 1 : 0);
				}
			}
        }
    }

    public void LoadAllTalents(HeroComponent character, int saveGroup)
    {
        foreach (var talentGroup in character.TalentManager.TalentsGroups)
        {
			foreach (var row in talentGroup.TalentRows)
			{
				foreach (var talent in row.Talents)
				{
					int isActive = _saveData.LoadInt($"{character.Data.Name}_Group{saveGroup}_{talentGroup.Name}_{talent.Data.Name}", 0);
					//int isActive = _saveData.LoadInt($"{character.Data.Name}_Group{saveGroup}_{talentGroup.Name}_{talent.Data.Name}", talent.Data.IsOpen ? 1 : 0);
					talent.Data.IsOpen = isActive == 1;
				}
			}
        }
    }
}
