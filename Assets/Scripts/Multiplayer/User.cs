using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class User : NetworkBehaviour
{
    public static User Instance;

    private const string ID = "id";
    private const string BOTTLE = "bottle";

    private int _id = -37;
    private int _bottle;

    public void SetID(int id)
    {
        if (_id < 0)
        {
            _id = id;

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
            {ID, _id.ToString()},
            };

            NetworkHTTP.Instance.Post(URLLibrary.GetBottle, data, Success);
        }
    }

    public override void OnStartClient()
    {
        if (isLocalPlayer && isOwned)
        {
            if (Instance == null)
            {
                Instance = this;
                _id = MPNetworkManager.Instance.UserID;
                InitializeManagers();
            }

            else if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
        }
    }

    private void Success(string data)
    {
        if (int.TryParse(data, out int bottle))
        {
            Debug.Log(bottle);
            _bottle = bottle;
        }
        else
        {
            //Error?.Invoke(data);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            BottleUserManager.Instance?.Dispose();
            LevelCharacterManager.Instance?.Dispose();
            Instance = null;
        }
    }

    private void InitializeManagers()
    {
        BottleUserManager.Instance?.BottleInitialize();
        LevelCharacterManager.Instance?.LevelInitialize();
    }
}

public class BottleUserManager
{
    private static BottleUserManager _instance;
    public static BottleUserManager Instance => _instance ??= new BottleUserManager();

    private const int MaxBottles = 99;

    private int _currentBottles = 0;
    private float _currentBottleVolume = 0f;
    private string _currentUser;
    private string mainMenuSceneName = "MainMenu";

    public void BottleInitialize()
    {
        _instance = this;
        LoadBottleData();
        LogBottleInfoOnClient();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void Dispose()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == mainMenuSceneName)
        {
            LogBottleInfoOnClient();
        }
    }

    public void SetUser(string user)
    {
        _currentUser = user;
        Debug.Log($"Current user set to: {_currentUser}");
        LoadBottleData();
    }

    public void AddBottleVolume(float amount)
    {
        _currentBottleVolume += amount;

        if (_currentBottleVolume >= 1f)
        {
            _currentBottles = Mathf.Min(_currentBottles + 1, MaxBottles);
            _currentBottleVolume = 0f;
        }

        SaveBottleData();
    }

    public bool TryUseBottle()
    {
        if (_currentBottles > 0)
        {
            _currentBottles--;
            SaveBottleData();
            return true;
        }
        return false;
    }

    public int GetCurrentBottles() => _currentBottles;
    public float GetCurrentBottleVolume() => _currentBottleVolume;

    private void SaveBottleData()
    {
        if (string.IsNullOrEmpty(_currentUser))
        {
            Debug.LogWarning("Cannot save bottle data: User not set.");
            return;
        }

        PlayerPrefs.SetInt(_currentUser + "_Bottles", _currentBottles);
        PlayerPrefs.SetFloat(_currentUser + "_BottleVolume", _currentBottleVolume);
        PlayerPrefs.Save();

        Debug.Log($"Bottle data saved for {_currentUser}. Bottles: {_currentBottles}, Volume: {_currentBottleVolume}");
    }

    private void LoadBottleData()
    {
        if (string.IsNullOrEmpty(_currentUser))
        {
            Debug.LogWarning("Cannot load bottle data: User not set.");
            return;
        }

        _currentBottles = PlayerPrefs.GetInt(_currentUser + "_Bottles", 0);
        _currentBottleVolume = PlayerPrefs.GetFloat(_currentUser + "_BottleVolume", 0f);

        Debug.Log($"Bottle data loaded for {_currentUser}. Bottles: {_currentBottles}, Volume: {_currentBottleVolume}");
    }

    public void LogBottleInfoOnClient()
    {
        Debug.Log($"Number of bottles: {_currentBottles}");
        Debug.Log($"The volume of the current bottle: {_currentBottleVolume * 100}%");
    }
}

public class LevelCharacterManager
{
    private static LevelCharacterManager _instance;
    public static LevelCharacterManager Instance => _instance ??= new LevelCharacterManager();

    private HeroComponent _character;
    private int _currentSaveGroup = 0;

    private int _currentLevel = 1;
    private int _currentExperience = 0;
    private int _experienceForNextLevel = 100;

    private const int _maxLevel = 9;
    private const int _maxExperienceAtMaxLevel = 800;

    public int MaxLevel => _maxLevel;

    public void LevelInitialize()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public void Dispose()
    {

    }

    public void SetHero(HeroComponent hero)
    {
        _character = hero;
        LoadLevelData();
        DisplayCurrentHeroLevelInfo();
        //ResetLevelData();
    }

    public void SetSaveIndex(int index)
    {
        _currentSaveGroup = index;
        LoadLevelData();
    }

    public void AddExperience(int experience)
    {
        if (_character == null || _currentLevel >= _maxLevel) return;

        _currentExperience += experience;
        CheckForLevelUp();
        SaveLevelData();
    }

    private void CheckForLevelUp()
    {
        while (_currentExperience >= _experienceForNextLevel && _currentLevel < _maxLevel)
        {
            _currentExperience -= _experienceForNextLevel;
            _currentLevel++;
            _experienceForNextLevel = CalculateExperienceForNextLevel();

            if (_currentLevel == _maxLevel)
            {
                _currentExperience = _maxExperienceAtMaxLevel;
                _experienceForNextLevel = _maxExperienceAtMaxLevel;
                break;
            }
        }
    }

    private int CalculateExperienceForNextLevel()
    {
        return _currentLevel * 100;
    }

    public int GetCurrentLevel() => _currentLevel;
    public int GetCurrentExperience() => _currentExperience;
    public int GetExperienceForNextLevel() => _experienceForNextLevel;

    private void SaveLevelData()
    {
        PlayerPrefs.SetInt(_character.Data.Name + "_Group" + _currentSaveGroup + "_Level", _currentLevel);
        PlayerPrefs.SetInt(_character.Data.Name + "_Group" + _currentSaveGroup + "_Experience", _currentExperience);
        PlayerPrefs.SetInt(_character.Data.Name + "_Group" + _currentSaveGroup + "_ExperienceForNextLevel", _experienceForNextLevel);
        PlayerPrefs.Save();
    }

    private void LoadLevelData()
    {
        _currentLevel = PlayerPrefs.GetInt(_character.Data.Name + "_Group" + _currentSaveGroup + "_Level", 1);
        _currentExperience = PlayerPrefs.GetInt(_character.Data.Name + "_Group" + _currentSaveGroup + "_Experience", 0);
        _experienceForNextLevel = PlayerPrefs.GetInt(_character.Data.Name + "_Group" + _currentSaveGroup + "_ExperienceForNextLevel", 100);

        if (_currentLevel >= _maxLevel)
        {
            _currentLevel = _maxLevel;
            _currentExperience = _maxExperienceAtMaxLevel;
            _experienceForNextLevel = _maxExperienceAtMaxLevel;
        }
    }

    public void ResetLevelData()
    {
        if (_character == null) return;

        PlayerPrefs.DeleteKey(_character.Data.Name + "_Group" + _currentSaveGroup + "_Level");
        PlayerPrefs.DeleteKey(_character.Data.Name + "_Group" + _currentSaveGroup + "_Experience");
        PlayerPrefs.DeleteKey(_character.Data.Name + "_Group" + _currentSaveGroup + "_ExperienceForNextLevel");
        PlayerPrefs.Save();

        _currentLevel = 1;
        _currentExperience = 0;
        _experienceForNextLevel = 100;
    }

    public void DisplayCurrentHeroLevelInfo()
    {
        if (_character == null) return;
        //Debug.Log($"Character: {_character.Data.Name} | Level: {_currentLevel} | Experience: {_currentExperience}/{_experienceForNextLevel}");
    }
}