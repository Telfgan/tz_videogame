using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntressTalent_6 : Talent
{
    [SerializeField] private ShotIntoSky shotIntoSky;
    [SerializeField] private ReconnaissanceFire reconnaissanceFire;
    [SerializeField] private TerrifyingElfAura terrifyingElfAura;

    public override void Enter()
    {
        reconnaissanceFire.SkillEnableBoostLogicActiveTalent(true);
        terrifyingElfAura.ElvenSkillPhysDamageHealthChance(true);
        shotIntoSky.ShotRadiusUpgradeActive(true);
        reconnaissanceFire.ReconnaissanceFireHealthTalentActive(true);
    }

    public override void Exit()
    {
        reconnaissanceFire.SkillEnableBoostLogicActiveTalent(false);
        terrifyingElfAura.ElvenSkillPhysDamageHealthChance(false);
        shotIntoSky.ShotRadiusUpgradeActive(false);
        reconnaissanceFire.ReconnaissanceFireHealthTalentActive(false);
    }
}
