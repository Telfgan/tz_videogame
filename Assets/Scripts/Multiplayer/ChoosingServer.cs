using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChoosingServer : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown _server;
    [SerializeField] private NetworkManager _networkManager;

    private void Awake()
    {
        _server.onValueChanged.AddListener(SetServer);
        SetServer(0);
    }


    private void SetServer(int i)
    {
        switch (i)
        {
            case 0:
                _networkManager.networkAddress = URLLibrary.LocalHost;
                break;
            case 1:
                _networkManager.networkAddress = URLLibrary.MainServer;
                break;
            default:
                break;
        }
    }
}
