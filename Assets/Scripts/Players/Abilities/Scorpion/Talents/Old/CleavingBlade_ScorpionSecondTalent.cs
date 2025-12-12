using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleavingBlade_ScorpionSecondTalent : Talent
{
    [SerializeField] private CleavingBlade_Scorpion cleavingBlade_Scorpion;

    public override void Enter()
    {
        cleavingBlade_Scorpion.CleavingBlade_ScorpionSecondTalent(true);
    }

    public override void Exit()
    {
        cleavingBlade_Scorpion.CleavingBlade_ScorpionSecondTalent(false);
    }
}
