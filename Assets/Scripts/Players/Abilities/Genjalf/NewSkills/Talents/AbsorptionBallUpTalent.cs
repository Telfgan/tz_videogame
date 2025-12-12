using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbsorptionBallUpTalent : Talent
{
    [SerializeField] private Gangdollarff.AbsorptionBall _absorptionBall;

    private float _timeDel = 12;
    private float _time = 0;
    private float _currentTime = 0;

    public override void Enter()
    {
        _absorptionBall.ShieldDuration = 4;
        _time = Time.time + _timeDel;
        _currentTime = Time.time;

        _absorptionBall.PreparingStarted += OnPreparingStarted;
    }

    public override void Exit()
    {
        _absorptionBall.ShieldDuration = 2;
    }

    private void OnPreparingStarted(Skill skill)
    {
        if (_time < _currentTime)
        {
            _absorptionBall.ShieldDuration = 4;
        }
        else 
        {
            _absorptionBall.ShieldDuration = 2;
        }
        _time = Time.time + _timeDel;
        _currentTime = Time.time;
    }
}
