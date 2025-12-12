using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeriesCompliteCompoTalent : Talent
{
    [SerializeField] private SeriesOfStrikes seriesOfStrikes;

    public override void Enter()
    {
        seriesOfStrikes.SeriesCompliteCompoTalentActive(true);
    }

    public override void Exit()
    {
        seriesOfStrikes.SeriesCompliteCompoTalentActive(false);
    }
}
