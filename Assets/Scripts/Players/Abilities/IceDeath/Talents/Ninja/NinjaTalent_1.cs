using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjaTalent_1 : Talent
{
    [SerializeField] private IceRolling iceRolling;
    [SerializeField] private SkillManager manager;

    public override void Enter()
    {
        manager.ActivateSkill(iceRolling);
        iceRolling.RollingWithEnemyTalentActive(true);
    }

    public override void Exit()
    {
        manager.DeactivateSkill(iceRolling);
        iceRolling.RollingWithEnemyTalentActive(false);
    }
}
