using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct CharacterMessage : NetworkMessage
{
    public int Index;
    public GameMode Mode;
}

public class MultiplayerManager : NetworkManager
{
    [SerializeField] private List<NetworkRoomsManager> _managers;
    [SerializeField] private List<HeroComponent> _heroList;
    [SerializeField] private List<GameMode> _activeCountModes;
    [SerializeField] private List<MainGameMode> _activeMainModes;
    
    private int _clientCount;
    private int _currentHeroIndex;
    private GameMode _currentGameMod = GameMode.GMTest;
    
    private static MultiplayerManager _instance;
    public static MultiplayerManager Instance => _instance;
    
    public List<GameMode> ActiveCountModes => _activeCountModes;
    public List<MainGameMode> ActiveMainModes => _activeMainModes;

    public List<HeroComponent> HeroList { get => _heroList; set => _heroList = value; }

    public override void Awake()
    {
        base.Awake();
        if (_instance != null)
        {
            Destroy(this);
        }
        else
        {
            _instance = this;
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        NetworkServer.RegisterHandler<CharacterMessage>(OnCreateCharacter);
    }

    //public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    //{
    //    StartCoroutine(OnServerAddPlayerDelayed(conn));
    //}

    private void OnCreateCharacter(NetworkConnectionToClient conn, CharacterMessage message)
    {
        StartCoroutine(OnServerAddPlayerDelayed(conn, message));

        //GameObject gameobject = Instantiate(_heroList[message.Index]).gameObject;

        //NetworkServer.AddPlayerForConnection(conn, gameobject);
    }

    IEnumerator OnServerAddPlayerDelayed(NetworkConnectionToClient conn, CharacterMessage message)
    {
        GameObject player = Instantiate(_heroList[message.Index]).gameObject;
        NetworkServer.AddPlayerForConnection(conn, player);

        int index = GetManagerIndex(message.Mode);

        yield return StartCoroutine(_managers[index].AddPlayerJob(player));

        conn.Send(new SceneMessage { sceneName = _managers[index].Scene, sceneOperation = SceneOperation.LoadAdditive });

        _clientCount++;
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


    //public override void OnStopServer()
    //{
    //    NetworkServer.SendToAll(new SceneMessage { sceneName = _room, sceneOperation = SceneOperation.UnloadAdditive });
    //    _clientCount = 0;
    //}

    public override void OnStopClient()
    {
        if (mode == NetworkManagerMode.Offline)
            StartCoroutine(ClientUnloadSubScenes());
    }

    IEnumerator ClientUnloadSubScenes()
    {
        for (int index = 0; index < SceneManager.sceneCount; index++)
            if (SceneManager.GetSceneAt(index) != SceneManager.GetActiveScene())
                yield return SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(index));
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        CharacterMessage characterMessage = new CharacterMessage
        {
            Index = _currentHeroIndex,
            Mode = _currentGameMod,
        };
        NetworkClient.Send(characterMessage);
    }

    public void SetPlayer(int heroIndex)
    {
        _currentHeroIndex = heroIndex;
    }
    public void SetMode(GameMode mode)
    {
        _currentGameMod = mode;
    }
}
