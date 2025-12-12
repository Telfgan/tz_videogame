using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolutionTalent_4 : Talent
{
    [SerializeField] private CheliceraStrike cheliceraStrike;
    [SerializeField] private JumpWithChelicera jumpWithChelicera;

    public override void Enter()
    {
        cheliceraStrike.CheliceraStrikeChanceDamageCrit(true);
        jumpWithChelicera.JumpWithCheliceraChanceDamageCrit(true);
    }

    public override void Exit()
    {
        cheliceraStrike.CheliceraStrikeChanceDamageCrit(false);
        jumpWithChelicera.JumpWithCheliceraChanceDamageCrit(false);
    }
}
