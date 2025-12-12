using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisciplineTalent_4 : Talent
{
    [SerializeField] private PriestShield _priestShield;
    [SerializeField] private SoulAid _soulAid;

    public override void Enter()
    {
        _priestShield.EnableTalentPhysicalShieldBoost(true);
        _priestShield.EnableDisciplineShieldBoost(true);
        _soulAid.EnableCooldownReduce(true);
    }

    public override void Exit()
    {
        _priestShield.EnableTalentPhysicalShieldBoost(false);
        _priestShield.EnableDisciplineShieldBoost(false);
        _soulAid.EnableCooldownReduce(false);
    }
}
