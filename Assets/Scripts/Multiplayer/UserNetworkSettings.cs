using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserNetworkSettings : NetworkBehaviour
{
    private List<HeroComponent> _allies = new List<HeroComponent>();
    private List<HeroComponent> _enemies = new List<HeroComponent>();
    public readonly SyncList<GameObject> Players = new SyncList<GameObject>();
    private Health _cachedHealth;
    private Scene _myRoom;
    [SyncVar] private Vector3 spawnPosition;

    [SyncVar] private byte _teamIndex;
    [SyncVar] private string _roomName;

    public Scene MyRoom { get { return _myRoom; } set { _myRoom = value; _roomName = value.name; } }

    public byte TeamIndex
    {
        get => _teamIndex;
        set
        {
            if (isServer)
            {
                _teamIndex = value;
                StartCoroutine(DelayedTargetUpdate());
            }
        }
    }

    private IEnumerator DelayedTargetUpdate()
    {
        yield return new WaitForSeconds(0.5f);

        if (connectionToClient != null) TargetUpdateLayers(connectionToClient);
    }

    public Health CachedHealth
    {
        get
        {
            if (_cachedHealth == null)
            {
                _cachedHealth = GetComponent<Health>();
            }
            return _cachedHealth;
        }
    }
    public string RoomName { get => _roomName; }

    public event Action<int> LayerMaskChanged;

    public void SetSpawnPosition(Vector3 position)
    {
        if (isServer)
        {
            spawnPosition = position;
            RpcUpdatePosition(position);
        }
    }

    [ClientRpc]
    private void RpcUpdatePosition(Vector3 position)
    {
        transform.position = position;
    }

    [TargetRpc]
    public void TargetUpdateLayers(NetworkConnection target)
    {
        MarkUpEnemiesOrAllies();
    }

    public void MarkUpEnemiesOrAllies()
    {
        if (Players == null || Players.Count == 0)
        {
            Debug.LogWarning("MarkUpEnemiesOrAllies: Players list is empty or not synced yet.");
            return;
        }

        _allies.Clear();
        _enemies.Clear();

        foreach (var item in Players)
        {
            if (item == null)
            {
                Debug.LogWarning("MarkUpEnemiesOrAllies: Player GameObject is null.");
                continue;
            }

            var userSettings = item.GetComponent<UserNetworkSettings>();
            var hero = item.GetComponent<HeroComponent>();
            var vision = item.GetComponent<VisionComponent>();

            if (userSettings == null || hero == null)
            {
                Debug.LogWarning($"MarkUpEnemiesOrAllies: Missing required components on {item.name}");
                continue;
            }

            if (userSettings.TeamIndex != _teamIndex)
            {
                item.layer = LayerMask.NameToLayer("Enemy");
                _enemies.Add(hero);

                if (vision != null)
                    vision.VisionRange = 0;
            }
            else
            {
                item.layer = LayerMask.NameToLayer("Allies");
                _allies.Add(hero);
            }

            LayerMaskChanged?.Invoke(item.layer);
        }

        TowerTeam towerTeam = FindObjectOfType<TowerTeam>();

        if (towerTeam == null)
        {
            Debug.LogWarning("MarkUpEnemiesOrAllies: TowerTeam not found in scene!");
            return;
        }

        if (towerTeam != null)
        {
            foreach (var tower in towerTeam.TowerTeam_1)
            {
                if (tower != null) tower.layer = _teamIndex == 1 ? LayerMask.NameToLayer("Allies") : LayerMask.NameToLayer("Enemy");
            }

            foreach (var tower in towerTeam.TowerTeam_2)
            {
                if (tower != null) tower.layer = _teamIndex == 2 ? LayerMask.NameToLayer("Allies") : LayerMask.NameToLayer("Enemy");
            }
        }
    }
}
