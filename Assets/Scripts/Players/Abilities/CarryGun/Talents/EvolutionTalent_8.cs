using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolutionTalent_8 : Talent
{
    [SerializeField] private DoubleCheliceraStrike doubleCheliceraStrike;
    [SerializeField] private SkillManager skillManager;

    public override void Enter()
    {
        skillManager.ActivateSkill(doubleCheliceraStrike);
    }

    public override void Exit()
    {
        skillManager.DeactivateSkill(doubleCheliceraStrike);
    }
}
