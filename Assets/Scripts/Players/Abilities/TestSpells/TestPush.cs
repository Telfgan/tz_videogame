using Mirror;
using System;
using System.Collections;
using UnityEngine;

public class TestPush : Skill
{
    [SerializeField] private float _pushDistance = 5f;

    private Character _target;
    private GameObject _tempTarget1;
    private MoveComponent _tempTargetMove;

    protected override bool IsCanCast
    {
        get
        {
            if (_target == null)
                return false;

            return NoObstacles(_target.transform.position, _obstacle) && IsTargetInRadius(Radius, _target.transform); ;
        }
    }

    protected override int AnimTriggerCastDelay => throw new System.NotImplementedException();

    protected override int AnimTriggerCast => throw new System.NotImplementedException();

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        _target = (Character)targetInfo.GetTargets()[0];
    }

    protected override IEnumerator CastJob()
    {
        float time = 3;

        while(time > 0)
        {
            time -= Time.deltaTime;
            CmdPush(_target.gameObject, (_target.transform.position - transform.position).normalized * 2 * Time.deltaTime);
            yield return null;
        }
    }

    protected override void ClearData()
    {
        _target = null;
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        while(_target == null)
        {
            if (GetMouseButton)
            {
                //_target = GetRaycastTarget(true);
            }
            yield return null;
        }
        TargetInfo targetInfo = new();
        targetInfo.AddTarget(_target);
        callbackDataSaved(targetInfo);
    }

    [Command]
    private void CmdPush(GameObject gameObject, Vector2 force)
    {
        if (_tempTarget1 != gameObject)
        {
            _tempTarget1 = gameObject;
            _tempTargetMove = gameObject.GetComponent<MoveComponent>();
        }
        _tempTargetMove.TargetRpcAddTransformPosition(force);
    }
}
