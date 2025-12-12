using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarknessTalent_10 : Talent
{
    [SerializeField] private Silence silence;

    public override void Enter()
    {
        silence.SilenceAddAllCharacterWithDeabaffElf(true);
    }

    public override void Exit()
    {
        silence.SilenceAddAllCharacterWithDeabaffElf(false);
    }
}
