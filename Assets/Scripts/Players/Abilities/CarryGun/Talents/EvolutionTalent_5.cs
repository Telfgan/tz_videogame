using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolutionTalent_5 : Talent
{
    [SerializeField] private DeafeningScream deafeningScream;
    [SerializeField] private SkillManager abilities;
    [SerializeField] private ClawStrike clawStrike;

    public override void Enter()
    {
        clawStrike.BleedingClawStrike(true);
        abilities.ActivateSkill(deafeningScream);
    }

    public override void Exit()
    {
        clawStrike.BleedingClawStrike(false);
        abilities.DeactivateSkill(deafeningScream);
    }
}
