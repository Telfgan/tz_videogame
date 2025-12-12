using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingWithEnemyTalent : Talent
{
    [SerializeField] private IceRolling iceRolling;

    public override void Enter()
    {
        iceRolling.RollingWithEnemyTalentActive(true);
    }

    public override void Exit()
    {
        iceRolling.RollingWithEnemyTalentActive(false);
    }
}
