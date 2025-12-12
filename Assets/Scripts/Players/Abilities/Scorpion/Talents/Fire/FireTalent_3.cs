using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTalent_3 : Talent
{
    [SerializeField] private ScorpionPassive scorpionPassive;

    public override void Enter()
    {
        scorpionPassive.ImpulseMatter(true);
    }

    public override void Exit()
    {
        scorpionPassive.ImpulseMatter(false);
    }
}
