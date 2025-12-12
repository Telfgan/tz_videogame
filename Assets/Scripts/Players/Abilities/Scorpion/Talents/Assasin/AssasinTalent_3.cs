using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssasinTalent_3 : Talent
{
    [SerializeField] private ScorpionPassive scorpionPassive;

    public override void Enter()
    {
        scorpionPassive.AddStateUpdateChance(true);
    }

    public override void Exit()
    {
        scorpionPassive.AddStateUpdateChance(false);
    }
}
