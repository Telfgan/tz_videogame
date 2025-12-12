using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kick_ScorpionTalent : Talent
{
    [SerializeField] private Kick_Scorpion kick_Scorpion;

    public override void Enter()
    {
        kick_Scorpion.Kick_ScorpionRowTalent(true);
    }

    public override void Exit()
    {
        kick_Scorpion.Kick_ScorpionRowTalent(false);
    }
}
