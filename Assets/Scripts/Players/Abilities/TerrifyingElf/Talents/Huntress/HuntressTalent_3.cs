using Mirror;
using UnityEngine;

public class HuntressTalent_3 : Talent
{
    [SerializeField] private TerrifyingElfAura terrifyingElfAura;

    public override void Enter()
    {
        //terrifyingElfAura.HuntressMarkPhysicsTalentActive(true);
        terrifyingElfAura.ElvenSkillPhysicsTalent(true);
    }

    public override void Exit()
    {
        //terrifyingElfAura.HuntressMarkPhysicsTalentActive(false);
        terrifyingElfAura.ElvenSkillPhysicsTalent(false);
    }
}
