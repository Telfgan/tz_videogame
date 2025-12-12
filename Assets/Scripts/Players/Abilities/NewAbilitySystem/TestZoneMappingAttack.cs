using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestZoneMappingAttack : Skill
{
    [SerializeField] private SkillRenderer skillRenderer;

    private Vector3 _targetPoint = Vector3.positiveInfinity;

    protected override bool IsCanCast => true;
    protected override int AnimTriggerCastDelay => throw new System.NotImplementedException();
    protected override int AnimTriggerCast => throw new System.NotImplementedException();

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        _targetPoint = targetInfo.Points[0];
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        while (float.IsPositiveInfinity(_targetPoint.x))
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 clickedPoint = GetMousePoint();

                if (IsPointInRadius(Radius, clickedPoint))
                {
                    _targetPoint = clickedPoint;
                }
            }
            yield return null;
        }
        TargetInfo targetInfo = new TargetInfo();
        targetInfo.Points.Add( _targetPoint );
        callbackDataSaved( targetInfo );
    }

    protected override IEnumerator CastJob()
    {
        DrawDamageZone(_targetPoint);

        yield return new WaitForSeconds(2f);
        //skillRenderer.CmdStopDrawDamageZone();
    }

    protected override void ClearData()
    {
        _targetPoint = Vector3.positiveInfinity;
    }
}

