using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleportation_ScorpionMagResist : Talent
{
    [SerializeField] private Teleportation_Scorpion teleportation_Scorpion;

    public override void Enter()
    {
        teleportation_Scorpion.Teleportation_ScorpionMagResist(true);
    }

    public override void Exit()
    {
        teleportation_Scorpion.Teleportation_ScorpionMagResist(false);
    }
}
