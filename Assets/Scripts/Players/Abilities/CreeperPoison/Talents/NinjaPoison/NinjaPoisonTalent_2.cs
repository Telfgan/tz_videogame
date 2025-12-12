using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjaPoisonTalent_2 : Talent
{
    [SerializeField] private SneakySpit sneakySpit;
    [SerializeField] private SkillManager skillManager;

    public override void Enter()
    {
        skillManager.ActivateSkill(sneakySpit);
    }

    public override void Exit()
    {
        skillManager.DeactivateSkill(sneakySpit);
    }
}
