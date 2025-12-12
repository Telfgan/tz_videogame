using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjaScorpionTalent_3 : Talent
{
    [SerializeField] private NewPunch_Scorpion newPunch;

    public override void Enter()
    {
        newPunch.WarningUpAddState(true);
    }

    public override void Exit()
    {
        newPunch.WarningUpAddState(false);
    }
}
