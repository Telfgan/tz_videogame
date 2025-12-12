using System.Collections.Generic;
using UnityEngine;

public class MinionPanel : MonoBehaviour
{
    [SerializeField] private MinionIcon _minionIconPref;

    private List<MinionIcon> _minionIcons = new List<MinionIcon>();
    private Character _hero;

    public void OnCharacterSelected(Character character)
    {
        foreach (var item in character.SpawnComponent.Units)
        {
            var temp = Instantiate(_minionIconPref, transform);
            temp.Init(item);
            _minionIcons.Add(temp);
        }
        _hero = character;
        _hero.SpawnComponent.UnitAdded += OnUnitAdded;
        _hero.SpawnComponent.UnitRemoved += OnUnitRemoved;
    }

    public void OnCharacterDeselected(Character character)
    {
        if(_hero != null)
        {
            _hero.SpawnComponent.UnitAdded -= OnUnitAdded;
            _hero.SpawnComponent.UnitRemoved -= OnUnitRemoved;
            _hero = null;
        }

        foreach (var item in _minionIcons)
        {
            Destroy(item.gameObject);
        }
        _minionIcons.Clear();
    }

    private void OnUnitAdded(Character minion)
    {
        UpdatePanel();
    }

    private void OnUnitRemoved()
    {
        UpdatePanel();
    }

    private void UpdatePanel()
    {
        var temp = _hero;
        OnCharacterDeselected(null);
        OnCharacterSelected(temp);
    }
}
