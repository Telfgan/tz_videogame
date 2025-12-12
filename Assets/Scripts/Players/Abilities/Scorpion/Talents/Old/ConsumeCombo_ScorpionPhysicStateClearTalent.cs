using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumeCombo_ScorpionPhysicStateClearTalent : Talent
{
    [SerializeField] private ConsumeCombo_Scorpion consume;

    public override void Enter()
    {
        consume.ConsumeCombo_ScorpionPhysicStateClearTalent(true);
    }

    public override void Exit()
    {
        consume.ConsumeCombo_ScorpionPhysicStateClearTalent(false);
    }
}
