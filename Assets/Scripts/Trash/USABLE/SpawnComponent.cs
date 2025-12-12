using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnComponent : NetworkBehaviour
{
    [SerializeField] private Character _hero;
    [SerializeField] private List<Character> _characterPrefabs;

    private readonly List<Character> _units = new();

    public List<Character> Units => _units;

    public event Action<Character> UnitAdded;
    public event Action UnitRemoved;

    #region Test Methods
    [SerializeField] private List<Character> _enemyPrefabs;
    [SerializeField] private List<Character>  _allyPrefabs;

    [Command]
    public void CmdSpawnUnitEnemy(int index)
    {
        SpawnCharacter(_enemyPrefabs[index], Vector3.back + Vector3.zero, Quaternion.identity);
    }

    [Command]
    public void CmdSpawnUnitAlies(int index)
    {
        SpawnCharacter(_allyPrefabs[index], Vector3.forward + Vector3.zero, Quaternion.identity);
    }

    [Command]
    public void CmdSpawnUnit(int index)
    {
        SpawnCharacter(_units[index], Vector3.forward + Vector3.zero, Quaternion.identity);
    }

    [Command] // не стал убирать метод с мейна, хотя мой ниже такой же, но сохраняет вращение и спавнит не по индексу, а напрямую берет префаб
    public void CmdSpawnUnitInPoint(Vector3 position, int index)
    {
        if (_characterPrefabs == null || _characterPrefabs.Count == 0) return;
        SpawnUnit(index, position);
    }

    [Command]
    public void CmdSpawnEnemyPoint(Vector3 position, Quaternion rotation, int index)
    {
        SpawnCharacter(_enemyPrefabs[index], position, rotation);
    }

    [Command]
    public void CmdSpawnAliesPoint(Vector3 position, Quaternion rotation, int index)
    {
        SpawnCharacter(_allyPrefabs[index], position, rotation);
    }

    [Command]
    public void CmdSpawnUnitPoint(Vector3 position, Quaternion rotation, int index)
    {
        SpawnCharacter(_units[index], position, rotation);
    }

    [Command]
    public void CmdSpawnEnemyPoint(Vector3 position, Quaternion rotation, Character toReplace, int index, bool remove, Character parenCharacter)
    {
        var spawned = SpawnCharacterTransfer(_enemyPrefabs[index], position, rotation, remove, parenCharacter);

        if (toReplace != null && remove == true)
        {
            RemoveUnitServer(toReplace);
        }
    }

    [Command]
    public void CmdSpawnAliesPoint(Vector3 position, Quaternion rotation, Character toReplace, int index, bool remove, Character parenCharacter)
    {
        var spawned = SpawnCharacterTransfer(_allyPrefabs[index], position, rotation, remove, parenCharacter);

        if (toReplace != null && remove == true)
        {
            RemoveUnitServer(toReplace);
        }
    }

    [Command]
    public void CmdSpawnPoint(Vector3 position, Quaternion rotation, int index, Character characterParent)
    {
        var spawned = SpawnCharacterTransfer(_units[index], position, rotation, false, characterParent);
    }
    #endregion

    public void SpawnUnit(int index, Vector3 position)
    {
        if (index < 0 || index >= _characterPrefabs.Count) 
        {
            Debug.LogError($"Index {index} is out of bounds for spawning units.");
            return;
        }

        var prefab = _characterPrefabs[index];
        SpawnCharacter(prefab, position, Quaternion.identity);
    }

    private void SpawnCharacter(Character prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            Debug.LogError("Character prefab is null.");
            return;
        }

        var spawnedCharacter = Instantiate(prefab, position, rotation);
        spawnedCharacter.Initialize();
        spawnedCharacter.NetworkSettings.MyRoom = _hero.NetworkSettings.MyRoom;

        if (_hero == null || _hero.NetworkSettings == null)
        {
            Debug.LogError("Hero or NetworkSettings is null. Cannot move character to scene.");
            Destroy(spawnedCharacter.gameObject);
            return;
        }

        SceneManager.MoveGameObjectToScene(spawnedCharacter.gameObject, _hero.NetworkSettings.MyRoom);

        if (connectionToClient == null)
        {
            Debug.LogError("Connection to client is null. Cannot spawn character.");
            Destroy(spawnedCharacter.gameObject);
            return;
        }

        NetworkServer.Spawn(spawnedCharacter.gameObject, connectionToClient);

        AddUnit(spawnedCharacter);
    }

    #region Test
    private Character SpawnCharacterTransfer(Character prefab, Vector3 position, Quaternion rotation, bool remove, Character parenCharacter)
    {
        if (prefab == null) return null;

        var spawnedCharacter = Instantiate(prefab, position, rotation);
        spawnedCharacter.CharacterParent = parenCharacter;
        spawnedCharacter.Initialize();

        spawnedCharacter.NetworkSettings.MyRoom = _hero.NetworkSettings.MyRoom;

        if (_hero == null || _hero.NetworkSettings == null)
        {
            Destroy(spawnedCharacter.gameObject);
            return null;
        }

        SceneManager.MoveGameObjectToScene(spawnedCharacter.gameObject, _hero.NetworkSettings.MyRoom);

        if (connectionToClient == null)
        {
            Destroy(spawnedCharacter.gameObject);
            return null;
        }

        NetworkServer.Spawn(spawnedCharacter.gameObject, connectionToClient);
        AddUnit(spawnedCharacter);

        return spawnedCharacter;
    }

    public void RemoveUnitServer(Character character)
    {
        if (character != null && _units.Contains(character))
        {
            _units.Remove(character);

            if (character is MinionComponent minion)
            {
                minion.Destroyed -= OnUnitDestroyed;
                minion.Intercepted -= OnUnitDestroyed;
            }
            else if (character is HeroComponent hero)
            {
                hero.Died -= OnUnitDestroyed;
            }

            if (character.gameObject != null)
            {
                ClientRpcOnUnitDestroyed(character.gameObject);
                NetworkServer.Destroy(character.gameObject);
            }

            UnitRemoved?.Invoke();
        }
    }
    #endregion

    public void AddUnit(Character character)
    {
        if (character == null)
        {
            Debug.LogError("Attempted to add a null character to the units list.");
            return;
        }

        _units.Add(character);
        UnitAdded?.Invoke(character);

        Debug.Log($"Character {character.name} added. Total units: {_units.Count}");

        if (character is MinionComponent minion)
        {
            minion.Destroyed += OnUnitDestroyed;
            minion.Intercepted += OnUnitDestroyed;
        }

        else if (character is HeroComponent hero)
        {
            hero.Died += OnUnitDestroyed;
        }

        ClientRpcUnitAdded(character.gameObject);
    }

    private void OnUnitDestroyed(Character character)
    {
        if (character == null)
        {
            Debug.LogWarning("Character is null in OnUnitDestroyed.");
            return;
        }

        if (_units.Contains(character))
        {
            _units.Remove(character);
            UnitRemoved?.Invoke();

            if (character is MinionComponent minion)
            {
                minion.Destroyed -= OnUnitDestroyed;
                minion.Intercepted -= OnUnitDestroyed;
            }
            else if (character is HeroComponent hero)
            {
                hero.Died -= OnUnitDestroyed;
            }

            Debug.Log($"Character {character.name} destroyed. Total units left: {_units.Count}");

            if (character.gameObject != null)
            {
                ClientRpcOnUnitDestroyed(character.gameObject);
                NetworkServer.Destroy(character.gameObject);
            }
        }
        else
        {
            Debug.LogWarning("Character not found in units list.");
        }
    }

    [Command]
    public void CmdRemoveUnit(Character character)
    {
        if (character != null && _units.Contains(character))
        {
            _units.Remove(character);

            if (character.gameObject != null)
            {
                NetworkServer.Destroy(character.gameObject);
            }
            UnitRemoved?.Invoke();
        }
    }

    [ClientRpc]
    private void ClientRpcUnitAdded(GameObject characterObject)
    {
        if (characterObject == null)
        {
            Debug.LogWarning("Character is null in ClientRpcUnitAdded.");
            return;
        }

        var character = characterObject.GetComponent<Character>();
        if (character == null)
        {
            Debug.LogError("Character component is missing on spawned object.");
            return;
        }

        _units.Add(character);
        _units.RemoveAll(unit => unit == null);
        UnitAdded?.Invoke(character);

        Debug.Log($"Character {character.name} added on client. Total units: {_units.Count}");
    }

    [ClientRpc]
    private void ClientRpcOnUnitDestroyed(GameObject characterObject)
    {
        if (characterObject != null)
        {
            var character = characterObject.GetComponent<Character>();
            if (character != null)
            {
                _units.Remove(character);
                Debug.Log($"Character {character.name} removed on client. Total units left: {_units.Count}");
            }
            else
            {
                Debug.LogWarning("Character component is null on destroyed object.");
            }
        }

        _units.RemoveAll(unit => unit == null);
        if (UnitRemoved != null) UnitRemoved.Invoke();
    }
}
