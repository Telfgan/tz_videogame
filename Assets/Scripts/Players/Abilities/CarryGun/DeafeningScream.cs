using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeafeningScream : Skill
{
    [SerializeField] private Character _playerLinks;
    [SerializeField] private JumpWithChelicera jumpWithChelicera;
    [SerializeField] private CooldownEnergy cooldownEnergy;
    [SerializeField] private float duration = 2f;

    //private IDamageable _target;
    private Character _runtimeTarget;

    protected override bool IsCanCast => CheckCanCast();

    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerCast => Animator.StringToHash("DeafeningScreamAnimation");

    private void OnDestroy() => Canceled -= HandleJumpEnd;
    private void OnEnable() => Canceled += HandleJumpEnd;

    private bool CheckCanCast()
    {
        return GetTargetCharacter() != null && cooldownEnergy.CurrentValue >= jumpWithChelicera.ChargeCooldown &&
        Vector3.Distance(GetTargetCharacter().transform.position, transform.position) <= Radius &&
        NoObstacles(GetTargetCharacter().transform.position, transform.position, _obstacle);
    }

    public void HandleJumpEnd()
    {
        Hero.Animator.applyRootMotion = false;
        _playerLinks.Move.StopLookAt();
        Hero.Move.CanMove = true;
        _isCanCancle = true;
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        _runtimeTarget = null;

        while (GetTargetCharacter() == null)
        {
            if (GetMouseButton)
            {
                FindTargetCharacter();

                if (GetTargetCharacter() != null) if (GetTargetCharacter() is Character characterTarget) _runtimeTarget = characterTarget;
                _isCanCancle = false;
            }
            yield return null;
        }

        TargetInfo targetInfo = new TargetInfo();
        targetInfo.AddTarget(_runtimeTarget);
        callbackDataSaved(targetInfo);
    }

    protected override IEnumerator CastJob()
    {
        if (GetTargetCharacter() != null) CmdApplyState(GetTargetCharacter().gameObject);

        cooldownEnergy.CastCooldownEnergySkill(13, this);
        AfterCastJob();

        yield return null;
    }

    protected override void ClearData()
    {
        ClearTarget();
        //_target = null;
    }

    [Command]
    private void CmdApplyState(GameObject targetGameObject)
    {
        var targetCharacter = targetGameObject.GetComponent<Character>();
        if (targetCharacter != null)
        {
            targetCharacter.CharacterState.AddState(States.Stupefaction, duration, 0, _playerLinks.gameObject, name);
        }
    }

    public void DeafeningScreamAnimationMove()
    {
        if (_hero == null || _hero.Move == null) return;

        _hero.Move.StopMoveAndAnimationMove();
        _hero.Move.CanMove = false;
    }

    public void DeafeningScreamCast()
    {
        AnimStartCastCoroutine();
        DeafeningScreamAnimationMove();
        Hero.Animator.applyRootMotion = true;
    }

    public void DeafeningScreamEnd()
    {
        AnimCastEnded();
        HandleJumpEnd();
        _isCanCancle = true;
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        if (targetInfo.GetTargets().Count > 0) SetTarget(targetInfo.GetTargets()[0] as Character);
        Hero.Move.LookAtTransform(GetTargetCharacter().transform);
        _isCanCancle = false;
    }
}
