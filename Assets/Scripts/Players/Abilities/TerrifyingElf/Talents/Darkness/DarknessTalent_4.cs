using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarknessTalent_4 : Talent
{
    [SerializeField] private Ghost ghost;
    [SerializeField] private PullingHealth pullingHealth;

    public override void Enter()
    {
        ghost.SendingGhostTargetTalentActive(true);
        ghost.CooldownGhostShotActiveTalent(true);
        pullingHealth.PullingHealthSpeedWithFearTalentActive(true);
    }

    public override void Exit()
    {
        ghost.SendingGhostTargetTalentActive(false);
        ghost.CooldownGhostShotActiveTalent(false);
        pullingHealth.PullingHealthSpeedWithFearTalentActive(false);
    }
}
