using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainLightningTalent : Talent
{
    [SerializeField] private ChainLightning _skill;
    [SerializeField] private SkillManager _skillManagerGangdollarff;
    [SerializeField] private Character _airElementPref;

    public override void Enter()
    {
        if (character.SpawnComponent.Units.Count > 0 && character.SpawnComponent.Units[0].TryGetComponent(out AirElement air))
        {
            character.SpawnComponent.Units[0].Abilities.ActivateSkill(character.SpawnComponent.Units[0].GetComponent<ChainLightning>());
        }
        _airElementPref.Abilities.ActivateSkill(_airElementPref.GetComponent<ChainLightning>());
        _skillManagerGangdollarff.ActivateSkill(_skill);
    }

    public override void Exit()
    {
        if (character.SpawnComponent.Units.Count > 0 && character.SpawnComponent.Units[0].TryGetComponent(out AirElement air))
        {
            character.SpawnComponent.Units[0].Abilities.DeactivateSkill(character.SpawnComponent.Units[0].GetComponent<ChainLightning>());
        }
        _airElementPref.Abilities.DeactivateSkill(_airElementPref.GetComponent<ChainLightning>());
        _skillManagerGangdollarff.DeactivateSkill(_skill);
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
