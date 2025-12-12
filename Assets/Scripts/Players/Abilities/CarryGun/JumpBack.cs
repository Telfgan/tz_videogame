using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpBack : Skill
{
    [SerializeField] private CooldownEnergy cooldownEnergy;
    [SerializeField] private float jumpDistance = 1.2f;
    [SerializeField] private float jumpWindow = 1f;
    [SerializeField] private float cooldownEnergyCost = 4;

    private Coroutine _jumpWindowCoroutine;

    private static readonly int jumpStart = Animator.StringToHash("JumpBackStart");
    private static readonly int jumpEnd = Animator.StringToHash("JumpBackEnd");

    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerCast => jumpStart;
    private Vector3 _mousePosition = Vector3.positiveInfinity;

    protected override bool IsCanCast => true && cooldownEnergy.CurrentValue >= cooldownEnergyCost;

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        if (targetInfo == null) return;
        if (targetInfo.GetTargets().Contains(Hero)) return;
        targetInfo.AddTarget(Hero);
    }

    public void JumpBackAnimationMove()
    {
        if (_hero == null || _hero.Move == null) return;

        _hero.Move.StopMoveAndAnimationMove();
        _hero.Move.CanMove = false;

        Vector3 direction = _hero.transform.forward;
        bool badDirection = float.IsInfinity(_hero.transform.forward.x) || direction.sqrMagnitude < 0.0001f;

        if (badDirection)
        {
            _hero.Move.StopLookAt();
            return;
        }

        _hero.Move.LookAtPosition(_hero.transform.forward);
    }

    public void HandleJumpBackEnd()
    {
        Hero.Animator.applyRootMotion = false;
        Hero.Move.StopLookAt();
        Hero.Move.CanMove = true;
        _isCanCancle = true;
    }

    public void JumpBackCast() => AnimStartCastCoroutine();
    public void JumpBackEnd()
    {
        HandleJumpBackEnd();
        AnimCastEnded();
    }

    public void ApplyRootJumpBackTrue()
    {
        JumpBackAnimationMove();
        Hero.Animator.applyRootMotion = true;
    }

    public void EnableJumpBack()
    {
        Disactive = false;
        if (_jumpWindowCoroutine != null) StopCoroutine(_jumpWindowCoroutine);
        _jumpWindowCoroutine = StartCoroutine(JumpWindowCoroutine());
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        while (Disactive && float.IsPositiveInfinity(_mousePosition.x))
        {
            if (GetMouseButton) _mousePosition = GetMousePoint();
            yield return null;
        }


        TargetInfo targetInfo = new TargetInfo();
        targetInfo.AddTarget(Hero);
        callbackDataSaved(targetInfo);
    }

    protected override IEnumerator CastJob()
    {
        cooldownEnergy.CastCooldownEnergySkill(cooldownEnergyCost, this);
        Vector3 jumpDir = -_hero.transform.forward;
        Vector3 targetPos = _hero.transform.position + jumpDir * jumpDistance;

        float duration = jumpDistance / 2f;

        CmdJumpBack(targetPos);
        yield return new WaitForSeconds(duration);
    }

    protected override void ClearData()
    {
        _mousePosition = Vector3.positiveInfinity;
    }

    private IEnumerator JumpWindowCoroutine()
    {
        yield return new WaitForSeconds(jumpWindow);
        Disactive = true;
        _jumpWindowCoroutine = null;
    }

    [Command]
    private void CmdJumpBack(Vector3 targetPos)
    {
        Vector3 from = _hero.transform.position;
        float distance = Vector3.Distance(from, targetPos);
        float speed = 2f;
        float duration = Mathf.Max(0.01f, distance / speed);

        _hero.Move.TargetRpcDoMove(targetPos, duration);
        StartCoroutine(JumpBackEndServerCoroutine(duration));
    }

    private IEnumerator JumpBackEndServerCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration + 0.03f);
        RpcPlayJumpEnd();
    }

    [ClientRpc]
    private void RpcPlayJumpEnd()
    {
        if (Hero == null || Hero.Animator == null) return;

        Hero.Animator.SetTrigger(jumpEnd);
        Hero.NetworkAnimator.SetTrigger(jumpEnd);

        HandleJumpBackEnd();
    }
}