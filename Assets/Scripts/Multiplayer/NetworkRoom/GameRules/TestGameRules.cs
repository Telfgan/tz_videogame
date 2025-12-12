using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestGameRules : GameRules
{
    [Header("Game Settings")]
    [SerializeField] private float _lifeTime = 10f;
    [SerializeField] private bool isRemoveRoom = true;

    [Header("Team Settings")]
    [SerializeField] private int experiencePerWin = 6;
    [SerializeField] private int experiencePerLoss = 2;
    [SerializeField] private float bottleVolumePerWin = 1f / 3f;

    private TeamsPanel _teams; // need rework
    private int[] _teamDeaths = new int[3];
    

	private int _teamMaxScore = 3;
    private int _team1Score = 0;
    private int _team2Score = 0;

    public override void GameStartServer(HeroSpawnManager spawnPoints)
    {
        StartCoroutine(HandleTeamsAndSpawns(spawnPoints));
    }

    protected override void GameStartClient()
    {
        _preparationAreaManager?.PreparationAreasDisable(5f);
    }

    protected override void OnPlayerDied(Character player)
    {
        if (_regenCoroutine != null) return;
		//AddExpForAllEnemy(player);
		_regenCoroutine = StartCoroutine(RevivalPlayerCoroutine(player));
        AddScorePoint(player.NetworkSettings.TeamIndex);

        if(_team1Score >= _teamMaxScore || _team2Score >= _teamMaxScore)
        {
            if (_team1Score > _team2Score)
            {
                RpcShowWinner(1);
            }
            else
            {
                RpcShowWinner(2);
            }
            EndGame();
        }
    }

    private void AddScorePoint(int teamIndex)
    {
        switch (teamIndex)
        {
            case 2:
                _team1Score++;
                RpcSetSource(1, _team1Score);
                break;

            case 1:
                _team2Score++;
                RpcSetSource(2, _team2Score);
                break;

            default:
                Debug.LogError("Not found");
                break;
        }
    }

    private void CancelActiveSkills(Character playerSettings)
    {
        var skills = playerSettings.Abilities.Abilities;
        foreach (var skill in skills)
        {
            skill.RpcCancelActiveSkill();
            skill.RpcResetSkillState();
        }
    }

    private void CheckForRoundEnd()
    {
        if (_teamDeaths[1] == GetTeamCount(1) || _teamDeaths[2] == GetTeamCount(2))
        {
            _team2Score += _teamDeaths[1] == GetTeamCount(1) ? 1 : 0;
            _team1Score += _teamDeaths[2] == GetTeamCount(2) ? 1 : 0;

            Debug.Log($"Round Over! Team 1 Score: {_team1Score}, Team 2 Score: {_team2Score}");
            if (_team1Score >= _teamMaxScore || _team2Score >= _teamMaxScore)
            {
                EndGame();
            }
            else
            {
                //RestartRound();
            }
        }
    }

    /*
    private void EndGame()
    {
        if (!isServer) return;

        var user = User.Instance ?? FindObjectOfType<User>();

        var bottleManager = BottleUserManager.Instance;
        var levelManager = LevelCharacterManager.Instance;

        GameMode currentMode = ServerManager.Instance.CurrentGameMode;
        bool isMaxLevel = levelManager.GetCurrentLevel() >= LevelCharacterManager.Instance.MaxLevel;
        bool isVictory = _team1Score >= _teamMaxScore;

        switch (currentMode)
        {
            case GameMode.GM1vs1MaximumMode:
                if (isVictory)
                {
                    if (isMaxLevel)
                    {
                        bottleManager.AddBottleVolume(bottleVolumePerWin);
                    }
                    else
                    {
                        levelManager.AddExperience(experiencePerWin);
                        bottleManager.AddBottleVolume(bottleVolumePerWin);
                    }
                }

                break;

            default:
                if (isVictory)
                {
                    if (isMaxLevel)
                    {
                        bottleManager.AddBottleVolume(bottleVolumePerWin);
                    }
                    else
                    {
                        levelManager.AddExperience(experiencePerLoss);
                        bottleManager.AddBottleVolume(bottleVolumePerWin);
                    }
                }
                break;
        }

        RpcCloseRoomOnClients();
        StartCoroutine(CloseRoomJob());
    }
    */

    private void RestartRound()
    {
        _teamDeaths[1] = 0;
        _teamDeaths[2] = 0;

        RpcEnablePreparationAreas(5f);

        if (isServer)
        {
            List<NetworkIdentity> objectsToRemove = new List<NetworkIdentity>();

            foreach (var networkIdentity in NetworkServer.spawned.Values)
            {
                bool isPlayer = _players.Exists(player => player.gameObject == networkIdentity.gameObject);
                bool isTestGameRules = networkIdentity.GetComponent<TestGameRules>() != null;
                bool isUser = networkIdentity.GetComponent<User>() != null;

                if (networkIdentity != null && !isPlayer && !isTestGameRules && !isUser)
                {
                    objectsToRemove.Add(networkIdentity);
                }
            }
        }

        foreach (var playerSettings in _players)
        {
            ResetPlayerState(playerSettings);

            int spawnIndex = playerSettings.NetworkSettings.TeamIndex - 1;

            if (_spawnPoints != null)
            {
                RpcTeleportPlayer(playerSettings.gameObject, _spawnPoints.GetRandomPoint(spawnIndex), _spawnPoints.GetRotate(spawnIndex));
            }
        }
    }

    private void ResetPlayerState(Character player)
    {
        //player.ServerResetAll();
        /*
        var health = playerSettings.Health;
        health?.ResetValue();

        var runeComponent = playerSettings.GetComponent<RuneComponent>();
        runeComponent?.ResetValueRune();

        var characterState = playerSettings.CharacterState;
        if (characterState != null)
        {
            var statesCopy = new List<AbstractCharacterState>(characterState.CurrentStates);
            foreach (var state in statesCopy)
            {
                characterState.RemoveState(state.State);
            }
        }
        */
    }

    private int GetTeamCount(int teamIndex)
    {
        int count = 0;
        foreach (var playerSettings in _players)
        {
            if (playerSettings.NetworkSettings.TeamIndex == teamIndex)
            {
                count++;
            }
        }
        return count;
    }

    protected override void UnsubscribeFromAllEvents()
    {
        if (isServer)
        {
            List<NetworkIdentity> objectsToRemove = new List<NetworkIdentity>();

            foreach (var networkIdentity in NetworkServer.spawned.Values)
            {
                bool isPlayer = _players.Exists(player => player.gameObject == networkIdentity.gameObject);
                if (!isPlayer)
                {
                    objectsToRemove.Add(networkIdentity);
                }
            }
        }
    }


    private IEnumerator CloseRoomOnClientAndLoadMainMenu()
    {
        yield return StartCoroutine(CloseRoomJob());

        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator HandleTeamsAndSpawns(HeroSpawnManager spawnPoints)
    {
        yield return StartCoroutine(SplitTeams(spawnPoints));
        foreach (var playerSettings in _players)
        {
            int spawnIndex = playerSettings.NetworkSettings.TeamIndex - 1;
            if (_spawnPoints != null)
            {
                playerSettings.transform.SetPositionAndRotation(_spawnPoints.GetRandomPoint(spawnIndex), _spawnPoints.GetRotate(spawnIndex));
            }
        }

        yield return StartCoroutine(SavePositionsAndAssignLayers());
    }

    private IEnumerator CloseJob()
    {
        while (_lifeTime > 0)
        {
            _lifeTime -= Time.deltaTime;
            yield return null;
        }

        StartCoroutine(CloseRoomJob());
    }

    [ClientRpc] private void RpcEnablePreparationAreas(float duration) => _preparationAreaManager?.PreparationAreasDisable(duration);

    protected override void OnTowerDied(Object tower)
    {
        throw new System.NotImplementedException();
    }
}