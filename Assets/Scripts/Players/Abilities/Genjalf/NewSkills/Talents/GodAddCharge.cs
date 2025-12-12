using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodAddCharge : Talent
{
    [SerializeField] private Gangdollarff.ClapOfLight _clapOfLight;
    [SerializeField] private Gangdollarff.Fisura _fisura;
    [SerializeField] private Gangdollarff.AbsorptionBall _absorptionBall;

    public override void Enter()
    {
        _clapOfLight.AddMaxChargeCount();
        _fisura.AddMaxChargeCount();
        _absorptionBall.AddMaxChargeCount();
    }

    public override void Exit()
    {
        _clapOfLight.DeductMaxChargeCount();
        _fisura.DeductMaxChargeCount();
        _absorptionBall.DeductMaxChargeCount();
    }
}
