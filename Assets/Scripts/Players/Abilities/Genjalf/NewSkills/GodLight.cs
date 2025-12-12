using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodLight : Skill
{
    protected override int AnimTriggerCastDelay => 0;

    protected override int AnimTriggerCast => 0;

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        
    }

    protected override IEnumerator CastJob()
    {
        foreach (var item in Hero.Abilities.Abilities)
        {
            if (item is IGodLightSpell spell)
            {
                spell.ChangeMode();
                item.CastEnded += OnCastEnded;
            }
        } 

        yield return null;
    }

    private void OnCastEnded()
    {
        foreach (var item in Hero.Abilities.Abilities)
        {
            if (item is IGodLightSpell spell)
            {
                spell.ChangeMode();
                item.CastEnded -= OnCastEnded;
            }
        }
    }

    protected override void ClearData()
    {
        
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> targetDataSavedCallback)
    {
        yield return null;
    }
}
