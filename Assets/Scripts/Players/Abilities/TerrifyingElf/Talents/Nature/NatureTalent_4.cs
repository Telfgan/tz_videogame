using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NatureTalent_4 : Talent
{
    [SerializeField] private ShotsIntoSky shotsIntoSky;
    [SerializeField] private GrowTree growTree;
    [SerializeField] private SkillManager _ability;

    public override void Enter()
    {
        _ability.ActivateSkill(shotsIntoSky);
        growTree.ShotTreeCooldownTalent(true);
        growTree.GrowTreeArrowIntoSkyRadius(true);
    }

    public override void Exit()
    {
        _ability.DeactivateSkill(shotsIntoSky);
        growTree.ShotTreeCooldownTalent(false);
        growTree.GrowTreeArrowIntoSkyRadius(false);
    }
}
