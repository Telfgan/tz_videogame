using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjaScorpionTalent_1 : Talent
{
    [SerializeField] private SkillManager skill;
    [SerializeField] private Kick_Scorpion kick_Scorpion;
    [SerializeField] private NewPunch_Scorpion newPunch_Scorpion;

    public override void Enter()
    {
        skill.ActivateSkill(kick_Scorpion);
        kick_Scorpion.Kick_ScorpionRowTalent(true);
        newPunch_Scorpion.StunningAddChance(true);
    }

    public override void Exit()
    {
        skill.DeactivateSkill(kick_Scorpion);
        kick_Scorpion.Kick_ScorpionRowTalent(false);
        newPunch_Scorpion.StunningAddChance(false);
    }
}
