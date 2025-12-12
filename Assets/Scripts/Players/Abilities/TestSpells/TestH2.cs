using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestH2 : Skill
{
    [SerializeField] private Projectile _projectile;
    [SerializeField] private float _damage;
    [SerializeField] private int _projectileCount;
    [SerializeField] private float _spawnDeley;

    //private Character _target;
	private Vector3 _targetPoint;

	protected override bool IsCanCast
    {
        get
        {
            if(GetTargetCharacter() != null)
                return Vector3.Distance(GetTargetCharacter().transform.position, transform.position) <= Radius;

            return false;
        }
    }

    protected override int AnimTriggerCastDelay => Animator.StringToHash("H2CastDelay");

    protected override int AnimTriggerCast => 0;

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        _targetPoint = targetInfo.Points[0];
        SetTarget((Character)targetInfo.GetTargets()[0]);
    }

    protected override IEnumerator CastJob()
    {
        Damage damage = new Damage
        {
            Value = Buff.Damage.GetBuffedValue(_damage),
            Type = DamageType,
            PhysicAttackType = AttackRangeType,
        };
        CmdApplyDamage(damage, GetTargetCharacter().gameObject);

        var deley = new WaitForSeconds(_spawnDeley); ;

        for (int i = 0; i < _projectileCount; i++)
        {
            float angle = i * 2 * Mathf.PI / _projectileCount;

            float x = Mathf.Cos(angle);
            float y = Mathf.Sin(angle);

            Vector3 point = new Vector3(x, y, 0) + GetTargetCharacter().transform.position;

            CmdCreateProjecttile(point, GetTargetCharacter().transform.position);
            yield return deley;
        }
        yield return null;
    }

    protected override void ClearData()
    {
        ClearTarget();
        //_targetPoint = 
        //_target = null;
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        while (GetTargetCharacter() == null)
        {
            if (GetMouseButton)
            {
                FindTargetCharacter();
				_targetPoint = GetMousePoint();
				//_target = GetRaycastTarget(true);
            }
            yield return null;
        }
        TargetInfo targetInfo = new();
        targetInfo.Points.Add(_targetPoint);
        targetInfo.AddTarget(GetTargetCharacter());
        callbackDataSaved(targetInfo);
    }

    [Command]
    protected void CmdCreateProjecttile(Vector3 pointToflay, Vector3 spawnPoint)
    {
        GameObject item = Instantiate(_projectile.gameObject, spawnPoint, Quaternion.identity);

        SceneManager.MoveGameObjectToScene(item, _hero.NetworkSettings.MyRoom);

        item.GetComponent<Projectile>().StartFly(pointToflay, true);

        NetworkServer.Spawn(item);
    }
}
