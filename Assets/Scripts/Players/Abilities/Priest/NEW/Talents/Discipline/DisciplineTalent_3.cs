using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisciplineTalent_3 : Talent
{
    [SerializeField] private SparkOfLight spark;
    [SerializeField] private FlowOfLight flowOfLight;

    public override void Enter()
    {
        spark.SpiritEnergyAddTalent(true);
        flowOfLight.SpiritEnergyAddTalent(true);
    }

    public override void Exit()
    {
        spark.SpiritEnergyAddTalent(false);
        flowOfLight.SpiritEnergyAddTalent(false);
    }
}
