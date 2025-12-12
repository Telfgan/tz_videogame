using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmTalent_3 : Talent
{
    [SerializeField] private Tentacles tentacles;

    public override void Enter()
    {
        tentacles.AttractionTentacleTalent(true);
    }

    public override void Exit()
    {
        tentacles.AttractionTentacleTalent(false);
    }
}
