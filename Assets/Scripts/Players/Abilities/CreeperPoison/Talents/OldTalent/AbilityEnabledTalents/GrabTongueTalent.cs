using UnityEngine;

public class GrabTongueTalent : Talent
{
    [SerializeField] private GrabTongue _grabTongue;
    [SerializeField] private SkillManager _ability;
    public override void Enter()
    {
        if (_ability.Abilities.Contains(_grabTongue))
        {
            _grabTongue.enabled = true;
        }
        else
        {
            //_ability.AddAbility(_grabTongue);
        }
    }

    public override void Exit()
    {
        if (_ability.Abilities.Contains(_grabTongue))
        {
            //_ability.RemoveAbility(_grabTongue);
            _grabTongue.enabled = false;
        }
        else
        {
            _grabTongue.enabled = false;
        }
    }
}
