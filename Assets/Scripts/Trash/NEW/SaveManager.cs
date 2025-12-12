using UnityEngine;

public interface ISaveData
{
    void SaveInt(string key, int value);
    int LoadInt(string key, int defaultValue = 0);
    void SaveString(string key, string value);
    string LoadString(string key, string defaultValue = "");
}

public class PlayerPrefsSaveData : ISaveData
{
    public void SaveInt(string key, int value)
    {
       // Debug.Log("SAVED TALENTS  " + key + value);
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }

    public int LoadInt(string key, int defaultValue = 0)
    {
		//Debug.Log("Load TALENTS  " + key + defaultValue + " loaded: "+ PlayerPrefs.GetInt(key, defaultValue));
		return PlayerPrefs.GetInt(key, defaultValue);
    }

    public void SaveString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
    }

    public string LoadString(string key, string defaultValue = "")
    {
        return PlayerPrefs.GetString(key, defaultValue);
    }
}

public class SaveManager : MonoBehaviour
{
    private static SaveManager _instance;
    public static SaveManager Instance => _instance;

    private HeroComponent _character;
    private int _currentSaveGroup = 0;
    private ISaveData _saveData;
    private AttributeSaveManager _attributeManager;
    private TalentSaveManager _talentManager;
    private AttributeModifier _attributeModifier;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            _saveData = new PlayerPrefsSaveData();
            _attributeManager = new AttributeSaveManager(_saveData);
            _talentManager = new TalentSaveManager(_saveData, _instance);
            _attributeModifier = new AttributeModifier(_attributeManager);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetHero(HeroComponent hero)
    {
        _character = hero;
        LoadHeroData();
    }

    public void SetSaveIndex(int index)
    {
        _currentSaveGroup = index;
        LoadHeroData();
    }
    
    public void SaveAttributePoints(int points)
    {
        var currentPoints = _character.Data.Attributes.FreeAttributePointsCount + points;
        _attributeManager.SaveAttributePoints(_character, _currentSaveGroup, currentPoints);
    }
    
    public int LoadAttributePoints()
    {
       return _character.Data.Attributes.FreeAttributePointsCount = _attributeManager.LoadAttributePoints(_character, _currentSaveGroup);
    }
    
    public void ChangeAttribute(int index, int points)
    {
        _attributeModifier.ChangeAttribute(_character, index, points, _currentSaveGroup);
    }
    
    public void SaveAttribute(int index)
    {
        _attributeManager.SaveAttribute(_character, index, _currentSaveGroup);
    }

    public void LoadAttribute(int attributeId)
    {
        _attributeManager.LoadAttribute(_character, attributeId, _currentSaveGroup);
    }

	/*public void SaveTalent(int idGroup, string idTalent, bool isActive)
	{
		_talentManager.SaveTalent(_character, idGroup, idTalent, isActive, _currentSaveGroup);
	}

	public void LoadTalent(int idGroup, string idTalent, bool needActivate)
	{
		_talentManager.LoadTalent(_character, idGroup, idTalent, needActivate, _currentSaveGroup);
	}*/

	public void SaveTalent(int idGroup, int row, string idTalent, bool isActive)
    {
        _talentManager.SaveTalent(_character, idGroup, row, idTalent, isActive, _currentSaveGroup);
    }

    public void LoadTalent(int idGroup, int row, string idTalent, bool needActivate)
    {
        _talentManager.LoadTalent(_character, idGroup, row, idTalent, needActivate, _currentSaveGroup);
    }

    public int ReduceFreePoints(int pointsToDeduct)
    {
        return _attributeModifier.ReduceFreePoints(_character, pointsToDeduct, _currentSaveGroup);
    }

    public void ReduceAttributePoints(int pointsToDeduct)
    {
        _attributeModifier.ReduceAttributePoints(_character, pointsToDeduct, _currentSaveGroup);
    }

    public void SaveAllData()
    {
        _attributeManager.SaveAllAttributes(_character, _currentSaveGroup);
        _talentManager.SaveAllTalents(_character, _currentSaveGroup);
    }

    public void LoadAllData()
    {
        _attributeManager.LoadAllAttributes(_character, _currentSaveGroup);
        _talentManager.LoadAllTalents(_character, _currentSaveGroup);
    }

    private void LoadHeroData()
    {
        LoadAllData();
    }
}