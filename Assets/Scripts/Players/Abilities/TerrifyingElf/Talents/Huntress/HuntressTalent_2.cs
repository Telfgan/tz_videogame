using UnityEngine;

public class HuntressTalent_2 : Talent
{
    [SerializeField] private ReconnaissanceFire reconnaissanceFire;
    [SerializeField] private TerrifyingElfAura terrifyingElfAura;
    [SerializeField] private SkillManager ability;

    public override void Enter()
    {
        ability.ActivateSkill(reconnaissanceFire); 
        terrifyingElfAura.FireWorshipperTalentActive(true);
    }

    public override void Exit()
    {
        ability.DeactivateSkill(reconnaissanceFire);
        terrifyingElfAura.FireWorshipperTalentActive(false);
    }
}
