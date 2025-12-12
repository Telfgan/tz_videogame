using UnityEngine;

public class InvisibleTalent : Talent
{
    [SerializeField] private CreeperInvisible _invisible;
    [SerializeField] private SkillManager _ability;
    public override void Enter()
    {
        if (_ability.Abilities.Contains(_invisible))
        {
            _invisible.enabled = true;
        }
        else
        {
            //_ability.AddAbility(_invisible);
        }
    }

    public override void Exit()
    {
        if (_ability.Abilities.Contains(_invisible))
        {
            //_ability.RemoveAbility(_invisible);
            _invisible.enabled = false;
        }
        else
        {
            _invisible.enabled = false;
        }
    }
}
