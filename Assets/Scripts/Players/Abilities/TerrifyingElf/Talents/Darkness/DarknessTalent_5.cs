using UnityEngine;

public class DarknessTalent_5 : Talent
{
    [SerializeField] private TerrifyingElfAura terrifyingElfAura;
    [SerializeField] private Silence silence;
    [SerializeField] private PullingHealth pullingHealth;

    public override void Enter()
    {
        terrifyingElfAura.ManaAbsorptionPhysicalTalentActive(true);
        terrifyingElfAura.SuppressionManaAbsorption(true);
        silence.SilenceEffectGhostCast(true);
        pullingHealth.SetPullingHealthGhostTalentActive(true);
        silence.SilenceEffectsOnMinionMagic(true);
    }

    public override void Exit()
    {
        terrifyingElfAura.ManaAbsorptionPhysicalTalentActive(false);
        terrifyingElfAura.SuppressionManaAbsorption(false);
        silence.SilenceEffectGhostCast(false);
        pullingHealth.SetPullingHealthGhostTalentActive(false);
        silence.SilenceEffectsOnMinionMagic(false);
    }
}
