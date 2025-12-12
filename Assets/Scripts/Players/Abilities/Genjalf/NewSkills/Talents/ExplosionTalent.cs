using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionTalent : Talent
{
    [SerializeField] private Character _fireElementPref;

    public override void Enter()
    {
        if (character.SpawnComponent.Units.Count > 0 && character.SpawnComponent.Units[0].TryGetComponent(out FireElement air))
        {
            character.SpawnComponent.Units[0].Abilities.ActivateSkill(character.SpawnComponent.Units[0].GetComponent<Explosion>());
        }
        _fireElementPref.Abilities.ActivateSkill(_fireElementPref.GetComponent<Explosion>());
    }

    public override void Exit()
    {
        if (character.SpawnComponent.Units.Count > 0 && character.SpawnComponent.Units[0].TryGetComponent(out FireElement air))
        {
            character.SpawnComponent.Units[0].Abilities.DeactivateSkill(character.SpawnComponent.Units[0].GetComponent<Explosion>());
        }
        _fireElementPref.Abilities.DeactivateSkill(_fireElementPref.GetComponent<Explosion>());
    }

    private void OnUnitAdded(Character character)
    {

        if (character.SpawnComponent.Units[0].TryGetComponent(out AirElement air))
        {

        }
        else if (character.SpawnComponent.Units[0].TryGetComponent(out EarthElement earth))
        {

        }
        else if (character.SpawnComponent.Units[0].TryGetComponent(out WaterElement water))
        {

        }
        else if (character.SpawnComponent.Units[0].TryGetComponent(out FireElement fire))
        {

        }
    }
}
