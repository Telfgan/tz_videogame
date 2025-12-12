using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkTalent_4 : Talent
{
    [SerializeField] private PriestShield _priestShield;

    public override void Enter()
    {
        _priestShield.EnableDarkMagicBoost(true);
    }

    public override void Exit()
    {
        _priestShield.EnableDarkMagicBoost(false);
    }
}
