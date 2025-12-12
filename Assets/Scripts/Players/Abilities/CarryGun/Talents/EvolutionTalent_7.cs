using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolutionTalent_7 : Talent
{
    [SerializeField] private CheliceraStrike cheliceraStrike;
    [SerializeField] private JumpBack jumpBack;
    [SerializeField] private SkillManager skillManager;

    public override void Enter()
    {
        skillManager.ActivateSkill(jumpBack);
        cheliceraStrike.ChanceCritDamageIncrease(true);
    }

    public override void Exit()
    {
        skillManager.DeactivateSkill(jumpBack);
        cheliceraStrike.ChanceCritDamageIncrease(false);
    }
}
