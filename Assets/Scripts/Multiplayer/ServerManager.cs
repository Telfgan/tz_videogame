using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerManager : NetworkBehaviour
{
    [SerializeField] private List<NetworkRoomsManager> _managers;
    [SerializeField] private List<HeroComponent> _heroList;

    [SerializeField] private GameObject _menuEnv;

    private static ServerManager _instance;
    private int _currentHeroIndex = 0;
    private GameMode _currentGameMode = GameMode.GM1vs1;

    public GameMode CurrentGameMode => _currentGameMode;
    public static ServerManager Instance => _instance;
    public List<HeroComponent> HeroList => _heroList;

    public void Awake()
    {
        if (_instance != null)
        {
            Destroy(this);
        }
        else
        {
            _instance = this;
        }
        _menuEnv.gameObject.SetActive(true);
    }

    public void StartClient()
    {
        _menuEnv.gameObject.SetActive(false);
        AddPlayer(User.Instance.gameObject, _currentHeroIndex, _currentGameMode);
    }

    public void EnableMenu()
    {
        _menuEnv.gameObject.SetActive(true);
    }

    [Command(requiresAuthority = false)]
    private void AddPlayer(GameObject user, int characterIndex, GameMode gameMode)
    {
        StartCoroutine(AddPlayerInRoomJob(user, characterIndex, gameMode));
    }

    private int GetManagerIndex(GameMode mode)
    {
        for (int i = 0; i < _managers.Count; i++)
        {
            if (_managers[i].GameMode == mode)
                return i;
        }
        Debug.LogError("manager not found");
        return -37;
    }

    private IEnumerator AddPlayerInRoomJob(GameObject user, int characterIndex, GameMode gameMode)
    {
        GameObject player = Instantiate(_heroList[characterIndex].gameObject);
        NetworkServer.Spawn(player, user);

        int index = GetManagerIndex(gameMode);

        yield return StartCoroutine(_managers[index].AddPlayerJob(player));

        user.GetComponent<User>().connectionToClient.Send(new SceneMessage { sceneName = _managers[index].Scene, sceneOperation = SceneOperation.LoadAdditive });
        //SceneManager.MoveGameObjectToScene(user, SceneManager.GetSceneAt(SceneManager.sceneCount - 1));
    }

    public void SetPlayer(HeroComponent hero)
    {
        _currentHeroIndex = _heroList.IndexOf(hero);

        if (LevelCharacterManager.Instance != null)
        {
            LevelCharacterManager.Instance.SetHero(hero);
        }
    }
    public void SetMode(GameMode mode)
    {
        _currentGameMode = mode;
    }
}