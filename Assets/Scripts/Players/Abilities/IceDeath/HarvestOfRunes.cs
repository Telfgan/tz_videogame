using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestOfRunes : Skill
{
    [SerializeField] private float enegry = 70;
    [SerializeField] private HarvestOfEnergy harvestOfEnergy;

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
        AddEnergy();
    }

    private void AddEnergy()
    {
        if (Hero.TryGetResource(ResourceType.Energy) is Energy energy) energy.CmdAdd(enegry);
        if (harvestOfEnergy != null) harvestOfEnergy.IncreaseSetCooldown(harvestOfEnergy.CooldownTime);
    }
}
