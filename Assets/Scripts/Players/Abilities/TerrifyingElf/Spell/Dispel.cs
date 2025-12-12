using Mirror;
using System;
using System.Collections;
using UnityEngine;

public class Dispel : Skill
{
    //private Character _target;
    private Vector3 _targetPoint = Vector3.positiveInfinity;

    protected override bool IsCanCast => GetTargetCharacter() != null && Vector3.Distance(GetTargetCharacter().transform.position, transform.position) <= Radius;

    protected override int AnimTriggerCastDelay => Animator.StringToHash("Dispel");
    protected override int AnimTriggerCast => 0;

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        while (float.IsPositiveInfinity(_targetPoint.x) && GetTargetCharacter() == null)
        {
            if (GetMouseButton)
            {
                _targetPoint = GetMousePoint();
                FindTargetCharacter();
                //_target = GetNearestTargetInRadius();
            }
            yield return null;
        }

        TargetInfo targetInfo = new TargetInfo();
        targetInfo.Points.Add(_targetPoint);
        callbackDataSaved(targetInfo);
    }

    protected override IEnumerator CastJob()
    {
        if (GetTargetCharacter() == null) yield break;

        if (GetTargetCharacter() is MinionComponent minionTarget) ApplyDamageToMinion(minionTarget);

        var targetCharacter = GetTargetCharacter().GetComponent<CharacterState>();
        if (targetCharacter != null)
        {
            //CmdDispelState(targetCharacter, _target.NetworkSettings.TeamIndex, Hero.NetworkSettings.TeamIndex);
            CmdDispelState(targetCharacter);
        }

        yield return null;
    }

    protected override void ClearData()
    {
        ClearTarget();
        //_target = null;
    }

    private void ApplyDamageToMinion(MinionComponent minionTarget)
    {
        float damageValue = Damage;

        if (minionTarget.Data.Type == "Magic")
        {
            damageValue *= 2;
        }

        Damage damage = new Damage
        {
            Value = damageValue,
            Type = DamageType.Magical,
        };

        CmdApplyDamage(minionTarget.gameObject, damage);
    }

    private Character GetNearestTargetInRadius()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, Radius, TargetsLayers);
        Character nearestTarget = null;
        float shortestDistance = Radius;

        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent(out Character character) && character != Hero)
            {
                float distance = Vector3.Distance(transform.position, character.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestTarget = character;
                }
            }
        }
        return nearestTarget;
    }

    [Command]
    private void CmdApplyDamage(GameObject targetObject, Damage damage)
    {
        if (targetObject.TryGetComponent<IDamageable>(out IDamageable damageable)) damageable.TryTakeDamage(ref damage, null);
    }

    //[Command]
    //private void CmdDispelState(CharacterState targetState, int targetTeamIndex, int playerTeamIndex)
    //{
    //    if (targetState == null) return;

    //    targetState.DispelStates(StateType.Magic, targetTeamIndex, playerTeamIndex, true);
    //}

    [Command]
    private void CmdDispelState(CharacterState targetState)
    {
        if (targetState == null) return;

        bool isAlly = targetState.gameObject.layer == LayerMask.NameToLayer("Allies");

        targetState.DispelStates(StateType.Magic, isAlly, true);
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        _targetPoint = targetInfo.Points[0];
    }
}
