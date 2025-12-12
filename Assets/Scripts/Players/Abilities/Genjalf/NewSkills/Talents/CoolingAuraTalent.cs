using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoolingAuraTalent : Talent
{
    [SerializeField] private Character _waterElementPref;

    public override void Enter()
    {
        if (character.SpawnComponent.Units.Count > 0 && character.SpawnComponent.Units[0].TryGetComponent(out WaterElement air))
        {
            character.SpawnComponent.Units[0].CharacterState.CmdAddState(States.CoolingAura, 0, 0, character.SpawnComponent.Units[0].gameObject, name);
        }
        //_waterElementPref.Abilities.ActivateSkill(_waterElementPref.GetComponent<Explosion>());
        
    }

    public override void Exit()
    {
        if (character.SpawnComponent.Units.Count > 0 && character.SpawnComponent.Units[0].TryGetComponent(out WaterElement air))
        {
            character.SpawnComponent.Units[0].CharacterState.CmdRemoveState(States.CoolingAura);
        }
        //_waterElementPref.Abilities.DeactivateSkill(_waterElementPref.GetComponent<Explosion>());
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
