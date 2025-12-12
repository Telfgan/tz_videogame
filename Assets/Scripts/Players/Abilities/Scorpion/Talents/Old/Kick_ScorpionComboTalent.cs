using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kick_ScorpionComboTalent : Talent
{
    [SerializeField] private Kick_Scorpion kick_Scorpion;

    public override void Enter()
    {
        kick_Scorpion.Kick_ScorpionComboTalent(true);
    }

    public override void Exit()
    {
        kick_Scorpion.Kick_ScorpionComboTalent(false);
    }
}
