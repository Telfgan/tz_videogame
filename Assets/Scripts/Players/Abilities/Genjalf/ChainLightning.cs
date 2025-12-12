using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChainLightning : Skill
{
    [SerializeField] private ParticleSystem _particlePref;
    [SerializeField, Range(0, 100)] private int _debuffChance = 15;

    //private Character _target;

    protected override bool IsCanCast { get => CheckCanCast(); }

    protected override int AnimTriggerCastDelay => 0;

    protected override int AnimTriggerCast => 0;

    private bool CheckCanCast()
    {
        return
               Vector3.Distance(GetTargetCharacter().transform.position, transform.position) <= Radius;
    }

    public void AnimCastLight()
    {
        AnimStartCastCoroutine();
    }

    public void AnimLightEnd()
    {
        AnimCastEnded();
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        SetTarget((Character)targetInfo.GetTargets()[0]);
    }

    protected override IEnumerator CastJob()
    {
        if (GetTargetCharacter() != null)
        {
            Attack(GetTargetCharacter());
            yield return new WaitForSecondsRealtime(0.3f);
            var temps = Physics.OverlapSphere(GetTargetCharacter().Position, Radius, _targetsLayers);
            
            for (int i = 0; i < temps.Length; i++)
            {
                if (i <= 5 && temps[i].TryGetComponent(out Character character))
                {
                    Attack(character);
                    yield return new WaitForSecondsRealtime(0.3f);
                }
            }
        }
        yield return null;
    }

    protected override void ClearData()
    {
        ClearTarget();
        //_target = null;
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        TargetInfo targetInfo = new TargetInfo();

        while (GetTargetCharacter() == null)
        {
            if (GetMouseButton)
            {
                FindTargetCharacter();
                //_target = GetRaycastTarget();
            }
            yield return null;
        }

        targetInfo.AddTarget(GetTargetCharacter());
        callbackDataSaved(targetInfo);
    }

    private void Attack(Character target)
    {
        Damage damage = new Damage
        {
            Value = Buff.Damage.GetBuffedValue(Damage),
            Type = DamageType,
            PhysicAttackType = AttackRangeType,
        };
        CmdApplyDamage(damage, target.gameObject);

        CmdCreateParticle(target.Position);
    }

    private void CreateParticle(Vector3 position)
    {
        GameObject item = Instantiate(_particlePref.gameObject, position, Quaternion.identity);
    }

    [Command]
    protected void CmdCreateParticle(Vector3 position)
    {
        RpcCreateParticle(position);
    }

    [ClientRpc]
    private void RpcCreateParticle(Vector3 position)
    {
        CreateParticle(position);
    }
}
