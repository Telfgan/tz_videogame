using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NatureTalent_9 : Talent
{
    [SerializeField] private SkillManager abilities;
    [SerializeField] private MultiMagicSpell multiMagicSpell;

    public override void Enter()
    {
        abilities.ActivateSkill(multiMagicSpell);
    }

    public override void Exit()
    {
        abilities.DeactivateSkill(multiMagicSpell);
    }
}
