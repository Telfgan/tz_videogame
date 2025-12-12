using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestOfEnergy : Skill
{
    [SerializeField] private float rune = 1;
    [SerializeField] private HarvestOfRunes harvestOfRunes;

    protected override int AnimTriggerCastDelay => Animator.StringToHash("SpellCastDelayAnimTrigger");
    protected override int AnimTriggerCast => 0;
    protected override bool IsCanCast => true;

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        if (targetInfo == null) return;
        if (targetInfo.GetTargets().Contains(Hero)) return;
        targetInfo.AddTarget(Hero);
    }

    protected override void ClearData() { }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        TargetInfo targetInfo = new TargetInfo();
        targetInfo.AddTarget(Hero);
        callbackDataSaved(targetInfo);

        yield break;
    }

    protected override IEnumerator CastJob()
    {
        if (Hero == null || Hero.CharacterState == null) yield break;
        AddRune();
    }

    private void AddRune()
    {
        if (Hero.TryGetResource(ResourceType.Rune) is RuneComponent runeAdd) runeAdd.CmdAdd(rune);
        if (harvestOfRunes != null) harvestOfRunes.IncreaseSetCooldown(harvestOfRunes.CooldownTime);
    }
}
