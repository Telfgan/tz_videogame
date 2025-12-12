using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

public class GameManager : MonoBehaviour
{
    [SerializeField] private HeroSpawnManager _heroSpawnManager;
    [SerializeField] private PreparationAreaManager preparationAreaManager;
    [SerializeField] private MonoBehaviour _sourceUI;
    [SerializeField] private TeamsPanel _teamsPanel;
    [SerializeField] private TeamSource _sourceTabl;

    private IGameSourceUI _gameSourceUI;

    private void OnValidate()
    {
        if (_sourceUI == null)
            return;

        if (_sourceUI is IGameSourceUI ui)
        {
            _gameSourceUI = ui;
        }
        else
        {
            Debug.LogError("SOURCEUI WRONG");
            _sourceUI = null;
        }
    }

    public HeroSpawnManager HeroSpawnManager { get => _heroSpawnManager; }
    public PreparationAreaManager PreparationAreaManager { get => preparationAreaManager; }
    public IGameSourceUI SourceUI { get => _gameSourceUI; }
    public TeamsPanel TeamsPanel { get => _teamsPanel; }
    public TeamSource Source { get => _sourceTabl; }
}
