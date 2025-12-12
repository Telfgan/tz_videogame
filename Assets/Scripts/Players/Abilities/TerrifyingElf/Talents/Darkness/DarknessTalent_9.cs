using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarknessTalent_9 : Talent
{
    [SerializeField] private Ghost ghost;

    public override void Enter()
    {
        ghost.GhostSpawnInRadiusTree(true);
    }

    public override void Exit()
    {
        ghost.GhostSpawnInRadiusTree(false);
    }
}
