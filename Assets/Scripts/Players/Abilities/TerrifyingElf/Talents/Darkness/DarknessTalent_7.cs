using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarknessTalent_7 : Talent
{
    [SerializeField] private SubjugationMind subjugationMind;
    [SerializeField] private SleepSpell sleep;
    [SerializeField] private SkillManager ability;
    [SerializeField] private Ghost ghost;

    public override void Enter()
    {
        ghost.PassingThroughGhost(true);
        ability.ActivateSkill(subjugationMind);
        sleep.SleepInnerDarknessTalent(true);
    }

    public override void Exit()
    {
        ghost.PassingThroughGhost(false);
        ability.DeactivateSkill(subjugationMind);
        sleep.SleepInnerDarknessTalent(true);
    }
}
