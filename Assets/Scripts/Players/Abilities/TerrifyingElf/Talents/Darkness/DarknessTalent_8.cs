using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarknessTalent_8 : Talent
{
    [SerializeField] private TerrifyingElfAura terrifyingElfAura;
    [SerializeField] private Ghost ghost;

    public override void Enter()
    {
        ghost.PullingHealthGostTeleport(true);
        terrifyingElfAura.ReductionRecharge(true);
    }

    public override void Exit()
    {
        ghost.PullingHealthGostTeleport(false);
        terrifyingElfAura.ReductionRecharge(false);
    }
}
