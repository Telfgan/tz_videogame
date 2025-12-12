using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectButton : MonoBehaviour
{
    [SerializeField] private MultiplayerManager _manager;
    [SerializeField] private GameMode _mode;
    public void Click()
    {
        _manager.SetMode(_mode);
        _manager.StartClient();
    }
}
