using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarknessTalent_6 : Talent
{
    [SerializeField] private RetributiveReckoning retributiveReckoning;
    [SerializeField] private Suppression suppression;
    [SerializeField] private PullingHealth pulling;
    [SerializeField] private SkillManager ability;
    [SerializeField] private ReconnaissanceFire reconnaissanceFire;

    public override void Enter()
    {
        ability.ActivateSkill(retributiveReckoning);
        ability.ActivateSkill(suppression);
        pulling.PullingHealthThroughGhosts(true);
        reconnaissanceFire.ReconnaissanceFireAuraDarknesActive(true);
    }

    public override void Exit()
    {
        ability.DeactivateSkill(retributiveReckoning);
        ability.DeactivateSkill(suppression);
        pulling.PullingHealthThroughGhosts(false);
        reconnaissanceFire.ReconnaissanceFireAuraDarknesActive(true);
    }
}
