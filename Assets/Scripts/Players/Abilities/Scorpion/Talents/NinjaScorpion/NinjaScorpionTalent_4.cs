using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjaScorpionTalent_4 : Talent
{
    [SerializeField] private Kick_Scorpion kick_Scorpion;

    public override void Enter()
    {
        kick_Scorpion.Kick_ScorpionRowBonusTalent(true);
    }

    public override void Exit()
    {
        kick_Scorpion.Kick_ScorpionRowBonusTalent(false);
    }
}
