using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MPNetworkManager : NetworkManager
{
    public static MPNetworkManager Instance;

    private int _userID = -37;
    public int UserID { get => _userID; set { if(_userID == -37) _userID = value; } }

    public override void OnStartClient()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
}
