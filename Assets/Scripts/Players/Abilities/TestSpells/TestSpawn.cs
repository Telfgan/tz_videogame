using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestSpawn : Skill
{
    private Vector3 _position;

    protected override bool IsCanCast => true;

    protected override int AnimTriggerCastDelay => 0;

    protected override int AnimTriggerCast => 0;

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        _position = targetInfo.Points[0];
    }

    protected override IEnumerator CastJob()
    {
        Hero.SpawnComponent.CmdSpawnUnitInPoint(_position, 0);

        yield return null;
    }

    protected override void ClearData()
    {
        _position = Vector2.zero;
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        while(_position == Vector3.zero)
        {
            if (GetMouseButton)
            {
                _position = GetMousePoint();
            }
            yield return null;
        }
        TargetInfo targetInfo = new();
        targetInfo.Points.Add(_position);
        callbackDataSaved(targetInfo);
    }
}
