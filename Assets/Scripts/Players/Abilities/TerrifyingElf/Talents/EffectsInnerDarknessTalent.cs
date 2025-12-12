using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsInnerDarknessTalent : Talent
{
    [SerializeField] private Silence silence;
    [SerializeField] private PullingHealth pulling;
    [SerializeField] private Ghost ghost;

    public override void Enter()
    {
        silence.EffectsInnerDarknessTalentActive(true);
        //pulling.EffectsInnerDarknessTalentActive(true);

        ghost.EffectsInnerDarknessTalentActive(true);

        foreach (var ghost in ghost.GhostTarget) if (ghost.TryGetComponent<GhostAura>(out var aura)) aura.EffectsInnerDarknessTalent = true; // убрать в будущем так как талант будет включатся всегда беред боем
    }

    public override void Exit()
    {
        silence.EffectsInnerDarknessTalentActive(false);
        //pulling.EffectsInnerDarknessTalentActive(false);
        ghost.EffectsInnerDarknessTalentActive(false);

        foreach (var ghost in ghost.GhostTarget) if (ghost.TryGetComponent<GhostAura>(out var aura)) aura.EffectsInnerDarknessTalent = false; // убрать в будущем
    }
}
