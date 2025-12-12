using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpEarth : Talent
{
    [SerializeField] private float _increasePercentages = 1.1f;

    private WaitForSeconds _increaseManaRegenerationDeley;
    private Resource _mana;

    public override void Enter()
    {
        character.Health.ChangedMaxValue(character.Health.MaxValue * _increasePercentages);
    }

    public override void Exit()
    {
        character.Health.ChangedMaxValue(character.Health.MaxValue / _increasePercentages);
    }
}
