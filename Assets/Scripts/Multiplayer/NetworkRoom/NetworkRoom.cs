using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class NetworkRoom
{
    private string _scene;
    private int _maxNumPlayers;
    private List<GameObject> _players;
    private Scene _currentRoom;
    private bool _isLoaded;
    private bool _isGameStarted = false;

    public bool IsHaveSlot
    {
        get
        {
            if (_isGameStarted == false)
                _players.RemoveAll(player => player == null);

            return _maxNumPlayers > _players.Count;
        }
    }
    public int NumOfFreeSlots
    {
        get
        {
            if (_isGameStarted == false)
                _players.RemoveAll(player => player == null);

            return _maxNumPlayers - _players.Count;
        }
    }
    public List<GameObject> Players => _players;
    public Scene Scene => _currentRoom;
    public string SceneName => _scene;
    public bool IsLoaded => _isLoaded;

    public event UnityAction<NetworkRoom> SlotsEnded;
    public event UnityAction<NetworkRoom> RoomClosed;

    public void Init(string scene, int maxNumPlayers)
    {
        _scene = scene;
        _maxNumPlayers = maxNumPlayers;
        _players = new List<GameObject>();
    }

    [Server]
    public IEnumerator LoadRoomJob(LocalPhysicsMode physicsMode = LocalPhysicsMode.Physics3D)
    {
        if (!_isLoaded)
        {
            yield return SceneManager.LoadSceneAsync(_scene, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = physicsMode });
            _currentRoom = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            _isLoaded = true;
        }
    }

    [Server]
    public IEnumerator UnloadRoomJob()
    {
        if (_isLoaded)
        {
            yield return SceneManager.UnloadSceneAsync(_currentRoom);
            _isLoaded = false;
            RoomClosed?.Invoke(this);
        }
    }

    public bool TryAddPlayerInRoom(GameObject player)
    {
        if (IsHaveSlot && _isLoaded)
        {
            SceneManager.MoveGameObjectToScene(player, _currentRoom);
            _players.Add(player);

            UserNetworkSettings playerSettings = player.GetComponent<UserNetworkSettings>();
            playerSettings.MyRoom = Scene;

            if (!IsHaveSlot)
            {
                SlotsEnded?.Invoke(this);
                _isGameStarted = true;
            }
            return true;
        }
        else
        {
            Debug.LogError($"Room loaded status - {_isLoaded}\nFree slots - {NumOfFreeSlots}");
            return false;
        }
    }

    public void GameStart(GameRules item)
    {
        if (_isLoaded)
        {
            SceneManager.MoveGameObjectToScene(item.gameObject, _currentRoom);
            NetworkServer.Spawn(item.gameObject);

            item.Init(this);
            item.IsStarted = true;
            item.GameStartServer(item.SpawnPoints);
        }
        else
        {
            Debug.LogError($"Room loaded status - {_isLoaded}");
        }
    }

    public void AddItem(GameObject item)
    {
        if (_isLoaded)
        {
            SceneManager.MoveGameObjectToScene(item, _currentRoom);
        }
        else
        {
            Debug.LogError($"Room loaded status - {_isLoaded}");
        }
    }
}