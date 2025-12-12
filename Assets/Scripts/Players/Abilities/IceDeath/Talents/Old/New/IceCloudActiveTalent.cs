using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcePuddleActiveTalent : Talent
{
    [SerializeField] private IcePuddle icePuddle;
    [SerializeField] private IceCloud iceCloud;
    [SerializeField] private SkillManager skillManager;

    public override void Enter()
    {
        skillManager.ActivateSkill(icePuddle);
        skillManager.ActivateSkill(iceCloud);
    }

    public override void Exit()
    {
        skillManager.DeactivateSkill(icePuddle);
        skillManager.DeactivateSkill(iceCloud);
    }
}
