using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkRoomsManager : NetworkBehaviour
{
	[SerializeField] private GameMode _gameMode;
	[SerializeField, Scene] private string _scene;
	[Header("Game Rules")]
	[SerializeField] private int _maxPlayers;
	[SerializeField] private GameRules _gameRulesPref;

	private GameRules _gameRules;

	private readonly List<NetworkRoom> _rooms = new();

	public string Scene => _scene;

	public GameMode GameMode { get => _gameMode; set => _gameMode = value; }

	protected override void OnValidate()
	{
		base.OnValidate();

		switch (_gameMode)
		{
			case GameMode.GM1vs1:
				_maxPlayers = 2;
				break;

			case GameMode.GM2vs2:
				_maxPlayers = 4;
				break;

			case GameMode.GM3vs3:
				_maxPlayers = 6;
				break;

			case GameMode.GM1vs1MaximumMode:
				_maxPlayers = 2;

				break;
		}
	}

	public IEnumerator AddPlayerJob(GameObject player)
	{
		NetworkRoom currentRoom;

		if (_rooms.Count <= 0 || _rooms[^1].IsHaveSlot == false)
		{
			currentRoom = new NetworkRoom();
			currentRoom.Init(_scene, _maxPlayers);

			_rooms.Add(currentRoom);

			_rooms[^1].SlotsEnded += OnRoomSlotsEnded;
			_rooms[^1].RoomClosed += OnRoomClosed;

			yield return StartCoroutine(_rooms[^1].LoadRoomJob());
			_gameRules = Instantiate(_gameRulesPref);
		}

		else currentRoom = _rooms[^1];

		while (!currentRoom.IsLoaded) yield return null;

		bool added = currentRoom.TryAddPlayerInRoom(player);
		if (!added) Debug.LogError($"[NetworkRoomsManager] Failed to add player {player.name} in room");
	}

	private void OnRoomSlotsEnded(NetworkRoom room)
	{
		room.GameStart(_gameRules);
	}

	private void OnRoomClosed(NetworkRoom room)
	{
		_rooms.Remove(room);
	}
}

public enum GameMode
{
	GMTest,
	GM1vs1,
	GM2vs2,
	GM3vs3,
	GMAllvsAll,
	GM1vs1MaximumMode,
	Battlegrounds,
	None
}

public enum MainGameMode
{
	Battlegrounds,
	Arena,
	None
}