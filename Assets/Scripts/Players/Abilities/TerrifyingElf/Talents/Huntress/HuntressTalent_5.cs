using UnityEngine;

public class HuntressTalent_5 : Talent
{
    [SerializeField] private ReconnaissanceFire reconnaissanceFire;
    [SerializeField] private TerrifyingElfAura terrifyingElfAura;
    [SerializeField] private GroundTrap groundTrap;

    public override void Enter()
    {
        reconnaissanceFire.partialBlindnessTalentActive(true);
        terrifyingElfAura.ElvenSkillTalent(true);
        groundTrap.GroundTrapHealthActiveTalent(true);
        reconnaissanceFire.FireWorshipperTalentActive(true);
        terrifyingElfAura.CalmnessOnElvenSkillTalent(true);
    }

    public override void Exit()
    {
        reconnaissanceFire.partialBlindnessTalentActive(false);
        terrifyingElfAura.ElvenSkillTalent(false);
        groundTrap.GroundTrapHealthActiveTalent(false);
        reconnaissanceFire.FireWorshipperTalentActive(false);
        terrifyingElfAura.CalmnessOnElvenSkillTalent(false);
    }
}
