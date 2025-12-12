using System;
using System.Collections;
using UnityEngine;

public class MultiMagicSpell : Skill
{
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
        if (Hero.CharacterState.CheckForState(States.MultiMagic)) Hero.CharacterState.CmdRemoveState(States.MultiMagic);
        else Hero.CharacterState.CmdAddState(States.MultiMagic, 9999, 0f, Hero.gameObject, name);
    }
}
