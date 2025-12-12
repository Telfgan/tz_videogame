using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class CarryGunAura : NetworkBehaviour
{
    [SerializeField] private SpawnComponent _spawnComponent;

    private List<MinionComponent> _swarm = new();
    private List<ScraderSpawn> _activeScraderSpawns = new();

    public void AddToSwarm(MinionComponent minion)
    {
        if (!_swarm.Contains(minion)) _swarm.Add(minion);
    }

    public void UnsubscribeScraderSpawn(ScraderSpawn scraderSpawn)
    {
        if (_activeScraderSpawns.Contains(scraderSpawn))
        {
            _activeScraderSpawns.Remove(scraderSpawn);
        }
    }
}
