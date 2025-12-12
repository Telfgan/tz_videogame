using Mirror;
using System;
using System.Collections;
using UnityEngine;

public class RetributiveReckoning : AutoAttackSkill
{
    [SerializeField] private Health health;
    [SerializeField] private MoveComponent moveComponent;
    [SerializeField] private float disactiveResetTime = 1f;
    [SerializeField] private float durationState = 3f;

    private Character _lastAttacker;
    private Coroutine _disactiveResetCoroutine;
    private bool _isTeleporting;

    protected override bool IsCanCast => _lastAttacker != null && IsTargetInRadius(Radius, _lastAttacker.transform);
    protected override int AnimTriggerAutoAttack => 0;
    protected override int AnimTriggerCastDelay => Animator.StringToHash("RetributiveReckoningCastDelay");

    private void Start()
    {
        health.DamageTaken += OnDamageTaken;
    }

    private void OnDestroy()
    {
        health.DamageTaken -= OnDamageTaken;
    }

    private void OnDamageTaken(Damage damage, Skill skill)
    {
        if (skill == null || skill.Hero == null) return;

        Character attacker = skill.Hero as Character;
        if (attacker == null || !IsBackAttack(attacker)) return;

        CmdOnDamageTaken(attacker);
    }

    private bool IsBackAttack(Character attacker)
    {
        Vector3 directionToAttacker = (attacker.transform.position - transform.position).normalized;
        Vector3 forwardDirection = moveComponent.transform.forward.normalized;
        float angle = Vector3.Angle(forwardDirection, directionToAttacker);

        return angle > 120 && Vector3.Distance(attacker.transform.position, transform.position) <= Radius;
    }

    private IEnumerator ResetDisactiveAfterDelay()
    {
        yield return new WaitForSeconds(disactiveResetTime);
        _lastAttacker = null;
        Disactive = true;
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        while (!Disactive)
        {
            if (_lastAttacker != null && IsTargetInRadius(Radius, _lastAttacker.transform))
            {
                if (IsAutoattackMode)
                {
                    CastAction();
                }
                else if (GetMouseButton)
                {
                    CmdTeleportPlayer(GetBehindPosition(_lastAttacker));
                    PayTeleportCost();
                }
            }
            yield return null;
        }
    }

    protected override IEnumerator CastJob()
    {
        if (_lastAttacker == null)
        {
            yield break;
        }

        if (!IsTargetInRadius(Radius, _lastAttacker.transform))
        {
            yield break;
        }

        _isTeleporting = true;

        Vector3 behindPosition = GetBehindPosition(_lastAttacker);
        CmdTeleportPlayer(behindPosition);

        yield return new WaitForSeconds(AttackDelay / 2);

        if (_lastAttacker != null && _lastAttacker.TryGetComponent<CharacterState>(out var state)) state.CmdAddState(States.Fear, durationState, 0, gameObject, "RetributiveReckoning");

        yield return new WaitForSeconds(AttackDelay / 2);

        _isTeleporting = false;
    }

    protected override void CastAction()
    {

    }

    private Vector3 GetBehindPosition(Character enemy)
    {
        Vector3 directionToEnemy = (transform.position - enemy.transform.position).normalized;
        return enemy.transform.position - directionToEnemy * Radius;
    }

    private void PayTeleportCost()
    {
        TryPayCost();
    }

    [ClientRpc]
    private void RpcActivateSkillOnClients(Character attacker)
    {
        _lastAttacker = attacker;
        Disactive = false;

        if (_disactiveResetCoroutine != null)
            StopCoroutine(_disactiveResetCoroutine);

        _disactiveResetCoroutine = StartCoroutine(ResetDisactiveAfterDelay());
    }

    [Command]
    private void CmdTeleportPlayer(Vector3 position)
    {
        RpcTeleportPlayer(position);
        moveComponent.TeleportToPositionSmooth(position, AttackDelay);
    }

    [Command]
    private void CmdOnDamageTaken(Character attacker)
    {
        if (attacker == null || !IsBackAttack(attacker)) return;

        RpcActivateSkillOnClients(attacker);
    }

    [ClientRpc]
    private void RpcTeleportPlayer(Vector3 position)
    {
        _hero.Animator.SetTrigger(AnimTriggerCastDelay);
    }

    protected override void ClearData()
    {
        _isTeleporting = false;
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        throw new NotImplementedException();
    }
}
