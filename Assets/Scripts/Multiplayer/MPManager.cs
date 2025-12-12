using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MPManager : NetworkManager
{
    [SerializeField] private List<NetworkRoomsManager> _managers;
    [Scene] private string _room;
    private int _roomsNum = 0;
    private bool _subscenesLoaded;
    private readonly List<Scene> _rooms = new List<Scene>();
    private int _clientCount;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        StartCoroutine(OnServerAddPlayerDelayed(conn));
    }

    IEnumerator OnServerAddPlayerDelayed(NetworkConnectionToClient conn)
    {
        GameObject player = Instantiate(playerPrefab);
        NetworkServer.AddPlayerForConnection(conn, player);

        yield return StartCoroutine(_managers[0].AddPlayerJob(player));
        
        conn.Send(new SceneMessage { sceneName = _managers[0].Scene, sceneOperation = SceneOperation.LoadAdditive });

        yield return new WaitForEndOfFrame();

        //base.OnServerAddPlayer(conn);

        UserNetworkSettings playerScore = conn.identity.GetComponent<UserNetworkSettings>();

        _clientCount++;
    }

    public override void OnStopServer()
    {
        NetworkServer.SendToAll(new SceneMessage { sceneName = _room, sceneOperation = SceneOperation.UnloadAdditive });
        _clientCount = 0;
    }

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
}
