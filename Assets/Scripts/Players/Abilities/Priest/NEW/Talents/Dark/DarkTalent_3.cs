using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkTalent_3 : Talent
{
    [SerializeField] private StunMagicPassiveSkill stunMagicPassiveSkill;

    public override void Enter()
    {
        stunMagicPassiveSkill.FillingDestruction(true);
    }

    public override void Exit()
    {
        stunMagicPassiveSkill.FillingDestruction(false);
    }
}
