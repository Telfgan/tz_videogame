using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjaPoisonTalent_1 : Talent
{
    [SerializeField] private CreeperStrike creeperStrike;
    [SerializeField] private FocusingOnReflexes focusingOnReflexes;
    [SerializeField] private SkillManager skillManager;

    public override void Enter()
    {
        creeperStrike.GeneticsTalentOne(true);
        skillManager.ActivateSkill(focusingOnReflexes);
    }

    public override void Exit()
    {
        creeperStrike.GeneticsTalentOne(false);
        skillManager.DeactivateSkill(focusingOnReflexes);
    }
}
