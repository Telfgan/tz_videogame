using Mirror;
using System;
using System.Collections;
using UnityEngine;

public class SleepSpell : Skill
{
    [SerializeField] private Character _playerLinks;
    [SerializeField] private float duration;

    //private Character _target;
    //private Character _runtimeTarget;
    private Vector3 _targetPoint = Vector3.positiveInfinity;
    private bool _isSleepInnerDarknessTalentActive = false;

    protected override bool IsCanCast => IsHaveCharge && GetTargetCharacter() != null && Vector3.Distance(GetTargetCharacter().transform.position, transform.position) <= Radius;
    protected override int AnimTriggerCastDelay => Animator.StringToHash("SpellCastDelayAnimTrigger");
    protected override int AnimTriggerCast => 0;

    public bool IsSleepInnerDarknessTalentActive { get => _isSleepInnerDarknessTalentActive; set => _isSleepInnerDarknessTalentActive = value; }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        var multiMagic = Hero.CharacterState.GetState(States.MultiMagic) as MultiMagic;

        while (GetTargetCharacter() == null && !_disactive)
        {
            if (GetMouseButton)
            {
                FindTargetCharacter();
                //_target = GetRaycastTarget(true);
                //_runtimeTarget = GetTarget();
                if (multiMagic != null) multiMagic.LastTarget = GetTargetCharacter();
            }
            yield return null;
        }

        TargetInfo targetInfo = new TargetInfo();
        targetInfo.AddTarget(GetTargetCharacter());
        callbackDataSaved(targetInfo);
    }

    protected override IEnumerator CastJob()
    {
        if (GetTargetCharacter() != null) CmdApplyAbsorptionState(GetTargetCharacter().gameObject);

        var multiMagic = Hero.CharacterState.GetState(States.MultiMagic) as MultiMagic;

        if (multiMagic != null)
        {
            foreach (var character in multiMagic.PopPendingTargets())
            {
                TryPayCost();
                CmdApplyAbsorptionState(character.gameObject);
            }
        }

        AfterCastJob();

        yield return null;
    }

    protected override void ClearData()
    {
        ClearTarget();
        //_target = null;
    }

    [Command]
    private void CmdApplyAbsorptionState(GameObject targetGameObject)
    {
        var targetCharacter = targetGameObject.GetComponent<Character>();
        if (targetCharacter != null)
        {
            targetCharacter.CharacterState.AddState(States.Sleep, duration, 0, _playerLinks.gameObject, name);
        }
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        if (targetInfo.GetTargets().Count > 0) SetTarget(targetInfo.GetTargets()[0] as Character);
    }

    #region Talent

    public void SleepInnerDarknessTalent(bool value) => _isSleepInnerDarknessTalentActive = value;

    #endregion
}
